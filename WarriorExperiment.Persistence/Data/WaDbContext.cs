using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WarriorExperiment.Persistence.Models;

namespace WarriorExperiment.Persistence.Data;

/// <summary>
/// Database context for the WarriorExperiment application
/// </summary>
public class WaDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WaDbContext"/> class
    /// </summary>
    /// <param name="options">The options to configure the context</param>
    public WaDbContext(DbContextOptions<WaDbContext> options) : base(options)
    {
    }
    
    /// <summary>
    /// Gets or sets the Users DbSet
    /// </summary>
    public DbSet<WaUser> Users { get; set; }
    
    /// <summary>
    /// Gets or sets the DailySurveys DbSet
    /// </summary>
    public DbSet<WaDailySurveyEntry> DailySurveys { get; set; }
    
    /// <summary>
    /// Gets or sets the MeasurementMethods DbSet
    /// </summary>
    public DbSet<WaMeasurementMethod> MeasurementMethods { get; set; }
    
    /// <summary>
    /// Gets or sets the MeasurementEntries DbSet
    /// </summary>
    public DbSet<WaMeasurementEntry> MeasurementEntries { get; set; }
    
    /// <summary>
    /// Gets or sets the RiteOfPassagePracticeEntries DbSet
    /// </summary>
    public DbSet<WaRiteOfPassagePracticeEntry> RiteOfPassagePracticeEntries { get; set; }
    
    /// <summary>
    /// Gets or sets the VarietyPractices DbSet
    /// </summary>
    public DbSet<WaVarietyPracticeEntry> VarietyPractices { get; set; }
    
    /// <summary>
    /// Gets or sets the Exercises DbSet
    /// </summary>
    public DbSet<WaExercise> Exercises { get; set; }
    
    /// <summary>
    /// Configures the model creating conventions
    /// </summary>
    /// <param name="modelBuilder">The model builder instance</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure the WaUser entity
        modelBuilder.Entity<WaUser>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            
            entity.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(u => u.Height)
                .HasPrecision(5, 2);
                
            entity.Property(u => u.IsDefault)
                .IsRequired()
                .HasDefaultValue(false);
                
            entity.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
                
            entity.Property(u => u.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
                
            // Configure one-to-many relationship with DailySurveys
            entity.HasMany(u => u.DailySurveys)
                .WithOne(ds => ds.User)
                .HasForeignKey(ds => ds.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure one-to-many relationship with MeasurementEntries
            entity.HasMany(u => u.MeasurementEntries)
                .WithOne(me => me.User)
                .HasForeignKey(me => me.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure one-to-many relationship with RiteOfPassagePracticeEntries
            entity.HasMany(u => u.RiteOfPassagePracticeEntries)
                .WithOne(rpe => rpe.User)
                .HasForeignKey(rpe => rpe.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure one-to-many relationship with VarietyPractices
            entity.HasMany(u => u.VarietyPractices)
                .WithOne(vp => vp.User)
                .HasForeignKey(vp => vp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configure the WaDailySurveyEntry entity
        modelBuilder.Entity<WaDailySurveyEntry>(entity =>
        {
            entity.ToTable("daily_surveys");
            entity.HasKey(ds => ds.Id);
            
            entity.Property(ds => ds.Date)
                .IsRequired();
                
            entity.Property(ds => ds.SleepQuality)
                .IsRequired()
                .HasDefaultValue(5);
                
            entity.Property(ds => ds.Energy)
                .IsRequired()
                .HasDefaultValue(5);
                
            entity.Property(ds => ds.Mood)
                .IsRequired()
                .HasDefaultValue(5);
                
            entity.Property(ds => ds.MuscleSoreness)
                .IsRequired()
                .HasDefaultValue(5);
                
            entity.Property(ds => ds.BowelMovement)
                .IsRequired()
                .HasConversion<string>();
                
            entity.Property(ds => ds.StressLevel)
                .IsRequired()
                .HasDefaultValue(5);
                
            entity.Property(ds => ds.HungerFeelingDuringUndereatingPhase)
                .IsRequired()
                .HasDefaultValue(5);
                
            entity.Property(ds => ds.Comment)
                .HasMaxLength(1000);
                
            entity.Property(ds => ds.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
                
            entity.Property(ds => ds.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
                
            // Create unique index on UserId and Date
            entity.HasIndex(ds => new { ds.UserId, ds.Date })
                .IsUnique();
        });
        
        // Configure the WaMeasurementMethod entity
        modelBuilder.Entity<WaMeasurementMethod>(entity =>
        {
            entity.ToTable("measurement_methods");
            entity.HasKey(mm => mm.Id);
            
            entity.Property(mm => mm.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(mm => mm.Description)
                .HasMaxLength(500);
                
            entity.Property(mm => mm.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
                
            entity.Property(mm => mm.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
                
            // Create unique index on Name
            entity.HasIndex(mm => mm.Name)
                .IsUnique();
                
            // Configure one-to-many relationship with MeasurementEntries
            entity.HasMany(mm => mm.MeasurementEntries)
                .WithOne(me => me.MeasurementMethod)
                .HasForeignKey(me => me.MeasurementMethodId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Configure the WaMeasurementEntry entity
        modelBuilder.Entity<WaMeasurementEntry>(entity =>
        {
            entity.ToTable("measurement_entries");
            entity.HasKey(me => me.Id);
            
            entity.Property(me => me.Date)
                .IsRequired();
                
            // Configure decimal precision for measurements (increased limits)
            entity.Property(me => me.Weight).HasPrecision(6, 2); // max 9999.99 kg
            entity.Property(me => me.BodyFat).HasPrecision(5, 2); // max 999.99%
            entity.Property(me => me.MuscleMass).HasPrecision(6, 2); // max 9999.99 kg
            entity.Property(me => me.WaterPercentage).HasPrecision(5, 2); // max 999.99%
            entity.Property(me => me.BoneMass).HasPrecision(5, 2); // max 999.99 kg
            entity.Property(me => me.BMI).HasPrecision(5, 2); // max 999.99
            
            // Configure circumference measurements (increased limits)
            entity.Property(me => me.ChestCircumference).HasPrecision(6, 2); // max 9999.99 cm
            entity.Property(me => me.WaistCircumference).HasPrecision(6, 2); // max 9999.99 cm
            entity.Property(me => me.HipCircumference).HasPrecision(6, 2); // max 9999.99 cm
            entity.Property(me => me.BicepCircumference).HasPrecision(5, 2); // max 999.99 cm
            entity.Property(me => me.ThighCircumference).HasPrecision(6, 2); // max 9999.99 cm
            entity.Property(me => me.CalfCircumference).HasPrecision(5, 2); // max 999.99 cm
            
            entity.Property(me => me.Notes)
                .HasMaxLength(1000);
                
            entity.Property(me => me.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
                
            entity.Property(me => me.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
        });
        
        // Configure the WaRiteOfPassagePracticeEntry entity
        modelBuilder.Entity<WaRiteOfPassagePracticeEntry>(entity =>
        {
            entity.ToTable("rite_of_passage_practice_entries");
            entity.HasKey(rpe => rpe.Id);
            
            entity.Property(rpe => rpe.Date)
                .IsRequired();
                
            entity.Property(rpe => rpe.PracticeIntensity)
                .IsRequired()
                .HasConversion<string>();
                
            entity.Property(rpe => rpe.Weight)
                .HasPrecision(5, 2);
                
            entity.Property(rpe => rpe.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
                
            entity.Property(rpe => rpe.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
                
            // Create unique index on UserId and Date to prevent duplicate entries on same day
            entity.HasIndex(rpe => new { rpe.UserId, rpe.Date })
                .IsUnique();
        });
        
        // Configure the WaVarietyPracticeEntry entity
        modelBuilder.Entity<WaVarietyPracticeEntry>(entity =>
        {
            entity.ToTable("variety_practices");
            entity.HasKey(vp => vp.Id);
            
            entity.Property(vp => vp.Date)
                .IsRequired();
                
            entity.Property(vp => vp.Notes)
                .HasMaxLength(1000);
                
            entity.Property(vp => vp.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
                
            entity.Property(vp => vp.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
                
            // Create unique index on UserId and Date
            entity.HasIndex(vp => new { vp.UserId, vp.Date })
                .IsUnique();
                
            // Configure one-to-many relationship with Exercises
            entity.HasMany(vp => vp.Exercises)
                .WithOne(e => e.VarietyPractice)
                .HasForeignKey(e => e.VarietyPracticeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configure the WaExercise entity
        modelBuilder.Entity<WaExercise>(entity =>
        {
            entity.ToTable("exercises");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Weight)
                .HasPrecision(5, 2);
                
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
                
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
                
            entity.Property(e => e.EnteredAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
        });
    }

    /// <summary>
    /// Value converter that ensures DateTime values are treated as UTC for PostgreSQL compatibility
    /// </summary>
    public class DateTimeToDateTimeUtc : ValueConverter<DateTime, DateTime>
    {
        /// <summary>
        /// Initializes a new instance of the DateTimeToDateTimeUtc converter
        /// </summary>
        public DateTimeToDateTimeUtc() : base(
            convertToProviderExpression: dateTime => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            convertFromProviderExpression: dateTime => dateTime)
        {
        }
    }

    /// <summary>
    /// Configures global conventions for the database context
    /// </summary>
    /// <param name="configurationBuilder">The model configuration builder</param>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Apply UTC converter to all DateTime properties globally
        configurationBuilder.Properties<DateTime>()
            .HaveConversion(typeof(DateTimeToDateTimeUtc));
            
        base.ConfigureConventions(configurationBuilder);
    }
}