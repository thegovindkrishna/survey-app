using Survey.Models;
using SurveyModel = Survey.Models.Survey;
using Question = Survey.Models.Question;

namespace Survey.Services
{
    /// <summary>
    /// Service interface for managing surveys, questions, and responses.
    /// Provides CRUD operations for surveys and handles survey response submissions.
    /// </summary>
    public interface ISurveyService
    {
        /// <summary>
        /// Creates a new survey in the database.
        /// </summary>
        /// <param name="survey">The survey object to create</param>
        /// <param name="email">The email of the admin creating the survey</param>
        /// <returns>The created survey with generated ID</returns>
        Task<SurveyModel> Create(SurveyModel survey, string email);

        /// <summary>
        /// Retrieves all surveys from the database with their questions included.
        /// </summary>
        /// <returns>A collection of all surveys with their associated questions</returns>
        Task<IEnumerable<SurveyModel>> GetAll();

        /// <summary>
        /// Retrieves a specific survey by its ID with questions included.
        /// </summary>
        /// <param name="id">The unique identifier of the survey</param>
        /// <returns>The survey with its questions, or null if not found</returns>
        Task<SurveyModel?> GetById(int id);

        /// <summary>
        /// Updates a survey with new data. If questions are provided, they will replace all existing questions.
        /// </summary>
        /// <param name="id">The unique identifier of the survey to update</param>
        /// <param name="survey">The updated survey data</param>
        /// <returns>The updated survey with questions, or null if not found</returns>
        Task<SurveyModel?> Update(int id, SurveyModel survey);

        /// <summary>
        /// Updates only the survey properties (title, description, dates, shareLink) without affecting questions.
        /// </summary>
        /// <param name="id">The unique identifier of the survey to update</param>
        /// <param name="survey">The survey data containing updated properties</param>
        /// <returns>The updated survey with questions, or null if not found</returns>
        Task<SurveyModel?> UpdateProperties(int id, SurveyModel survey);

        /// <summary>
        /// Deletes a survey and all its associated questions from the database.
        /// </summary>
        /// <param name="id">The unique identifier of the survey to delete</param>
        /// <returns>True if the survey was successfully deleted, false if not found</returns>
        Task<bool> Delete(int id);

        // Question-specific methods

        /// <summary>
        /// Adds a new question to an existing survey.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <param name="question">The question object to add</param>
        /// <returns>The updated survey with all questions, or null if survey not found</returns>
        Task<SurveyModel?> AddQuestion(int surveyId, Question question);

        /// <summary>
        /// Updates a specific question within a survey.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <param name="questionId">The unique identifier of the question to update</param>
        /// <param name="question">The updated question data</param>
        /// <returns>The updated survey with all questions, or null if survey or question not found</returns>
        Task<SurveyModel?> UpdateQuestion(int surveyId, int questionId, Question question);

        /// <summary>
        /// Removes a specific question from a survey.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <param name="questionId">The unique identifier of the question to delete</param>
        /// <returns>The updated survey with remaining questions, or null if survey or question not found</returns>
        Task<SurveyModel?> DeleteQuestion(int surveyId, int questionId);

        /// <summary>
        /// Submits a survey response from a user.
        /// Validates that all required questions are answered before saving.
        /// </summary>
        /// <param name="response">The survey response object containing answers</param>
        /// <returns>The submitted survey response with generated ID</returns>
        /// <exception cref="ArgumentException">Thrown when survey not found or required questions are missing</exception>
        Task<SurveyResponse> SubmitResponse(SurveyResponse response);

        /// <summary>
        /// Retrieves all responses for a specific survey.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <returns>A collection of all responses for the specified survey</returns>
        /// <exception cref="ArgumentException">Thrown when survey not found</exception>
        Task<IEnumerable<SurveyResponse>> GetResponses(int surveyId);

        /// <summary>
        /// Retrieves a specific survey response by its ID.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <param name="responseId">The unique identifier of the response</param>
        /// <returns>The specific survey response, or null if not found</returns>
        /// <exception cref="ArgumentException">Thrown when survey not found</exception>
        Task<SurveyResponse?> GetResponse(int surveyId, int responseId);
    }
}
