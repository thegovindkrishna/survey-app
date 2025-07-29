using Survey.Data;
using System.Threading.Tasks;

namespace Survey.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public ISurveyRepository Surveys { get; private set; }
        public IUserRepository Users { get; private set; }
        public ISurveyResponseRepository SurveyResponses { get; private set; }
        public IQuestionRepository Questions { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Surveys = new SurveyRepository(_context);
            Users = new UserRepository(_context);
            SurveyResponses = new SurveyResponseRepository(_context);
            Questions = new QuestionRepository(_context);
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

