using Survey.Models;
using SurveyModel = Survey.Models.Survey;

namespace Survey.Services
{
    public interface ISurveyService
    {
        Task<SurveyModel> Create(SurveyModel survey, string email);
        Task<IEnumerable<SurveyModel>> GetAll();
        Task<SurveyModel?> GetById(int id);
        Task<SurveyModel?> Update(int id, SurveyModel survey);
        Task<bool> Delete(int id);

        /// <summary>
        /// Submits a survey response.
        /// </summary>
        /// <param name="response">The survey response to submit.</param>
        /// <returns>The submitted survey response.</returns>
        Task<SurveyResponse> SubmitResponse(SurveyResponse response);

        /// <summary>
        /// Gets all responses for a specific survey.
        /// </summary>
        /// <param name="surveyId">The ID of the survey.</param>
        /// <returns>A collection of survey responses.</returns>
        Task<IEnumerable<SurveyResponse>> GetResponses(int surveyId);

        /// <summary>
        /// Gets a specific survey response by its ID.
        /// </summary>
        /// <param name="surveyId">The ID of the survey.</param>
        /// <param name="responseId">The ID of the response.</param>
        /// <returns>The survey response, or null if not found.</returns>
        Task<SurveyResponse?> GetResponse(int surveyId, int responseId);
    }
}
