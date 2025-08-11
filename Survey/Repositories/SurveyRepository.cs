using Microsoft.EntityFrameworkCore;
using Survey.Data;
using Survey.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SurveyModel = Survey.Models.SurveyModel;

namespace Survey.Repositories
{
    public class SurveyRepository : Repository<SurveyModel>, ISurveyRepository
    {
        public SurveyRepository(AppDbContext context) : base(context)
        {
        } 

        public async Task<PagedList<SurveyModel>> GetAllWithQuestionsAsync(PaginationParams paginationParams, string? sortBy = null, string? sortOrder = null)
        {
            var query = _context.Surveys.Include(s => s.Questions).AsQueryable();
            // Add sorting support
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy.ToLower() == "startdate")
                {
                    query = (sortOrder?.ToLower() == "desc")
                        ? query.OrderByDescending(s => s.StartDate)
                        : query.OrderBy(s => s.StartDate);
                }
                else if (sortBy.ToLower() == "id")
                {
                    query = (sortOrder?.ToLower() == "desc")
                        ? query.OrderByDescending(s => s.Id)
                        : query.OrderBy(s => s.Id);
                }
                // Add more sort options as needed
            }
            return await PagedList<SurveyModel>.CreateAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
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
