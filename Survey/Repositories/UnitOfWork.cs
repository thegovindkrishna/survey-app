using Survey.Data;
using System.Threading.Tasks;

namespace Survey.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public ISurveyRepository Surveys { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Surveys = new SurveyRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

