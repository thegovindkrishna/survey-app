using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Survey.Models.Dtos;
using Survey.Services;

using Microsoft.Extensions.Logging;

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
        private readonly IMapper _mapper;
        private readonly ILogger<SurveyResultsController> _logger;

        /// <summary>
        /// Initializes a new instance of the SurveyResultsController with the specified results service.
        /// </summary>
        /// <param name="resultsService">The service for handling survey results operations</param>
        /// <param name="mapper">The AutoMapper instance for object mapping</param>
        /// <param name="logger">The logger instance</param>
        public SurveyResultsController(ISurveyResultsService resultsService, IMapper mapper, ILogger<SurveyResultsController> logger)
        {
            _resultsService = resultsService;
            _mapper = mapper;
            _logger = logger;
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
            _logger.LogInformation("Attempting to get results for survey {SurveyId}", surveyId);
            try
            {
                var results = await _resultsService.GetSurveyResults(surveyId);
                var resultsDto = _mapper.Map<SurveyResultsDto>(results);
                _logger.LogInformation("Successfully retrieved results for survey {SurveyId}", surveyId);
                return Ok(resultsDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Failed to get results for survey {SurveyId}: {ErrorMessage}", surveyId, ex.Message);
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
            _logger.LogInformation("Attempting to export survey {SurveyId} responses to CSV", surveyId);
            try
            {
                var csvBytes = await _resultsService.ExportToCsv(surveyId);
                _logger.LogInformation("Successfully exported survey {SurveyId} responses to CSV", surveyId);
                return File(csvBytes, "text/csv", $"survey_{surveyId}_responses.csv");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Failed to export survey {SurveyId} responses to CSV: {ErrorMessage}", surveyId, ex.Message);
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
            _logger.LogInformation("Attempting to generate share link for survey {SurveyId}", surveyId);
            try
            {
                var shareLink = await _resultsService.GenerateShareLink(surveyId);
                _logger.LogInformation("Successfully generated share link for survey {SurveyId}", surveyId);
                return Ok(new { shareLink });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Failed to generate share link for survey {SurveyId}: {ErrorMessage}", surveyId, ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
} 