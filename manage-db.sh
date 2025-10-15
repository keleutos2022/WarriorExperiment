#!/bin/bash

# Database configuration
CONTAINER_NAME="warrior-experiment-db"
DB_NAME="warrior_experiment"
DB_USER="postgres"
DB_PASSWORD="postgres"
DB_PORT="7777"
POSTGRES_VERSION="16"

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

# Main execution
echo "=== Database Container Management ==="
stop_existing_container
create_new_container
echo "=== Done ==="