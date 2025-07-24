using Microsoft.EntityFrameworkCore;
using Survey.Data;
using Survey.Models;
using Survey.Models.Dtos;
using Survey.Repositories;
using SurveyModel = Survey.Models.Survey;
using Question = Survey.Models.Question;

namespace Survey.Services
{
    /// <summary>
    /// Service implementation for managing surveys, questions, and responses.
    /// Provides CRUD operations for surveys and handles survey response submissions.
    /// </summary>
    public class SurveyService : ISurveyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context; // Keep for entities without a repository yet

        /// <summary>
        /// Initializes a new instance of the SurveyService with the specified unit of work and context.
        /// </summary>
        /// <param name="unitOfWork">The unit of work for accessing repositories</param>
        /// <param name="context">The database context for operations not yet in repositories</param>
        public SurveyService(IUnitOfWork unitOfWork, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        /// <summary>
        /// Creates a new survey and saves it to the database.
        /// </summary>
        public async Task<SurveyModel> Create(SurveyCreateDto surveyDto, string adminEmail)
        {
            var survey = new SurveyModel
            {
                Title = surveyDto.Title,
                Description = surveyDto.Description,
                StartDate = surveyDto.StartDate,
                EndDate = surveyDto.EndDate,
                CreatedBy = adminEmail,
                Questions = surveyDto.Questions.Select(qDto => new Question
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
            return survey;
        }

        /// <summary>
        /// Retrieves all surveys from the database with their questions included.
        /// </summary>
        public async Task<IEnumerable<SurveyDto>> GetAll()
        {
            var surveys = await _unitOfWork.Surveys.GetAllWithQuestionsAsync();
            return surveys.Select(s => new SurveyDto(s.Id, s.Title, s.Description, s.StartDate, s.EndDate, s.CreatedBy, s.ShareLink,
                s.Questions.Select(q => new QuestionDto(q.Id, q.QuestionText, q.Type, q.Required, q.Options, q.MaxRating)).ToList()));
        }

        /// <summary>
        /// Retrieves a specific survey by its ID with questions included.
        /// </summary>
        public async Task<SurveyDto?> GetById(int id)
        {
            var survey = await _unitOfWork.Surveys.GetByIdWithQuestionsAsync(id);
            if (survey == null) return null;

            return new SurveyDto(survey.Id, survey.Title, survey.Description, survey.StartDate, survey.EndDate, survey.CreatedBy, survey.ShareLink,
                survey.Questions.Select(q => new QuestionDto(q.Id, q.QuestionText, q.Type, q.Required, q.Options, q.MaxRating)).ToList());
        }

        /// <summary>
        /// Updates a survey with new data, replacing all existing questions.
        /// </summary>
        public async Task<SurveyModel> Update(int id, SurveyUpdateDto surveyDto)
        {
            var existing = await _unitOfWork.Surveys.GetByIdWithQuestionsAsync(id);
            if (existing == null) return null;

            existing.Title = surveyDto.Title;
            existing.Description = surveyDto.Description;
            existing.StartDate = surveyDto.StartDate;
            existing.EndDate = surveyDto.EndDate;

            existing.Questions.Clear();
            foreach (var qDto in surveyDto.Questions)
            {
                existing.Questions.Add(new Question
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
            return existing;
        }
        
        /// <summary>
        /// Updates only the survey properties (title, description, dates, shareLink) without affecting questions.
        /// </summary>
        public async Task<SurveyModel?> UpdateProperties(int id, SurveyModel updatedSurvey)
        {
            var existing = await _unitOfWork.Surveys.GetByIdAsync(id);
            if (existing == null) return null;

            existing.Title = updatedSurvey.Title;
            existing.Description = updatedSurvey.Description;
            existing.StartDate = updatedSurvey.StartDate;
            existing.EndDate = updatedSurvey.EndDate;
            existing.ShareLink = updatedSurvey.ShareLink;

            _unitOfWork.Surveys.Update(existing);
            await _unitOfWork.CompleteAsync();
            return await _unitOfWork.Surveys.GetByIdWithQuestionsAsync(id);
        }

        /// <summary>
        /// Deletes a survey and all its associated questions.
        /// </summary>
        public async Task<bool> Delete(int id)
        {
            var survey = await _unitOfWork.Surveys.GetByIdAsync(id);
            if (survey == null) return false;

            _unitOfWork.Surveys.Delete(survey);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        /// <summary>
        /// Adds a new question to an existing survey.
        /// </summary>
        public async Task<SurveyModel?> AddQuestion(int surveyId, Question question)
        {
            var survey = await _unitOfWork.Surveys.GetByIdWithQuestionsAsync(surveyId);
            if (survey == null) return null;

            question.SurveyId = surveyId;
            survey.Questions.Add(question);

            await _unitOfWork.CompleteAsync();
            return survey;
        }

        /// <summary>
        /// Updates a specific question within a survey.
        /// </summary>
        public async Task<SurveyModel?> UpdateQuestion(int surveyId, int questionId, QuestionUpdateDto questionDto)
        {
            var survey = await _unitOfWork.Surveys.GetByIdWithQuestionsAsync(surveyId);
            if (survey == null) return null;

            var existingQuestion = survey.Questions.FirstOrDefault(q => q.Id == questionId);
            if (existingQuestion == null) return null;

            existingQuestion.QuestionText = questionDto.QuestionText;
            existingQuestion.Type = questionDto.Type;
            existingQuestion.Required = questionDto.Required;
            existingQuestion.Options = questionDto.Options;
            existingQuestion.MaxRating = questionDto.MaxRating;

            await _unitOfWork.CompleteAsync();
            return survey;
        }

        /// <summary>
        /// Removes a specific question from a survey.
        /// </summary>
        public async Task<SurveyModel?> DeleteQuestion(int surveyId, int questionId)
        {
            var survey = await _unitOfWork.Surveys.GetByIdWithQuestionsAsync(surveyId);
            if (survey == null) return null;

            var question = survey.Questions.FirstOrDefault(q => q.Id == questionId);
            if (question == null) return null;

            survey.Questions.Remove(question);
            await _unitOfWork.CompleteAsync();
            return survey;
        }

        // --- Methods still using DbContext directly (to be refactored later) ---

        /// <summary>
        /// Submits a survey response from a user.
        /// </summary>
        public async Task<SurveyResponse> SubmitResponse(SurveyResponse response)
        {
            var survey = await _unitOfWork.Surveys.GetByIdWithQuestionsAsync(response.SurveyId);
            if (survey == null)
                throw new ArgumentException("Survey not found");

            var requiredQuestions = survey.Questions.Where(q => q.Required).ToList();
            var answeredQuestionIds = response.responses.Select(r => r.QuestionId).ToList();
            var missingRequiredQuestions = requiredQuestions.Where(q => !answeredQuestionIds.Contains(q.Id)).ToList();

            if (missingRequiredQuestions.Any())
                throw new ArgumentException($"Missing required questions: {string.Join(", ", missingRequiredQuestions.Select(q => q.QuestionText))}");

            _context.SurveyResponses.Add(response);
            await _context.SaveChangesAsync();
            return response;
        }

        /// <summary>
        /// Retrieves all responses for a specific survey.
        /// </summary>
        public async Task<IEnumerable<SurveyResponse>> GetResponses(int surveyId)
        {
            var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId);
            if (survey == null)
                throw new ArgumentException("Survey not found");

            return await _context.SurveyResponses
                .Where(r => r.SurveyId == surveyId)
                .ToListAsync();
        }

        /// <summary>
        // Retrieves a specific survey response by its ID.
        /// </summary>
        public async Task<SurveyResponse?> GetResponse(int surveyId, int responseId)
        {
            var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId);
            if (survey == null)
                throw new ArgumentException("Survey not found");

            return await _context.SurveyResponses
                .FirstOrDefaultAsync(r => r.SurveyId == surveyId && r.Id == responseId);
        }

        public string GetQuestionText(int questionId)
        {
            var question = _context.Questions.FirstOrDefault(q => q.Id == questionId);
            return question?.QuestionText ?? string.Empty;
        }
    }
}