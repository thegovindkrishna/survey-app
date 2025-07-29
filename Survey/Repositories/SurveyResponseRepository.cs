using Survey.Data;
using Survey.Models;

namespace Survey.Repositories
{
    public class SurveyResponseRepository : Repository<SurveyResponseModel>, ISurveyResponseRepository
    {
        public SurveyResponseRepository(AppDbContext context) : base(context)
        {
        }
    }
}
