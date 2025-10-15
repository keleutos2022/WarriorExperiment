# WarriorExperiment Application Documentation

## Overview

**WarriorExperiment** is a comprehensive health and fitness tracking application built with .NET 9 and Blazor Server. The application focuses on tracking wellness metrics, body measurements, and specialized training programs including "Rite of Passage" strength training and variety practices.

## Purpose & Domain

The application serves as a digital companion for individuals following structured fitness and wellness programs, with particular emphasis on:

- **Daily wellness monitoring** through comprehensive health surveys
- **Body composition tracking** with photos and detailed measurements
- **Rite of Passage training** - A progressive ladder-based strength training methodology
- **Variety practice sessions** - Multi-exercise workout tracking
- **Progress visualization** through calendar and dashboard views

## Technology Stack

### Core Technologies
- **.NET 9** - Latest .NET framework
- **Blazor Server** - Interactive server-side rendering
- **Entity Framework Core 9** - ORM and database management
- **PostgreSQL** - Primary database (running on port 7777)
- **Radzen Blazor** - UI component library
- **FluentValidation** - Model validation

### Key Libraries
- `Radzen.Blazor` (8.0.4) - Comprehensive UI components
- `Blazored.FluentValidation` (2.2.0) - Blazor validation integration
- `Npgsql.EntityFrameworkCore.PostgreSQL` (9.0.0) - PostgreSQL provider
- `FluentValidation.AspNetCore` (11.3.0) - Server-side validation

## Project Architecture

The solution follows a **Clean Architecture** pattern with clear separation of concerns:

```
WarriorExperiment/
├── WarriorExperiment.App/          # Presentation Layer (Blazor Server)
├── WarriorExperiment.Core/         # Business Logic Layer
├── WarriorExperiment.Persistence/  # Data Access Layer
└── WarriorExperiment.sln          # Solution File
```

### 1. Presentation Layer (`WarriorExperiment.App`)

**Blazor Server application** providing the user interface.

#### Key Components:
- **Pages** - Main application screens
- **Forms** - Data entry components with validation
- **Grids** - Data display components with filtering/sorting
- **Layout** - Application shell and navigation
- **Base Components** - Shared functionality (`WaBaseComponent`, `WaBasePage`)

#### Notable Features:
- **User Selection** - Cascading parameter system for multi-user support
- **Calendar Integration** - Radzen Scheduler for timeline visualization
- **Real-time Validation** - FluentValidation integration
- **Responsive Design** - Mobile-friendly layouts

### 2. Business Logic Layer (`WarriorExperiment.Core`)

**Service layer** containing business rules and application logic.

#### Key Services:
- **WaEntryService** - Unified calendar entry aggregation
- **WaRiteOfPassagePracticeEntryService** - Advanced ladder progression logic
- **WaUserService** - User management
- **WaMeasurementEntryService** - Body measurement tracking
- **WaDailySurveyEntryService** - Wellness survey management
- **WaVarietyPracticeEntryService** - Multi-exercise session tracking
- **WaImageService** - Base64 image handling

#### Advanced Features:
- **Smart Progression Logic** - Automatic ladder training advancement
- **Intensity Cycling** - Heavy → Light → Medium training patterns
- **Calendar Aggregation** - Unified view across all entry types

### 3. Data Access Layer (`WarriorExperiment.Persistence`)

**Entity Framework Core** implementation with PostgreSQL backend.

#### Data Models:
- **WaUser** - User profiles with program tracking
- **WaDailySurveyEntry** - Daily wellness assessments
- **WaMeasurementEntry** - Body composition and measurements
- **WaRiteOfPassagePracticeEntry** - Strength training sessions
- **WaVarietyPracticeEntry** - Multi-exercise workouts
- **WaExercise** - Individual exercises within variety practices
- **WaMeasurementMethod** - Measurement device/method tracking

#### Database Features:
- **Automatic Migrations** - Applied on application startup
- **Unique Constraints** - Prevent duplicate daily entries
- **Cascade Deletes** - Maintain referential integrity
- **Precision Decimals** - Accurate measurement storage
- **Enum Storage** - String-based enum persistence

## Core Data Models

### WaUser
Central user entity with program tracking capabilities.

```csharp
public class WaUser : WaBase
{
    public string UserName { get; set; }
    public decimal? Height { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? DateOfStart { get; set; }
    
    // Navigation Collections
    public ICollection<WaDailySurveyEntry> DailySurveys { get; set; }
    public ICollection<WaMeasurementEntry> MeasurementEntries { get; set; }
    public ICollection<WaRiteOfPassagePracticeEntry> RiteOfPassagePracticeEntries { get; set; }
    public ICollection<WaVarietyPracticeEntry> VarietyPractices { get; set; }
}
```

### WaDailySurveyEntry
Comprehensive daily wellness tracking with 7 key metrics.

**Tracked Metrics:**
- Sleep Quality (1-10)
- Energy Level (1-10)
- Mood (1-10)
- Muscle Soreness (1-10)
- Stress Level (1-10)
- Hunger During Undereating Phase (1-10)
- Bowel Movement Time (enum: Morning/Midday/Afternoon/Night)

### WaMeasurementEntry
Detailed body composition and circumference tracking.

**Capabilities:**
- **Photo Storage** - Front, back, side photos (Base64)
- **Body Composition** - Weight, body fat %, muscle mass, water %, bone mass, BMI, visceral fat
- **Metabolic Data** - Metabolic age, basal metabolic rate
- **Circumferences** - Chest, waist, hip, bicep, thigh, calf (all in cm)
- **Method Tracking** - Links to specific measurement devices/scales

### WaRiteOfPassagePracticeEntry
Advanced strength training with intelligent progression.

**Core Features:**
- **Ladder Training** - Progressive 1,2,3,4,5 step methodology
- **Intensity Levels** - Heavy/Medium/Light with automatic cycling
- **Smart Progression** - Automatic advancement based on success
- **Performance Metrics** - Pull count, time (dice roll), pulls/minute
- **Weight Carryover** - Automatic weight suggestion from previous sessions

**Progression Logic:**
```
Stage 1: 1,2,3 × 3 sets
Stage 2: 1,2,3 × 4 sets  
Stage 3: 1,2,3 × 5 sets
Stage 4: 1,2,3,4 × 1 set + 1,2,3 × 4 sets
Stage 5: 1,2,3,4 × 2 sets + 1,2,3,4,5 × 3 sets
Final: 1,2,3,4,5 × 5 sets
```

**Intensity Adjustments:**
- **Heavy:** Full progression pattern (progresses from last successful heavy day)
- **Medium:** Ladder height - 1 (e.g., 1,2 instead of 1,2,3)
- **Light:** Ladder height - 2 (e.g., 1 instead of 1,2,3)

### WaVarietyPracticeEntry & WaExercise
Flexible multi-exercise session tracking.

**Structure:**
- **Variety Practice** - Session container with date and notes
- **Exercises** - Individual exercises with sets, reps, weight
- **Flexibility** - Unlimited exercises per session

## Key Business Features

### 1. Intelligent Rite of Passage Progression

The application implements sophisticated training progression logic:

**Automatic Progression:**
- Tracks last successful heavy day
- Progresses ladder count (3→4→5)
- Advances step count (3→4→5 steps per ladder)
- Maintains intensity cycling pattern (Heavy → Light → Medium)

**Weight Management:**
- Auto-fills weight from last session
- Persists across intensity changes
- Supports metric precision (0.5kg increments)

**Validation Rules:**
- Ensures proper ladder progression (can't have Ladder 5 without 1-4)
- Validates uniform ladder values (all active ladders same value)
- Prevents invalid configurations

### 2. Unified Calendar System

**Cross-Entry Integration:**
- Aggregates all entry types into unified calendar view
- Color-coded by entry type
- Detailed hover information
- Direct navigation to edit forms

**Filtering & Views:**
- Toggle between calendar and table views
- Filter by entry type
- Date range selection (last 90 days)
- Search and sorting capabilities

### 3. Comprehensive Validation

**FluentValidation Integration:**
- Server-side validation for all models
- Real-time client feedback
- Business rule enforcement
- Data integrity protection

**Custom Validators:**
- **WaRiteOfPassagePracticeEntryValidator** - Ladder progression rules
- **WaDailySurveyValidator** - Wellness metric constraints
- **WaMeasurementEntryValidator** - Measurement value ranges
- **WaUserValidator** - User profile requirements

## User Interface Components

### Dashboard (`WaDashboardPage`)
Central hub with unified calendar and data views.

**Features:**
- **Split Button Navigation** - Quick access to all entry types
- **Calendar/Table Toggle** - Flexible data visualization
- **Entry Type Filtering** - Selective display by category
- **90-Day Window** - Performance-optimized date range

### Forms System
Consistent, validated data entry across all entity types.

**Common Features:**
- Auto-fill capabilities (dates, weights, progressions)
- Real-time validation feedback
- Cancel/save workflows
- Context-aware field suggestions

**Specialized Forms:**
- **Rite of Passage Form** - Advanced progression display and auto-calculation
- **Measurement Form** - Photo upload and comprehensive metrics
- **Daily Survey Form** - Streamlined wellness assessment
- **Variety Practice Form** - Dynamic exercise management

### Grid Components
Data display with advanced filtering and interaction.

**Capabilities:**
- Pagination and virtual scrolling
- Column sorting and filtering
- Color-coded status indicators
- Direct edit navigation
- Export capabilities

## Database Schema

### Key Relationships
```
WaUser (1) → (∞) WaDailySurveyEntry
WaUser (1) → (∞) WaMeasurementEntry
WaUser (1) → (∞) WaRiteOfPassagePracticeEntry
WaUser (1) → (∞) WaVarietyPracticeEntry

WaMeasurementMethod (1) → (∞) WaMeasurementEntry
WaVarietyPracticeEntry (1) → (∞) WaExercise
```

### Unique Constraints
- **Daily Surveys** - One per user per day
- **Rite of Passage** - One per user per day  
- **Variety Practices** - One per user per day
- **Measurement Methods** - Unique names

### Audit Trail
All entities inherit from `WaBase`:
```csharp
public abstract class WaBase
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime EnteredAt { get; set; }
}
```

## Configuration & Setup

### Database Configuration
```csharp
// PostgreSQL connection (Program.cs)
Host=localhost;Port=7777;Database=warrior_experiment;Username=postgres;Password=postgres
```

### Service Registration
```csharp
// Core services registration
builder.Services.AddWaCoreServices();

// Validation services
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(WaDailySurveyValidator).Assembly);

// Radzen UI services
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();
```

### Automatic Migration
```csharp
// Applied on startup (Program.cs)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WaDbContext>();
    await dbContext.Database.MigrateAsync();
}
```

## Development Scripts

### Database Management
- **`manage-db.sh`** - Database administration commands
- **`addMigration.sh`** - Entity Framework migration creation
- **`init-tmux.sh`** - Development environment setup

### Migration Workflow
```bash
# Create new migration
./addMigration.sh "MigrationName"

# Database operations
./manage-db.sh reset    # Reset database
./manage-db.sh migrate  # Apply migrations
./manage-db.sh seed     # Seed test data
```

## Key Workflows

### 1. Daily Wellness Tracking
1. User selects date (defaults to today)
2. Completes 7-metric assessment
3. Optional comments
4. Validation ensures 1-10 range compliance
5. Prevents duplicate entries per day

### 2. Rite of Passage Training
1. System suggests next intensity (Heavy→Light→Medium cycle)
2. Auto-fills ladder values based on progression rules
3. Carries over weight from last session
4. User can manually adjust values
5. Success tracking drives future progressions

### 3. Body Measurement Sessions
1. Select measurement method/device
2. Capture photos (front/back/side)
3. Record available metrics
4. System calculates derived values (BMI, etc.)
5. Historical comparison and trend analysis

### 4. Variety Practice Sessions
1. Create session with date and notes
2. Add multiple exercises dynamically
3. Record sets, reps, weight per exercise
4. Flexible exercise library management

## Advanced Features

### Smart Progression Algorithm
The Rite of Passage progression system implements sophisticated training logic:

```csharp
// Progression rules (simplified)
if (activeLadders == 3) return (maxSteps, maxSteps, maxSteps, maxSteps, 0);      // Add 4th ladder
if (activeLadders == 4) return (maxSteps, maxSteps, maxSteps, maxSteps, maxSteps); // Add 5th ladder
if (activeLadders == 5 && maxSteps < 5) return (maxSteps + 1, ...);              // Increment steps
```

### Calendar Integration
Unified data aggregation across all entry types with intelligent summarization:

```csharp
// Example calendar entry generation
Title = $"Rite of Passage - {intensity}"
Description = $"Pulls: {pullCount}, Time: {dice}min, {(success ? "✓" : "✗")}"
AdditionalData = { PracticeIntensity, PullCount, Dice, Success, PullsPerMinute }
```

### Photo Management
Base64 image storage with efficient handling:
- Direct database storage for simplicity
- Supports front, back, and side body photos
- Integrated with measurement entries
- Memory-efficient display

## Security & Validation

### Data Integrity
- **FluentValidation** - Comprehensive server-side validation
- **Unique Constraints** - Prevent duplicate entries
- **Foreign Key Constraints** - Maintain referential integrity
- **Range Validation** - Ensure realistic metric values

### Input Sanitization
- **Maximum Length Limits** - Prevent data overflow
- **Type Safety** - Strong typing throughout
- **Enum Validation** - Constrained value sets
- **SQL Injection Protection** - Entity Framework parameterization

## Performance Considerations

### Database Optimization
- **Indexed Relationships** - Fast user-based queries
- **Pagination Support** - Large dataset handling
- **Lazy Loading** - Efficient relationship loading
- **Connection Pooling** - PostgreSQL optimization

### UI Performance
- **Component Reuse** - Shared base components
- **Virtual Scrolling** - Large grid performance
- **Caching** - Service-level data caching
- **Progressive Loading** - Improved perceived performance

## Monitoring & Logging

### Application Logging
- **Console Logging** - Development feedback
- **Exception Handling** - Graceful error recovery
- **Migration Logging** - Database update tracking

### Health Checks
- **Database Connectivity** - Automatic migration on startup
- **Service Registration** - Dependency injection validation

## Running the Application

### Prerequisites
- .NET 9 SDK
- PostgreSQL (running on port 7777)
- Container runtime (Docker/Podman)

### Database Setup
```bash
# Start PostgreSQL container
podman run --name warrior-postgres -e POSTGRES_PASSWORD=postgres -p 7777:5432 -d postgres:latest
```

### Application Startup
```bash
# Navigate to app directory
cd WarriorExperiment.App

# Run application (migrations applied automatically)
dotnet run
```

## UTC DateTime Handling

### PostgreSQL DateTime Compatibility

The application uses PostgreSQL's `timestamp with time zone` data type for all DateTime fields, which requires proper UTC DateTime handling to prevent runtime errors.

### Common Issues & Solutions

#### Issue: DateTime.Kind=Unspecified Error
**Error**: `Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone'`

**Root Cause**: Using `DateTime.UtcNow.Date` creates a DateTime with `DateTimeKind.Unspecified`

**Solution**: Use `DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc)` for date-only values

#### Implementation Patterns

**✅ Correct DateTime Assignments:**
```csharp
// For new entries in edit pages
model.Date = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
model.EnteredAt = DateTime.UtcNow;

// In service CreateAsync methods
entry.CreatedAt = DateTime.UtcNow;
entry.UpdatedAt = DateTime.UtcNow;
entry.EnteredAt = DateTime.UtcNow;
```

**❌ Problematic DateTime Assignments:**
```csharp
// These cause PostgreSQL errors
model.Date = DateTime.UtcNow.Date;  // Kind=Unspecified
model.Date = DateTime.Now.Date;     // Kind=Local
```

### EnteredAt Field Requirements

All entity models include an `EnteredAt` timestamp field that must be set during creation:

- **WaUser**: User registration timestamp
- **WaMeasurementEntry**: Measurement recording time
- **WaDailySurveyEntry**: Survey completion time
- **WaRiteOfPassagePracticeEntry**: Training session entry time
- **WaVarietyPracticeEntry**: Workout recording time
- **WaExercise**: Exercise definition time

### Files Modified for UTC Compliance

#### Service Layer Fixes:
- `WaMeasurementEntryService.cs:88` - Added EnteredAt assignment
- `WaDailySurveyEntryService.cs:85` - Added EnteredAt assignment
- `WaVarietyPracticeEntryService.cs:88` - Added EnteredAt assignment
- `WaUserService.cs:83` - Already included EnteredAt assignment
- `WaRiteOfPassagePracticeEntryService.cs:86` - Already included EnteredAt assignment

#### UI Layer Fixes:
- `WaMeasurementEditPage.razor:192` - Fixed Date field with SpecifyKind
- `WaDailySurveyEditPage.razor:125` - Fixed Date field with SpecifyKind
- `WaVarietyPracticeEditPage.razor:179` - Fixed Date field with SpecifyKind
- `WaRiteOfPassagePracticeEditPage.razor` - Fixed Date field and added EnteredAt assignment

### Best Practices

1. **Always use DateTime.UtcNow** for timestamp fields (CreatedAt, UpdatedAt, EnteredAt)
2. **Use DateTime.SpecifyKind()** when setting date-only values from UTC timestamps
3. **Set EnteredAt in both services and edit pages** for comprehensive tracking
4. **Test with PostgreSQL** to catch UTC issues early in development

## Future Considerations

### Potential Enhancements
- **Multi-tenancy** - Organization support
- **Mobile App** - Native iOS/Android
- **Advanced Analytics** - Machine learning insights
- **Export/Import** - Data portability
- **Social Features** - Community challenges
- **Offline Support** - PWA capabilities

### Scalability Options
- **Microservices** - Service decomposition
- **Event Sourcing** - Audit trail enhancement
- **CQRS Implementation** - Read/write optimization
- **Cloud Deployment** - Azure/AWS hosting

## Conclusion

WarriorExperiment represents a sophisticated health and fitness tracking platform with particular strength in progressive training methodologies. The clean architecture, comprehensive validation, and intelligent automation features create a robust foundation for long-term wellness tracking and program adherence.

The application successfully balances ease of use with advanced functionality, making it suitable for both casual fitness enthusiasts and serious practitioners of structured training programs like the Rite of Passage methodology.