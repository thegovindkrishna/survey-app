using Microsoft.EntityFrameworkCore;
using Survey.Data;
using Survey.Models;
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
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the SurveyService with the specified database context.
        /// </summary>
        /// <param name="context">The database context for survey operations</param>
        public SurveyService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new survey and saves it to the database.
        /// Sets the CreatedBy property to the admin's email address.
        /// </summary>
        /// <param name="survey">The survey object to create</param>
        /// <param name="adminEmail">The email of the admin creating the survey</param>
        /// <returns>The created survey with generated ID</returns>
        public async Task<SurveyModel> Create(SurveyModel survey, string adminEmail)
        {
            survey.CreatedBy = adminEmail;
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            return survey;
        }

        /// <summary>
        /// Retrieves all surveys from the database with their questions included.
        /// </summary>
        /// <returns>A collection of all surveys with their associated questions</returns>
        public async Task<IEnumerable<SurveyModel>> GetAll()
        {
            return await _context.Surveys
                .Include(s => s.Questions)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific survey by its ID with questions included.
        /// </summary>
        /// <param name="id">The unique identifier of the survey</param>
        /// <returns>The survey with its questions, or null if not found</returns>
        public async Task<SurveyModel?> GetById(int id)
        {
            return await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Updates only the survey properties (title, description, dates, shareLink) without affecting questions.
        /// </summary>
        /// <param name="id">The unique identifier of the survey to update</param>
        /// <param name="updatedSurvey">The survey data containing updated properties</param>
        /// <returns>The updated survey with questions, or null if not found</returns>
        public async Task<SurveyModel?> UpdateProperties(int id, SurveyModel updatedSurvey)
        {
            var existing = await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (existing == null) return null;

            // Update basic properties only (no questions)
            existing.Title = updatedSurvey.Title;
            existing.Description = updatedSurvey.Description;
            existing.StartDate = updatedSurvey.StartDate;
            existing.EndDate = updatedSurvey.EndDate;
            existing.ShareLink = updatedSurvey.ShareLink;

            try
            {
                await _context.SaveChangesAsync();
                // Fetch the updated survey to ensure we return the complete object
                return await _context.Surveys
                    .Include(s => s.Questions)
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating survey properties: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Updates a survey with new data. If questions are provided, they will replace all existing questions.
        /// </summary>
        /// <param name="id">The unique identifier of the survey to update</param>
        /// <param name="updatedSurvey">The updated survey data</param>
        /// <returns>The updated survey with questions, or null if not found</returns>
        public async Task<SurveyModel?> Update(int id, SurveyModel updatedSurvey)
        {
            var existing = await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (existing == null) return null;

            // Update basic properties only
            existing.Title = updatedSurvey.Title;
            existing.Description = updatedSurvey.Description;
            existing.StartDate = updatedSurvey.StartDate;
            existing.EndDate = updatedSurvey.EndDate;
            existing.ShareLink = updatedSurvey.ShareLink;

            // Only update questions if they are explicitly provided in the request
            if (updatedSurvey.Questions != null && updatedSurvey.Questions.Any())
            {
                // Clear existing questions and add new ones
                existing.Questions.Clear();
                foreach (var question in updatedSurvey.Questions)
                {
                    var newQuestion = new Question
                    {
                        QuestionText = question.QuestionText,
                        type = question.type,
                        required = question.required,
                        options = question.options,
                        maxRating = question.maxRating,
                        SurveyId = id
                    };
                    existing.Questions.Add(newQuestion);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                // Fetch the updated survey to ensure we return the complete object
                return await _context.Surveys
                    .Include(s => s.Questions)
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating survey: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Deletes a survey and all its associated questions from the database.
        /// </summary>
        /// <param name="id">The unique identifier of the survey to delete</param>
        /// <returns>True if the survey was successfully deleted, false if not found</returns>
        public async Task<bool> Delete(int id)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (survey == null) return false;

            _context.Surveys.Remove(survey);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Submits a survey response from a user.
        /// Validates that all required questions are answered before saving.
        /// </summary>
        /// <param name="response">The survey response object containing answers</param>
        /// <returns>The submitted survey response with generated ID</returns>
        /// <exception cref="ArgumentException">Thrown when survey not found or required questions are missing</exception>
        public async Task<SurveyResponse> SubmitResponse(SurveyResponse response)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == response.SurveyId);
            if (survey == null)
                throw new ArgumentException("Survey not found");

            // Validate that all required questions are answered
            var requiredQuestions = survey.Questions.Where(q => q.required).ToList();
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
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <returns>A collection of all responses for the specified survey</returns>
        /// <exception cref="ArgumentException">Thrown when survey not found</exception>
        public async Task<IEnumerable<SurveyResponse>> GetResponses(int surveyId)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == surveyId);
            if (survey == null)
                throw new ArgumentException("Survey not found");

            return await _context.SurveyResponses
                .Where(r => r.SurveyId == surveyId)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific survey response by its ID.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <param name="responseId">The unique identifier of the response</param>
        /// <returns>The specific survey response, or null if not found</returns>
        /// <exception cref="ArgumentException">Thrown when survey not found</exception>
        public async Task<SurveyResponse?> GetResponse(int surveyId, int responseId)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == surveyId);
            if (survey == null)
                throw new ArgumentException("Survey not found");

            return await _context.SurveyResponses
                .FirstOrDefaultAsync(r => r.SurveyId == surveyId && r.Id == responseId);
        }

        /// <summary>
        /// Adds a new question to an existing survey.
        /// Sets the SurveyId on the question and adds it to the survey's question collection.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <param name="question">The question object to add</param>
        /// <returns>The updated survey with all questions, or null if survey not found</returns>
        public async Task<SurveyModel?> AddQuestion(int surveyId, Question question)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == surveyId);
                
            if (survey == null) return null;

            question.SurveyId = surveyId;
            survey.Questions.Add(question);
            
            try
            {
                await _context.SaveChangesAsync();
                return await _context.Surveys
                    .Include(s => s.Questions)
                    .FirstOrDefaultAsync(s => s.Id == surveyId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding question: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Updates a specific question within a survey.
        /// Modifies the existing question's properties without affecting other questions.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <param name="questionId">The unique identifier of the question to update</param>
        /// <param name="updatedQuestion">The updated question data</param>
        /// <returns>The updated survey with all questions, or null if survey or question not found</returns>
        public async Task<SurveyModel?> UpdateQuestion(int surveyId, int questionId, Question updatedQuestion)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == surveyId);
                
            if (survey == null) return null;

            var existingQuestion = survey.Questions.FirstOrDefault(q => q.Id == questionId);
            if (existingQuestion == null) return null;

            // Update the existing question properties
            existingQuestion.QuestionText = updatedQuestion.QuestionText;
            existingQuestion.type = updatedQuestion.type;
            existingQuestion.required = updatedQuestion.required;
            existingQuestion.options = updatedQuestion.options;
            existingQuestion.maxRating = updatedQuestion.maxRating;
            
            try
            {
                await _context.SaveChangesAsync();
                return await _context.Surveys
                    .Include(s => s.Questions)
                    .FirstOrDefaultAsync(s => s.Id == surveyId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating question: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Removes a specific question from a survey.
        /// Deletes the question from the survey's question collection.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <param name="questionId">The unique identifier of the question to delete</param>
        /// <returns>The updated survey with remaining questions, or null if survey or question not found</returns>
        public async Task<SurveyModel?> DeleteQuestion(int surveyId, int questionId)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == surveyId);
                
            if (survey == null) return null;

            var question = survey.Questions.FirstOrDefault(q => q.Id == questionId);
            if (question == null) return null;

            survey.Questions.Remove(question);
            
            try
            {
                await _context.SaveChangesAsync();
                return await _context.Surveys
                    .Include(s => s.Questions)
                    .FirstOrDefaultAsync(s => s.Id == surveyId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting question: {ex.Message}");
                throw;
            }
        }
    }
}
