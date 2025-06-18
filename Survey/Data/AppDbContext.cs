using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Survey.Models;
using SurveyModel = Survey.Models.Survey;
using System.Text.Json;

namespace Survey.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<SurveyModel> Surveys { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<SurveyResponse> SurveyResponses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SurveyModel>(entity =>
            {
                entity.Property(e => e.Title).HasColumnType("longtext");
                entity.Property(e => e.Description).HasColumnType("longtext");
                entity.Property(e => e.CreatedBy).HasColumnType("longtext");
                entity.Property(e => e.ShareLink).HasColumnType("longtext");
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.ToTable("Question");
                entity.Property(e => e.QuestionText).HasColumnType("longtext");
                entity.Property(e => e.type).HasColumnType("longtext");
                entity.Property(e => e.required).HasColumnType("tinyint(1)");
                entity.Property(e => e.options).HasColumnType("json");
                entity.Property(e => e.maxRating).HasColumnType("int");
                entity.Property(e => e.SurveyId).HasColumnType("int");

                entity.HasOne(q => q.Survey)
                    .WithMany(s => s.Questions)
                    .HasForeignKey(q => q.SurveyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SurveyModel>()
                .HasMany(s => s.Questions)
                .WithOne(q => q.Survey)
                .HasForeignKey(q => q.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Value converter for List<QuestionResponse> to JSON string
            var questionResponseConverter = new ValueConverter<List<QuestionResponse>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<QuestionResponse>>(v, (JsonSerializerOptions)null) ?? new List<QuestionResponse>()
            );

            modelBuilder.Entity<SurveyResponse>(entity =>
            {
                entity.Property(e => e.RespondentEmail).HasColumnType("longtext");
                entity.Property(e => e.SubmissionDate).HasColumnType("datetime");
                entity.Property(e => e.responses)
                    .HasColumnType("longtext")
                    .HasConversion(questionResponseConverter);

                entity.HasOne<SurveyModel>()
                    .WithMany()
                    .HasForeignKey(e => e.SurveyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}