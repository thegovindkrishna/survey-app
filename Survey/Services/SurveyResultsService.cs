using Microsoft.EntityFrameworkCore;
using Survey.Data;
using Survey.Models;
using System.Text;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace Survey.Services
{
    /// <summary>
    /// Service implementation for generating survey results and reports.
    /// Provides functionality for aggregating survey data, exporting responses to CSV/PDF, and generating shareable links.
    /// </summary>
    public class SurveyResultsService : ISurveyResultsService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the SurveyResultsService with the specified database context and configuration.
        /// </summary>
        /// <param name="context">The database context for survey operations</param>
        /// <param name="configuration">The configuration containing base URL settings</param>
        public SurveyResultsService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Gets aggregated results for a survey including response counts and statistics.
        /// Calculates average ratings for rating questions and response counts for other question types.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <returns>Aggregated survey results with question statistics</returns>
        /// <exception cref="ArgumentException">Thrown when survey not found</exception>
        public async Task<SurveyResults> GetSurveyResults(int surveyId)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == surveyId);

            if (survey == null)
                throw new ArgumentException("Survey not found");

            var responses = await _context.SurveyResponses
                .Where(r => r.SurveyId == surveyId)
                .ToListAsync();

            var results = new SurveyResults
            {
                SurveyId = survey.Id,
                SurveyTitle = survey.Title,
                TotalResponses = responses.Count,
                QuestionResults = new List<QuestionResult>()
            };

            foreach (var question in survey.Questions)
            {
                var questionResult = new QuestionResult
                {
                    QuestionId = question.Id,
                    QuestionText = question.QuestionText,
                    QuestionType = question.type,
                    ResponseCounts = new Dictionary<string, int>()
                };

                if (question.type == "rating" && question.maxRating.HasValue)
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
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == surveyId);

            if (survey == null)
                throw new ArgumentException("Survey not found");

            var responses = await _context.SurveyResponses
                .Where(r => r.SurveyId == surveyId)
                .ToListAsync();

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
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == surveyId);

            if (survey == null)
                throw new ArgumentException("Survey not found");

            var responses = await _context.SurveyResponses
                .Where(r => r.SurveyId == surveyId)
                .ToListAsync();

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
            var survey = await _context.Surveys.FindAsync(surveyId);
            if (survey == null)
                throw new ArgumentException("Survey not found");

            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:4200";
            var shareLink = $"{baseUrl}/survey/{surveyId}";

            survey.ShareLink = shareLink;
            await _context.SaveChangesAsync();

            return shareLink;
        }
    }
} 