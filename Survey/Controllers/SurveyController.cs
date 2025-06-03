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
            var json = System.Text.Json.JsonSerializer.Serialize(surveys);
            Console.WriteLine("Surveys JSON: " + json);
            return Ok(surveys);
        }

        /// <summary>
        /// Get details of a specific survey.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Console.WriteLine($"Getting survey with ID: {id}");
            var survey = await _surveyService.GetById(id);
            Console.WriteLine($"Found survey: {System.Text.Json.JsonSerializer.Serialize(survey)}");
            return survey != null ? Ok(survey) : NotFound();
        }

        /// <summary>
        /// Update a survey by ID.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SurveyModel survey)
        {
            Console.WriteLine($"Updating survey {id} with data: {System.Text.Json.JsonSerializer.Serialize(survey)}");
            var updatedSurvey = await _surveyService.Update(id, survey).ConfigureAwait(false);
            Console.WriteLine($"Update result: {System.Text.Json.JsonSerializer.Serialize(updatedSurvey)}");
            return updatedSurvey != null ? Ok(updatedSurvey) : NotFound();
        }

        /// <summary>
        /// Delete a survey by ID.
        /// await and async 
        /// 
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _surveyService.Delete(id);
            return success ? Ok("Deleted successfully.") : NotFound();
        }
    }
}
