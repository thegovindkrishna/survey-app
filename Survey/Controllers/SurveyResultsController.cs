using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Services;

namespace Survey.Controllers
{
    /// <summary>
    /// Controller for managing survey results and reports.
    /// Provides endpoints for retrieving aggregated results, exporting responses, and generating shareable links.
    /// All endpoints require Admin role authorization.
    /// </summary>
    [ApiController]
    [Route("api/surveys/{surveyId}")]
    [Authorize(Roles = "Admin")]
    public class SurveyResultsController : ControllerBase
    {
        private readonly ISurveyResultsService _resultsService;

        /// <summary>
        /// Initializes a new instance of the SurveyResultsController with the specified results service.
        /// </summary>
        /// <param name="resultsService">The service for handling survey results operations</param>
        public SurveyResultsController(ISurveyResultsService resultsService)
        {
            _resultsService = resultsService;
        }

        /// <summary>
        /// Gets aggregated results for a survey including response counts and statistics.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <returns>
        /// 200 OK with aggregated survey results if successful,
        /// 400 Bad Request if survey not found
        /// </returns>
        [HttpGet("results")]
        public async Task<IActionResult> GetResults(int surveyId)
        {
            try
            {
                var results = await _resultsService.GetSurveyResults(surveyId);
                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Exports survey responses to CSV format for data analysis.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <returns>
        /// 200 OK with CSV file download if successful,
        /// 400 Bad Request if survey not found
        /// </returns>
        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv(int surveyId)
        {
            try
            {
                var csvBytes = await _resultsService.ExportToCsv(surveyId);
                return File(csvBytes, "text/csv", $"survey_{surveyId}_responses.csv");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Exports survey responses to PDF format for reporting.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <returns>
        /// 200 OK with PDF file download if successful,
        /// 400 Bad Request if survey not found
        /// </returns>
        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportToPdf(int surveyId)
        {
            try
            {
                var pdfBytes = await _resultsService.ExportToPdf(surveyId);
                return File(pdfBytes, "application/pdf", $"survey_{surveyId}_responses.pdf");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Generates a shareable link for a survey.
        /// Updates the survey's ShareLink property in the database.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <returns>
        /// 200 OK with the shareable link if successful,
        /// 400 Bad Request if survey not found
        /// </returns>
        [HttpGet("share-link")]
        public async Task<IActionResult> GenerateShareLink(int surveyId)
        {
            try
            {
                var shareLink = await _resultsService.GenerateShareLink(surveyId);
                return Ok(new { shareLink });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
} 