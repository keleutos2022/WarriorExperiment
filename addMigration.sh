#!/bin/bash

# Script to create Entity Framework migrations
# Usage: ./addMigration.sh <MigrationName>

# Check if migration name is provided
if [ -z "$1" ]; then
    echo "Error: Migration name is required"
    echo "Usage: ./addMigration.sh <MigrationName>"
    exit 1
fi

# Migration name from first argument
MIGRATION_NAME=$1

# Project paths
STARTUP_PROJECT="WarriorExperiment.App"
MIGRATIONS_PROJECT="WarriorExperiment.Persistence"

echo "Creating migration: $MIGRATION_NAME"
echo "Startup project: $STARTUP_PROJECT"
echo "Migrations project: $MIGRATIONS_PROJECT"
echo ""

# Run the migration command
dotnet ef migrations add "$MIGRATION_NAME" \
    --startup-project "$STARTUP_PROJECT" \
    --project "$MIGRATIONS_PROJECT" \
    --context WaDbContext

# Check if migration was successful
if [ $? -eq 0 ]; then
    echo ""
    echo "Migration '$MIGRATION_NAME' created successfully!"
    echo ""
    echo "To apply this migration to the database, run:"
    echo "dotnet ef database update --startup-project $STARTUP_PROJECT --project $MIGRATIONS_PROJECT"
else
    echo ""
    echo "Error: Failed to create migration"
    exit 1
fi