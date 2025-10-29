using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Radzen;
using Serilog;
using WarriorExperiment.App.Components;
using WarriorExperiment.Core.Extensions;
using WarriorExperiment.Core.Services;
using WarriorExperiment.Persistence.Data;
using WarriorExperiment.Persistence.Entities;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog as the logging provider
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure SignalR for larger message sizes (for photo uploads)
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
    options.EnableDetailedErrors = true;
});

// Add API controllers
builder.Services.AddControllers();

// Configure PostgreSQL connection with transient lifetime to avoid concurrency issues
builder.Services.AddDbContext<WaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient);

// Add ASP.NET Core Identity
builder.Services.AddIdentity<WaUser, IdentityRole<int>>(options =>
{
    // Password settings - allow any password
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 1;
    options.Password.RequiredUniqueChars = 0;
    
    // Lockout settings - disable lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(0);
    options.Lockout.MaxFailedAccessAttempts = 999;
    options.Lockout.AllowedForNewUsers = false;
    
    // User settings - username only, no email required
    options.User.RequireUniqueEmail = false;
    options.User.AllowedUserNameCharacters = null; // Allow any characters
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<WaDbContext>()
.AddDefaultTokenProviders();

// Configure authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// Configure backup options
builder.Services.Configure<WarriorExperiment.Core.Models.WaBackupOptions>(
    builder.Configuration.GetSection("BackupOptions"));

// Register HTTP context accessor for authentication
builder.Services.AddHttpContextAccessor();

// Register Core services
builder.Services.AddWaCoreServices();

// Register FluentValidation validators
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(WarriorExperiment.Persistence.Validators.WaDailySurveyValidator).Assembly);

// Add Radzen services
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

var app = builder.Build();

// Apply database migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WaDbContext>();
    try 
    {
        // Apply any pending migrations
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating the database");
        throw;
    }
}

// Migrate existing users to Identity system
using (var scope = app.Services.CreateScope())
{
    var migrationService = scope.ServiceProvider.GetRequiredService<WaUserMigrationService>();
    try
    {
        var migratedCount = await migrationService.MigrateUsersAsync();
        if (migratedCount > 0)
        {
            Log.Information("Migrated {Count} users to Identity system with default credentials", migratedCount);
            Log.Information("Default username pattern: user{{id}}");
            Log.Information("Default password: 123");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating users to Identity system");
        // Don't throw - app should still start even if user migration fails
    }
}

// Reset passwords for debugging authentication issues
using (var scope = app.Services.CreateScope())
{
    var passwordResetService = scope.ServiceProvider.GetRequiredService<WaPasswordResetService>();
    try
    {
        Log.Information("=== Resetting passwords for debugging ===");
        
        var usernames = new[] { "Moritz", "DemoWarrior", "Max", "Bart" };
        foreach (var username in usernames)
        {
            var resetResult = await passwordResetService.ResetUserPasswordAsync(username, "123");
            if (resetResult)
            {
                var testResult = await passwordResetService.TestPasswordAsync(username, "123");
                Log.Information("Password reset for {Username}: Success, Test: {TestResult}", username, testResult);
            }
            else
            {
                Log.Warning("Failed to reset password for {Username}", username);
            }
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to reset passwords");
    }
}

// Log all users with credentials for CLI reference
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WaDbContext>();
    try
    {
        var users = await dbContext.Users.ToListAsync();
        if (users.Any())
        {
            Log.Information("=== Available Users for Login ===");
            foreach (var user in users)
            {
                var username = user.UserName ?? $"user{user.Id}";
                var displayName = user.DisplayName ?? $"User {user.Id}";
                var hasPassword = !string.IsNullOrEmpty(user.PasswordHash);
                var password = hasPassword ? "123" : "No password set";
                
                Log.Information("User: {DisplayName} | Username: {Username} | Password: {Password}", 
                    displayName, username, password);
            }
            Log.Information("=== End User List ===");
        }
        else
        {
            Log.Information("No users found in database");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to list users");
    }
}

// Seed default daily tasks
using (var scope = app.Services.CreateScope())
{
    var defaultDataService = scope.ServiceProvider.GetRequiredService<WaDefaultDataService>();
    try
    {
        var hasDefaultTasks = await defaultDataService.HasDefaultDailyTasksAsync();
        if (!hasDefaultTasks)
        {
            var createdCount = await defaultDataService.SeedDefaultDailyTasksAsync();
            Log.Information("Seeded {Count} default daily tasks", createdCount);
        }
        else
        {
            Log.Information("Default daily tasks already exist, skipping seeding");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while seeding default daily tasks");
        // Don't throw - app should still start even if task seeding fails
    }
}

// Seed default motivation quotes
using (var scope = app.Services.CreateScope())
{
    var defaultDataService = scope.ServiceProvider.GetRequiredService<WaDefaultDataService>();
    try
    {
        var quotesCreated = await defaultDataService.SeedDefaultMotivationQuotesAsync();
        if (quotesCreated > 0)
        {
            Log.Information("Seeded {Count} default motivation quotes", quotesCreated);
        }
        else
        {
            Log.Information("Default motivation quotes already exist, skipping seeding");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while seeding default motivation quotes");
        // Don't throw - app should still start even if quote seeding fails
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map API controllers
app.MapControllers();

try
{
    Log.Information("Starting WarriorExperiment application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
