using Microsoft.EntityFrameworkCore;
using Survey.Data;
using SurveyModel = Survey.Models.Survey;

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
        public async Task<bool> Update(int id, SurveyModel updatedSurvey)
        {
            var existing = await _context.Surveys.FindAsync(id);
            if (existing == null) return false;

            existing.Title = updatedSurvey.Title;
            existing.Description = updatedSurvey.Description;
            existing.StartDate = updatedSurvey.StartDate;
            existing.EndDate = updatedSurvey.EndDate;

            await _context.SaveChangesAsync();
            return true;
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
