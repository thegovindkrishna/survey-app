using Microsoft.EntityFrameworkCore;
using System.Linq;
using Survey.Data;
using Survey.Models;
using Survey.Models.Dtos;
using Survey.Repositories;
using Microsoft.Extensions.Logging;
using SurveyModel = Survey.Models.SurveyModel;
using QuestionModel = Survey.Models.QuestionModel;

namespace Survey.Services
{
    /// <summary>
    /// Service implementation for managing surveys, questions, and responses.
    /// Provides CRUD operations for surveys and handles survey response submissions.
    /// </summary>
    public class SurveyService : ISurveyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SurveyService> _logger;

        /// <summary>
        /// Initializes a new instance of the SurveyService with the specified unit of work and context.
        /// </summary>
        /// <param name="unitOfWork">The unit of work for accessing repositories</param>
        /// <param name="context">The database context for operations not yet in repositories</param>
        /// <param name="logger">The logger instance</param>
        public SurveyService(IUnitOfWork unitOfWork, ILogger<SurveyService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new survey and saves it to the database.
        /// </summary>
        public async Task<SurveyModel> Create(SurveyCreateDto surveyDto, string adminEmail)
        {
            _logger.LogInformation("Creating new survey with title '{Title}' by admin {AdminEmail}", surveyDto.Title, adminEmail);
            var survey = new SurveyModel
            {
                Title = surveyDto.Title,
                Description = surveyDto.Description,
                StartDate = surveyDto.StartDate,
                EndDate = surveyDto.EndDate,
                CreatedBy = adminEmail,
                Questions = surveyDto.Questions.Select(qDto => new QuestionModel
                {
                    QuestionText = qDto.QuestionText,
                    Type = qDto.Type,
                    Required = qDto.Required,
                    Options = qDto.Type == "short answer" ? null : qDto.Options,
                    MaxRating = qDto.Type == "rating" ? qDto.MaxRating : null
                }).ToList()
            };

            await _unitOfWork.Surveys.AddAsync(survey);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Survey '{Title}' (ID: {SurveyId}) created successfully.", survey.Title, survey.Id);
            return survey;
        }

        /// <summary>
        /// Retrieves all surveys from the database with their questions included.
        /// </summary>
        public async Task<IEnumerable<SurveyDto>> GetAll()
        {
            _logger.LogInformation("Retrieving all surveys.");
            var surveys = await _unitOfWork.Surveys.GetAllWithQuestionsAsync();
            _logger.LogInformation("Retrieved {Count} surveys.", surveys.Count());
            return surveys.Select(s => new SurveyDto(s.Id, s.Title, s.Description, s.StartDate, s.EndDate, s.CreatedBy, s.ShareLink,
                s.Questions.Select(q => new QuestionDto(q.Id, q.QuestionText, q.Type, q.Required, q.Options, q.MaxRating)).ToList()));
        }

        public async Task<PagedList<SurveyDto>> GetAll(PaginationParams paginationParams)
        {
            _logger.LogInformation("Retrieving all surveys.");
            var surveys = await _unitOfWork.Surveys.GetAllWithQuestionsAsync(paginationParams);
            _logger.LogInformation("Retrieved {Count} surveys.", surveys.Count());
            var surveyDtos = surveys.Select(s => new SurveyDto(s.Id, s.Title, s.Description, s.StartDate, s.EndDate, s.CreatedBy, s.ShareLink,
                                 s.Questions.Select(q => new QuestionDto(q.Id, q.QuestionText, q.Type, q.Required, q.Options, q.MaxRating)).ToList()));

            return new PagedList<SurveyDto>(surveyDtos.ToList(), surveys.TotalCount, surveys.CurrentPage, surveys.PageSize);
        }

        /// <summary>
        /// Retrieves a specific survey by its ID with questions included.
        /// </summary>
        public async Task<SurveyDto?> GetById(int id)
        {
            _logger.LogInformation("Retrieving survey with ID: {SurveyId}", id);
            var survey = await _unitOfWork.Surveys.GetByIdWithQuestionsAsync(id);
            if (survey == null)
            {
                _logger.LogWarning("Survey with ID: {SurveyId} not found.", id);
                return null;
            }

            _logger.LogInformation("Survey with ID: {SurveyId} retrieved successfully.", id);
            return new SurveyDto(survey.Id, survey.Title, survey.Description, survey.StartDate, survey.EndDate, survey.CreatedBy, survey.ShareLink,
                survey.Questions.Select(q => new QuestionDto(q.Id, q.QuestionText, q.Type, q.Required, q.Options, q.MaxRating)).ToList());
        }

        /// <summary>
        /// Updates a survey with new data, replacing all existing questions.
        /// </summary>
        public async Task<SurveyModel> Update(int id, SurveyUpdateDto surveyDto)
        {
            _logger.LogInformation("Updating survey with ID: {SurveyId}", id);
            var existing = await _unitOfWork.Surveys.GetByIdWithQuestionsAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Update failed: Survey with ID: {SurveyId} not found.", id);
                return null;
            }

            existing.Title = surveyDto.Title;
            existing.Description = surveyDto.Description;
            existing.StartDate = surveyDto.StartDate;
            existing.EndDate = surveyDto.EndDate;

            existing.Questions.Clear();
            foreach (var qDto in surveyDto.Questions)
            {
                existing.Questions.Add(new QuestionModel
                {
                    QuestionText = qDto.QuestionText,
                    Type = qDto.Type,
                    Required = qDto.Required,
                    Options = qDto.Type == "short answer" ? null : qDto.Options,
                    MaxRating = qDto.Type == "rating" ? qDto.MaxRating : null,
                    SurveyId = id
                });
            }

            _unitOfWork.Surveys.Update(existing);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Survey with ID: {SurveyId} updated successfully.", id);
            return existing;
        }
        
        /// <summary>
        /// Updates only the survey properties (title, description, dates, shareLink) without affecting questions.
        /// </summary>
        public async Task<SurveyModel?> UpdateProperties(int id, SurveyModel updatedSurvey)
        {
            _logger.LogInformation("Updating properties for survey with ID: {SurveyId}", id);
            var existing = await _unitOfWork.Surveys.GetByIdAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Update properties failed: Survey with ID: {SurveyId} not found.", id);
                return null;
            }

            existing.Title = updatedSurvey.Title;
            existing.Description = updatedSurvey.Description;
            existing.StartDate = updatedSurvey.StartDate;
            existing.EndDate = updatedSurvey.EndDate;
            existing.ShareLink = updatedSurvey.ShareLink;

            _unitOfWork.Surveys.Update(existing);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Properties for survey with ID: {SurveyId} updated successfully.", id);
            return await _unitOfWork.Surveys.GetByIdWithQuestionsAsync(id);
        }

        /// <summary>
        /// Deletes a survey and all its associated questions.
        /// </summary>
        public async Task<bool> Delete(int id)
        {
            _logger.LogInformation("Deleting survey with ID: {SurveyId}", id);
            var survey = await _unitOfWork.Surveys.GetByIdAsync(id);
            if (survey == null)
            {
                _logger.LogWarning("Delete failed: Survey with ID: {SurveyId} not found.", id);
                return false;
            }

            _unitOfWork.Surveys.Delete(survey);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Survey with ID: {SurveyId} deleted successfully.", id);
            return true;
        }

        /// <summary>
        /// Adds a new question to an existing survey.
        /// </summary>
        public async Task<SurveyModel?> AddQuestion(int surveyId, QuestionCreateDto questionDto)
        {
            _logger.LogInformation("Adding question to survey with ID: {SurveyId}", surveyId);
            var survey = await _unitOfWork.Surveys.GetByIdWithQuestionsAsync(surveyId);
            if (survey == null)
            {
                _logger.LogWarning("Add question failed: Survey with ID: {SurveyId} not found.", surveyId);
                return null;
            }

            var newQuestion = new QuestionModel
            {
                QuestionText = questionDto.QuestionText,
                Type = questionDto.Type,
                Required = questionDto.Required,
                Options = questionDto.Options,
                MaxRating = questionDto.MaxRating,
                SurveyId = surveyId
            };
            survey.Questions.Add(newQuestion);

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Question added successfully to survey with ID: {SurveyId}", surveyId);
            return survey;
        }

        /// <summary>
        /// Updates a specific question within a survey.
        /// </summary>
        public async Task<SurveyModel?> UpdateQuestion(int surveyId, int questionId, QuestionUpdateDto questionDto)
        {
            _logger.LogInformation("Updating question {QuestionId} in survey {SurveyId}", questionId, surveyId);
            var survey = await _unitOfWork.Surveys.GetByIdWithQuestionsAsync(surveyId);
            if (survey == null)
            {
                _logger.LogWarning("Update question failed: Survey with ID: {SurveyId} not found.", surveyId);
                return null;
            }

            var existingQuestion = survey.Questions.FirstOrDefault(q => q.Id == questionId);
            if (existingQuestion == null)
            {
                _logger.LogWarning("Update question failed: Question with ID: {QuestionId} not found in survey {SurveyId}", questionId, surveyId);
                return null;
            }

            existingQuestion.QuestionText = questionDto.QuestionText;
            existingQuestion.Type = questionDto.Type;
            existingQuestion.Required = questionDto.Required;
            existingQuestion.Options = questionDto.Options;
            existingQuestion.MaxRating = questionDto.MaxRating;

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Question {QuestionId} updated successfully in survey {SurveyId}", questionId, surveyId);
            return survey;
        }

        /// <summary>
        /// Removes a specific question from a survey.
        /// </summary>
        public async Task<SurveyModel?> DeleteQuestion(int surveyId, int questionId)
        {
            _logger.LogInformation("Deleting question {QuestionId} from survey {SurveyId}", questionId, surveyId);
            var survey = await _unitOfWork.Surveys.GetByIdWithQuestionsAsync(surveyId);
            if (survey == null)
            {
                _logger.LogWarning("Delete question failed: Survey with ID: {SurveyId} not found.", surveyId);
                return null;
            }

            var question = survey.Questions.FirstOrDefault(q => q.Id == questionId);
            if (question == null)
            {
                _logger.LogWarning("Delete question failed: Question with ID: {QuestionId} not found in survey {SurveyId}", questionId, surveyId);
                return null;
            }

            survey.Questions.Remove(question);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Question {QuestionId} deleted successfully from survey {SurveyId}", questionId, surveyId);
            return survey;
        }

        // --- Methods still using DbContext directly (to be refactored later) ---

        /// <summary>
        /// Submits a survey response from a user.
        /// </summary>
        public async Task<SurveyResponseModel> SubmitResponse(SurveyResponseModel response)
        {
            _logger.LogInformation("Submitting response for survey {SurveyId} by {RespondentEmail}", response.SurveyId, response.RespondentEmail);
            var survey = await _unitOfWork.Surveys.GetByIdWithQuestionsAsync(response.SurveyId);
            if (survey == null)
            {
                _logger.LogError("Submit response failed: Survey {SurveyId} not found.", response.SurveyId);
                throw new ArgumentException("Survey not found");
            }

            var requiredQuestions = survey.Questions.Where(q => q.Required).ToList();
            var answeredQuestionIds = response.responses.Select(r => r.QuestionId).ToList();
            var missingRequiredQuestions = requiredQuestions.Where(q => !answeredQuestionIds.Contains(q.Id)).ToList();

            if (missingRequiredQuestions.Any())
            {
                var missingQuestionsText = string.Join(", ", missingRequiredQuestions.Select(q => q.QuestionText));
                _logger.LogError("Submit response failed for survey {SurveyId}: Missing required questions: {MissingQuestions}", response.SurveyId, missingQuestionsText);
                throw new ArgumentException($"Missing required questions: {missingQuestionsText}");
            }

            await _unitOfWork.SurveyResponses.AddAsync(response);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Response submitted successfully for survey {SurveyId} by {RespondentEmail}", response.SurveyId, response.RespondentEmail);
            return response;
        }

        /// <summary>
        /// Retrieves all responses for a specific survey.
        /// </summary>
        public async Task<IEnumerable<SurveyResponseModel>> GetResponses(int surveyId)
        {
            _logger.LogInformation("Retrieving all responses for survey {SurveyId}", surveyId);
            var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId);
            if (survey == null)
            {
                _logger.LogError("Get responses failed: Survey {SurveyId} not found.", surveyId);
                throw new ArgumentException("Survey not found");
            }

            var responses = await _unitOfWork.SurveyResponses.GetAllAsync(r => r.SurveyId == surveyId);
            _logger.LogInformation("Retrieved {Count} responses for survey {SurveyId}", responses.Count(), surveyId);
            return responses;
        }

        /// <summary>
        // Retrieves a specific survey response by its ID.
        /// </summary>
        public async Task<SurveyResponseModel?> GetResponse(int surveyId, int responseId)
        {
            _logger.LogInformation("Retrieving response {ResponseId} for survey {SurveyId}", responseId, surveyId);
            var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId);
            if (survey == null)
            {
                _logger.LogError("Get response failed: Survey {SurveyId} not found.", surveyId);
                throw new ArgumentException("Survey not found");
            }

            var response = await _unitOfWork.SurveyResponses.GetFirstOrDefaultAsync(r => r.SurveyId == surveyId && r.Id == responseId);
            if (response == null)
            {
                _logger.LogWarning("Response {ResponseId} not found for survey {SurveyId}", responseId, surveyId);
            }
            else
            {
                _logger.LogInformation("Response {ResponseId} retrieved successfully for survey {SurveyId}", responseId, surveyId);
            }
            return response;
        }

        public string GetQuestionText(int questionId)
        {
            _logger.LogInformation("Retrieving question text for question ID: {QuestionId}", questionId);
            var question = _unitOfWork.Questions.GetByIdAsync(questionId).Result;
            if (question == null)
            {
                _logger.LogWarning("Question text not found for question ID: {QuestionId}", questionId);
            }
            return question?.QuestionText ?? string.Empty;
        }
    }
}