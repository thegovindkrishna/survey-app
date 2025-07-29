using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; // <-- Add this using statement
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Survey.Models;
using System.Text.Json;
using SurveyModel = Survey.Models.SurveyModel;

namespace Survey.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<SurveyModel> Surveys { get; set; }
        public DbSet<QuestionModel> Questions { get; set; }
        public DbSet<SurveyResponseModel> SurveyResponses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Corrected data types for SQL Server
            modelBuilder.Entity<SurveyModel>(entity =>
            {
                entity.Property(e => e.Title).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CreatedBy).HasColumnType("nvarchar(max)");
                entity.Property(e => e.ShareLink).HasColumnType("nvarchar(max)");
            });

            modelBuilder.Entity<QuestionModel>(entity =>
            {
                entity.ToTable("Question");
                entity.Property(e => e.QuestionText).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Type).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Required).HasColumnType("bit"); // Use 'bit' for boolean in SQL Server
                entity.Property(e => e.Options).HasColumnType("nvarchar(max)"); // Store JSON as string
                entity.Property(e => e.MaxRating).HasColumnType("int");
                entity.Property(e => e.SurveyId).HasColumnType("int");

                // This relationship is sufficient, the one below is a duplicate
                entity.HasOne(q => q.Survey)
                    .WithMany(s => s.Questions)
                    .HasForeignKey(q => q.SurveyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // This is redundant as it's defined above from the Question entity's perspective.
            // modelBuilder.Entity<SurveyModel>()
            //     .HasMany(s => s.Questions)
            //     .WithOne(q => q.Survey)
            //     .HasForeignKey(q => q.SurveyId)
            //     .OnDelete(DeleteBehavior.Cascade);

            // --- Value converter AND comparer for List<QuestionResponse> ---
            var questionResponseConverter = new ValueConverter<List<QuestionResponseModel>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<QuestionResponseModel>>(v, (JsonSerializerOptions)null) ?? new List<QuestionResponseModel>()
            );

            // This tells EF Core how to compare the lists to detect changes
            var questionResponseComparer = new ValueComparer<List<QuestionResponseModel>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList());

            modelBuilder.Entity<SurveyResponseModel>(entity =>
            {
                entity.Property(e => e.RespondentEmail).HasColumnType("nvarchar(max)");
                entity.Property(e => e.SubmissionDate).HasColumnType("datetime2"); // Use 'datetime2' for SQL Server
                entity.Property(e => e.responses)
                    .HasColumnType("nvarchar(max)") // Store JSON as string
                    .HasConversion(questionResponseConverter)
                    .Metadata.SetValueComparer(questionResponseComparer); // <-- Set the comparer here

                entity.HasOne<SurveyModel>()
                    .WithMany()
                    .HasForeignKey(e => e.SurveyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}