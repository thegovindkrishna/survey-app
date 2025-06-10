using Microsoft.EntityFrameworkCore;
using Survey.Data;
using Survey.Models;
using SurveyModel = Survey.Models.Survey;
using Question = Survey.Models.Question;

namespace Survey.Services
{
    public class SurveyService : ISurveyService
    {
        private readonly AppDbContext _context;

        public SurveyService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a new survey and save it to the database.
        /// admin only
        /// </summary>
        public async Task<SurveyModel> Create(SurveyModel survey, string adminEmail)
        {
            survey.CreatedBy = adminEmail;
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            return survey;
        }

        /// <summary>
        /// Get all surveys from the database.
        /// </summary>
        public async Task<IEnumerable<SurveyModel>> GetAll()
        {
            return await _context.Surveys.ToListAsync();
        }

        /// <summary>
        /// Get a single survey by its ID.
        /// </summary>
        public async Task<SurveyModel?> GetById(int id)
        {
            return await _context.Surveys.FindAsync(id);
        }

        /// <summary>
        /// Update the fields of a specific survey.
        /// </summary>
        public async Task<SurveyModel?> Update(int id, SurveyModel updatedSurvey)
        {
            var existing = await _context.Surveys.FindAsync(id);
                
            if (existing == null) return null;

            // Update basic properties
            existing.Title = updatedSurvey.Title;
            existing.Description = updatedSurvey.Description;

            // Update questions
            existing.Questions.Clear();
            foreach (var question in updatedSurvey.Questions)
            {
                var newQuestion = new Question
                {
                    QuestionText = question.QuestionText,
                    type = question.type,
                    required = question.required,
                    options = question.options,
                    maxRating = question.maxRating
                };
                existing.Questions.Add(newQuestion);
            }

            try
            {
                await _context.SaveChangesAsync();
                // Fetch the updated survey to ensure we return the complete object
                return await _context.Surveys.FindAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating survey: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete a survey by its ID.
        /// </summary>
        public async Task<bool> Delete(int id)
        {
            var survey = await _context.Surveys.FindAsync(id);
            if (survey == null) return false;

            _context.Surveys.Remove(survey);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<SurveyResponse> SubmitResponse(SurveyResponse response)
        {
            var survey = await _context.Surveys.FindAsync(response.SurveyId);
            if (survey == null)
                throw new ArgumentException("Survey not found");

            // Validate that all required questions are answered
            var requiredQuestions = survey.Questions.Where(q => q.required).ToList();
            var answeredQuestionIds = response.Responses.Select(r => r.QuestionId).ToList();
            var missingRequiredQuestions = requiredQuestions.Where(q => !answeredQuestionIds.Contains(q.Id)).ToList();

            if (missingRequiredQuestions.Any())
                throw new ArgumentException($"Missing required questions: {string.Join(", ", missingRequiredQuestions.Select(q => q.QuestionText))}");

            _context.SurveyResponses.Add(response);
            await _context.SaveChangesAsync();
            return response;
        }

        public async Task<IEnumerable<SurveyResponse>> GetResponses(int surveyId)
        {
            var survey = await _context.Surveys.FindAsync(surveyId);
            if (survey == null)
                throw new ArgumentException("Survey not found");

            return await _context.SurveyResponses
                .Where(r => r.SurveyId == surveyId)
                .ToListAsync();
        }

        public async Task<SurveyResponse?> GetResponse(int surveyId, int responseId)
        {
            var survey = await _context.Surveys.FindAsync(surveyId);
            if (survey == null)
                throw new ArgumentException("Survey not found");

            return await _context.SurveyResponses
                .FirstOrDefaultAsync(r => r.SurveyId == surveyId && r.Id == responseId);
        }
    }
}
