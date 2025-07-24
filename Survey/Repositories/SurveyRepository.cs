using Microsoft.EntityFrameworkCore;
using Survey.Data;
using Survey.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using SurveyModel = Survey.Models.Survey;

namespace Survey.Repositories
{
    public class SurveyRepository : Repository<SurveyModel>, ISurveyRepository
    {
        public SurveyRepository(AppDbContext context) : base(context)
        {
        } 

        public async Task<IEnumerable<SurveyModel>> GetAllWithQuestionsAsync()
        {
            return await _context.Surveys
                .Include(s => s.Questions)
                .ToListAsync();
        }

        public async Task<SurveyModel?> GetByIdWithQuestionsAsync(int id)
        {
            return await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        // Add other specific methods here
    }
}
