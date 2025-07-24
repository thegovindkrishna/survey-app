using Survey.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using SurveyModel = Survey.Models.Survey;

namespace Survey.Repositories
{
    public interface ISurveyRepository : IRepository<SurveyModel>
    {
        Task<IEnumerable<SurveyModel>> GetAllWithQuestionsAsync();
        Task<SurveyModel?> GetByIdWithQuestionsAsync(int id);
    }
}
