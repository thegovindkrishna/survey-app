using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Services;
using System.Security.Claims;
using SurveyModel = Survey.Models.Survey;

namespace Survey.Controllers
{
    [ApiController]
    [Route("api/surveys")]
    [Authorize(Roles = "Admin")]
    public class SurveyController : ControllerBase
    {
        private readonly ISurveyService _surveyService;

        public SurveyController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        /// <summary>
        /// Create a new survey.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SurveyModel survey)
        {
            var email = User.FindFirstValue(ClaimTypes.Name)!;
            var created = await _surveyService.Create(survey, email);
            return Ok(created);
        }

        /// <summary>
        /// Get all surveys created by the admin.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var surveys = await _surveyService.GetAll();
            return Ok(surveys);
        }

        /// <summary>
        /// Get details of a specific survey.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var survey = await _surveyService.GetById(id);
            return survey != null ? Ok(survey) : NotFound();
        }

        /// <summary>
        /// Update a survey by ID.
        /// </summary>
        [HttpPost("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SurveyModel survey)
        {
            var success = await _surveyService.Update(id, survey);
            return success ? Ok("Updated successfully.") : NotFound();
        }

        /// <summary>
        /// Delete a survey by ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _surveyService.Delete(id);
            return success ? Ok("Deleted successfully.") : NotFound();
        }
    }
}
