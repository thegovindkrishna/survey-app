using SurveyModel = Survey.Models.Survey;

namespace Survey.Services
{
    public interface ISurveyService
    {
        Task<List<SurveyModel>> GetAll();
        Task<SurveyModel?> GetById(int id);
        Task<SurveyModel> Create(SurveyModel survey, string adminEmail);
        Task<SurveyModel?> Update(int id, SurveyModel survey);
        Task<bool> Delete(int id);
    }
}
