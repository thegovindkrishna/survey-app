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
    }
}
