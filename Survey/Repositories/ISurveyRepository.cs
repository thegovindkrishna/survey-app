using Survey.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using SurveyModel = Survey.Models.SurveyModel;

namespace Survey.Repositories
{
    public interface ISurveyRepository : IRepository<SurveyModel>
    {
        Task<PagedList<SurveyModel>> GetAllWithQuestionsAsync(PaginationParams paginationParams, string? sortBy = null, string? sortOrder = null);
        Task<IEnumerable<SurveyModel>> GetAllWithQuestionsAsync();
        Task<SurveyModel?> GetByIdWithQuestionsAsync(int id);
    }
}
