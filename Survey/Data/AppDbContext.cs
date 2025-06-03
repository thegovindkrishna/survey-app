using Microsoft.EntityFrameworkCore;
using Survey.Models;
using SurveyModel = Survey.Models.Survey;

namespace Survey.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<SurveyModel> Surveys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SurveyModel>(entity =>
            {
                entity.Property(e => e.Title).HasColumnType("longtext");
                entity.Property(e => e.Description).HasColumnType("longtext");
                entity.Property(e => e.CreatedBy).HasColumnType("longtext");
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.Property(e => e.QuestionText).HasColumnType("longtext");
                entity.Property(e => e.type).HasColumnType("longtext");
                entity.Property(e => e.required).HasColumnType("tinyint(1)");
                entity.Property(e => e.options).HasColumnType("json");
                entity.Property(e => e.maxRating).HasColumnType("int");
            });

            modelBuilder.Entity<SurveyModel>()
                .HasMany(s => s.Questions)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}