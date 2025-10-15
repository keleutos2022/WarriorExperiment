#!/bin/bash

# Database configuration
CONTAINER_NAME="warrior-experiment-db"
DB_NAME="warrior_experiment"
DB_USER="postgres"
DB_PASSWORD="postgres"
DB_PORT="7777"
POSTGRES_VERSION="16"
BACKUP_DIR="./backups"

# Function to stop existing container
stop_existing_container() {
    echo "Stopping existing database container..."
    if podman ps -q -f name="$CONTAINER_NAME" | grep -q .; then
        podman stop "$CONTAINER_NAME"
        echo "Container stopped."
    else
        echo "No running container found."
    fi
    
    # Remove container if it exists
    if podman ps -a -q -f name="$CONTAINER_NAME" | grep -q .; then
        podman rm "$CONTAINER_NAME"
        echo "Container removed."
    fi
    
    # Check for any containers using the port and stop them
    echo "Checking for containers using port $DB_PORT..."
    # Check both running and stopped containers that might have port conflicts
    CONTAINERS_ON_PORT=$(podman ps -a --format "{{.Names}}" | xargs -I {} sh -c 'podman port {} 2>/dev/null | grep -q ":'$DB_PORT'->" && echo {}' | head -10)
    if [ ! -z "$CONTAINERS_ON_PORT" ]; then
        echo "Found containers using port $DB_PORT: $CONTAINERS_ON_PORT"
        echo "Stopping and removing containers on port $DB_PORT..."
        echo "$CONTAINERS_ON_PORT" | xargs -r podman stop 2>/dev/null || true
        echo "$CONTAINERS_ON_PORT" | xargs -r podman rm 2>/dev/null || true
    fi
}

# Function to create new container
create_new_container() {
    echo "Creating new PostgreSQL database container..."
    podman run -d \
        --name "$CONTAINER_NAME" \
        -e POSTGRES_DB="$DB_NAME" \
        -e POSTGRES_USER="$DB_USER" \
        -e POSTGRES_PASSWORD="$DB_PASSWORD" \
        -p "$DB_PORT":5432 \
        postgres:"$POSTGRES_VERSION"
    
    if [ $? -eq 0 ]; then
        echo "Database container created successfully!"
        echo "Connection details:"
        echo "  Host: localhost"
        echo "  Port: $DB_PORT"
        echo "  Database: $DB_NAME"
        echo "  Username: $DB_USER"
        echo "  Password: $DB_PASSWORD"
        
        echo ""
        echo "Waiting for database to be ready..."
        sleep 5
        
        # Test connection
        podman exec "$CONTAINER_NAME" pg_isready -U "$DB_USER" -d "$DB_NAME"
        if [ $? -eq 0 ]; then
            echo "Database is ready!"
        else
            echo "Database might still be starting up. Please wait a moment and try connecting."
        fi
    else
        echo "Failed to create database container!"
        exit 1
    fi
}

# Function to create backup
create_backup() {
    echo "=== Creating Database Backup ==="
    
    # Create backup directory if it doesn't exist
    mkdir -p "$BACKUP_DIR"
    
    # Generate backup filename with timestamp
    BACKUP_FILE="$BACKUP_DIR/wa_backup_$(date +%Y%m%d_%H%M%S).sql"
    
    echo "Creating backup: $BACKUP_FILE"
    
    # Set password for pg_dump
    export PGPASSWORD="$DB_PASSWORD"
    
    # Create backup using pg_dump
    pg_dump -h localhost -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" \
        --verbose --clean --create --if-exists \
        --file="$BACKUP_FILE"
    
    if [ $? -eq 0 ]; then
        echo "Backup created successfully: $BACKUP_FILE"
        
        # Compress the backup
        gzip "$BACKUP_FILE"
        echo "Backup compressed: ${BACKUP_FILE}.gz"
        
        # Show backup size
        BACKUP_SIZE=$(du -h "${BACKUP_FILE}.gz" | cut -f1)
        echo "Backup size: $BACKUP_SIZE"
        
        # List recent backups
        echo ""
        echo "Recent backups:"
        ls -lht "$BACKUP_DIR"/*.gz 2>/dev/null | head -5
    else
        echo "Backup failed!"
        exit 1
    fi
    
    unset PGPASSWORD
}

# Function to restore from backup
restore_backup() {
    local backup_file="$1"
    
    if [ -z "$backup_file" ]; then
        echo "Usage: $0 restore <backup_file>"
        echo "Available backups:"
        ls -lt "$BACKUP_DIR"/*.gz 2>/dev/null | head -10
        exit 1
    fi
    
    if [ ! -f "$backup_file" ]; then
        echo "Backup file not found: $backup_file"
        exit 1
    fi
    
    echo "=== Restoring Database from Backup ==="
    echo "Backup file: $backup_file"
    
    read -p "This will replace all current data. Are you sure? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "Restore cancelled."
        exit 0
    fi
    
    # Set password for psql
    export PGPASSWORD="$DB_PASSWORD"
    
    # Decompress if needed
    if [[ "$backup_file" == *.gz ]]; then
        echo "Decompressing backup..."
        temp_file="/tmp/wa_restore_$(date +%s).sql"
        gunzip -c "$backup_file" > "$temp_file"
        restore_file="$temp_file"
    else
        restore_file="$backup_file"
    fi
    
    echo "Restoring database..."
    psql -h localhost -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" \
        --verbose --file="$restore_file"
    
    if [ $? -eq 0 ]; then
        echo "Database restored successfully!"
    else
        echo "Restore failed!"
        exit 1
    fi
    
    # Cleanup temporary file
    if [[ "$backup_file" == *.gz ]] && [ -f "$temp_file" ]; then
        rm "$temp_file"
    fi
    
    unset PGPASSWORD
}

# Function to list backups
list_backups() {
    echo "=== Available Backups ==="
    if [ -d "$BACKUP_DIR" ] && [ "$(ls -A $BACKUP_DIR 2>/dev/null)" ]; then
        ls -lht "$BACKUP_DIR"/*.{sql,gz} 2>/dev/null | head -20
    else
        echo "No backups found in $BACKUP_DIR"
    fi
}

# Function to cleanup old backups
cleanup_backups() {
    local keep_count="${1:-10}"
    
    echo "=== Cleaning up old backups (keeping $keep_count) ==="
    
    if [ ! -d "$BACKUP_DIR" ]; then
        echo "Backup directory does not exist: $BACKUP_DIR"
        return
    fi
    
    # Count current backups
    backup_count=$(ls "$BACKUP_DIR"/*.{sql,gz} 2>/dev/null | wc -l)
    
    if [ "$backup_count" -le "$keep_count" ]; then
        echo "Only $backup_count backups found, nothing to cleanup."
        return
    fi
    
    echo "Found $backup_count backups, removing oldest..."
    
    # Remove oldest backups, keeping the specified count
    ls -t "$BACKUP_DIR"/*.{sql,gz} 2>/dev/null | tail -n +$((keep_count + 1)) | while read file; do
        echo "Removing: $file"
        rm "$file"
    done
    
    echo "Cleanup completed."
}

# Function to show help
show_help() {
    echo "=== Database Management Script ==="
    echo "Usage: $0 [command] [options]"
    echo ""
    echo "Commands:"
    echo "  setup              - Stop existing container and create new one"
    echo "  backup             - Create a database backup"
    echo "  restore <file>     - Restore database from backup file"
    echo "  list               - List available backups"
    echo "  cleanup [count]    - Remove old backups (default: keep 10)"
    echo "  help               - Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 setup"
    echo "  $0 backup"
    echo "  $0 restore ./backups/wa_backup_20241015_120000.sql.gz"
    echo "  $0 list"
    echo "  $0 cleanup 5"
    echo ""
}

# Main execution based on command line arguments
case "${1:-setup}" in
    "setup")
        echo "=== Database Container Management ==="
        stop_existing_container
        create_new_container
        echo "=== Done ==="
        ;;
    "backup")
        create_backup
        ;;
    "restore")
        restore_backup "$2"
        ;;
    "list")
        list_backups
        ;;
    "cleanup")
        cleanup_backups "$2"
        ;;
    "help"|"-h"|"--help")
        show_help
        ;;
    *)
        echo "Unknown command: $1"
        show_help
        exit 1
        ;;
esac