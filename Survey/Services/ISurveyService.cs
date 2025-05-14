using SurveyModel = Survey.Models.Survey;

namespace Survey.Services
{
    public interface ISurveyService
    {
        Task<List<SurveyModel>> GetAll();
        Task<SurveyModel?> GetById(int id);
        Task<SurveyModel> Create(SurveyModel survey, string adminEmail);
        Task<bool> Update(int id, SurveyModel updatedSurvey);
        Task<bool> Delete(int id);
    }
}
