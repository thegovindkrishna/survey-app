using Survey.Data;
using Survey.Models;

namespace Survey.Repositories
{
    public class QuestionRepository : Repository<QuestionModel>, IQuestionRepository
    {
        public QuestionRepository(AppDbContext context) : base(context)
        {
        }
    }
}
