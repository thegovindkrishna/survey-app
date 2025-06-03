using Microsoft.EntityFrameworkCore;
using Survey.Data;
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
        public async Task<List<SurveyModel>> GetAll()
        {
            return await _context.Surveys.Include(s => s.Questions).ToListAsync();
        }

        /// <summary>
        /// Get a single survey by its ID.
        /// </summary>
        public async Task<SurveyModel?> GetById(int id)
        {
            return await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Update the fields of a specific survey.
        /// </summary>
        public async Task<SurveyModel?> Update(int id, SurveyModel updatedSurvey)
        {
            var existing = await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == id);
                
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
    }
}
