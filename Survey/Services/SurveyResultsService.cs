using Survey.Repositories;
using Survey.Data;
using Survey.Models;
using System.Text;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Extensions.Logging;

namespace Survey.Services
{
    /// <summary>
    /// Service implementation for generating survey results and reports.
    /// Provides functionality for aggregating survey data, exporting responses to CSV/PDF, and generating shareable links.
    /// </summary>
    public class SurveyResultsService : ISurveyResultsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SurveyResultsService> _logger;

        /// <summary>
        /// Initializes a new instance of the SurveyResultsService with the specified database context and configuration.
        /// </summary>
        /// <param name="context">The database context for survey operations</param>
        /// <param name="configuration">The configuration containing base URL settings</param>
        /// <param name="logger">The logger instance</param>
        public SurveyResultsService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<SurveyResultsService> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Gets aggregated results for a survey including response counts and statistics.
        /// Calculates average ratings for rating questions and response counts for other question types.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <returns>Aggregated survey results with question statistics</returns>
        /// <exception cref="ArgumentException">Thrown when survey not found</exception>
        public async Task<SurveyResultsModel> GetSurveyResults(int surveyId)
        {
            _logger.LogInformation("Getting survey results for survey ID: {SurveyId}", surveyId);
            var survey = await _unitOfWork.Surveys

                .GetFirstOrDefaultAsync(s => s.Id == surveyId, includeProperties: "Questions");

            if (survey == null)
            {
                _logger.LogWarning("Survey with ID: {SurveyId} not found for results.", surveyId);
                throw new ArgumentException("Survey not found");
            }

            var responses = await _unitOfWork.SurveyResponses
                .GetAllAsync(r => r.SurveyId == surveyId);

            var results = new SurveyResultsModel
            {
                SurveyId = survey.Id,
                SurveyTitle = survey.Title,
                TotalResponses = responses.Count(),
                QuestionResults = new List<QuestionResultModel>()
            };

            foreach (var question in survey.Questions)
            {
                var questionResult = new QuestionResultModel
                {
                    QuestionId = question.Id,
                    QuestionText = question.QuestionText,
                    QuestionType = question.Type,
                    ResponseCounts = new Dictionary<string, int>()
                };

                if (question.Type == "rating" && question.MaxRating.HasValue)
                {
                    var ratings = responses
                        .SelectMany(r => r.responses)
                        .Where(r => r.QuestionId == question.Id)
                        .Select(r => double.Parse(r.response))
                        .ToList();

                    if (ratings.Any())
                    {
                        questionResult.AverageRating = ratings.Average();
                    }
                }
                else
                {
                    var responseCounts = responses
                        .SelectMany(r => r.responses)
                        .Where(r => r.QuestionId == question.Id)
                        .GroupBy(r => r.response)
                        .ToDictionary(g => g.Key, g => g.Count());

                    questionResult.ResponseCounts = responseCounts;
                }

                results.QuestionResults.Add(questionResult);
            }
            _logger.LogInformation("Successfully retrieved survey results for survey ID: {SurveyId}. Total responses: {TotalResponses}", surveyId, results.TotalResponses);
            return results;
        }

        /// <summary>
        /// Exports survey responses to CSV format for data analysis.
        /// Creates a CSV file with headers for each question and rows for each response.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <returns>CSV file content as byte array</returns>
        /// <exception cref="ArgumentException">Thrown when survey not found</exception>
        public async Task<byte[]> ExportToCsv(int surveyId)
        {
            _logger.LogInformation("Exporting survey responses to CSV for survey ID: {SurveyId}", surveyId);
            var survey = await _unitOfWork.Surveys

                .GetFirstOrDefaultAsync(s => s.Id == surveyId, includeProperties: "Questions");

            if (survey == null)
            {
                _logger.LogWarning("Survey with ID: {SurveyId} not found for CSV export.", surveyId);
                throw new ArgumentException("Survey not found");
            }

            var responses = await _unitOfWork.SurveyResponses
                .GetAllAsync(r => r.SurveyId == surveyId);

            var csv = new StringBuilder();
            
            // Add headers
            csv.AppendLine("Respondent Email,Submission Date," + string.Join(",", survey.Questions.Select(q => $"Q{q.Id}: {q.QuestionText}")));

            // Add responses
            foreach (var response in responses)
            {
                var responseDict = response.responses.ToDictionary(r => r.QuestionId, r => r.response);
                var responseLine = new List<string>
                {
                    response.RespondentEmail,
                    response.SubmissionDate.ToString("yyyy-MM-dd HH:mm:ss")
                };

                foreach (var question in survey.Questions)
                {
                    responseDict.TryGetValue(question.Id, out var answer);
                    responseLine.Add(answer ?? "");
                }

                csv.AppendLine(string.Join(",", responseLine));
            }
            _logger.LogInformation("Successfully exported {Count} responses to CSV for survey ID: {SurveyId}", responses.Count(), surveyId);
            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        /// <summary>
        /// Exports survey responses to PDF format for reporting.
        /// Creates a formatted PDF document with survey title and all responses.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <returns>PDF file content as byte array</returns>
        /// <exception cref="ArgumentException">Thrown when survey not found</exception>
        public async Task<byte[]> ExportToPdf(int surveyId)
        {
            _logger.LogInformation("Exporting survey responses to PDF for survey ID: {SurveyId}", surveyId);
            var survey = await _unitOfWork.Surveys

                .GetFirstOrDefaultAsync(s => s.Id == surveyId, includeProperties: "Questions");

            if (survey == null)
            {
                _logger.LogWarning("Survey with ID: {SurveyId} not found for PDF export.", surveyId);
                throw new ArgumentException("Survey not found");
            }

            var responses = await _unitOfWork.SurveyResponses
                .GetAllAsync(r => r.SurveyId == surveyId);

            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Add title
            document.Add(new Paragraph($"Survey Results: {survey.Title}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20));

            // Add responses
            foreach (var response in responses)
            {
                document.Add(new Paragraph($"\nResponse from: {response.RespondentEmail}")
                    .SetFontSize(14));
                document.Add(new Paragraph($"Submitted on: {response.SubmissionDate:yyyy-MM-dd HH:mm:ss}")
                    .SetFontSize(12));

                foreach (var questionResponse in response.responses)
                {
                    var question = survey.Questions.FirstOrDefault(q => q.Id == questionResponse.QuestionId);
                    if (question != null)
                    {
                        document.Add(new Paragraph($"Q: {question.QuestionText}")
                            .SetFontSize(12)
                            .SetBold());
                        document.Add(new Paragraph($"A: {questionResponse.response}")
                            .SetFontSize(12));
                    }
                }
            }

            document.Close();
            _logger.LogInformation("Successfully exported {Count} responses to PDF for survey ID: {SurveyId}", responses.Count(), surveyId);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Generates a shareable link for a survey.
        /// Updates the survey's ShareLink property in the database with the generated URL.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <returns>The shareable survey link</returns>
        /// <exception cref="ArgumentException">Thrown when survey not found</exception>
        public async Task<string> GenerateShareLink(int surveyId)
        {
            _logger.LogInformation("Generating share link for survey ID: {SurveyId}", surveyId);
            var survey = await _unitOfWork.Surveys.GetByIdAsync(surveyId);
            if (survey == null)
            {
                _logger.LogWarning("Survey with ID: {SurveyId} not found for share link generation.", surveyId);
                throw new ArgumentException("Survey not found");
            }

            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:4200";
            var shareLink = $"{baseUrl}/survey/{surveyId}";

            survey.ShareLink = shareLink;
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Share link generated and saved for survey ID: {SurveyId}: {ShareLink}", surveyId, shareLink);

            return shareLink;
        }
    }
} 