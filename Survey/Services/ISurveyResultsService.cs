using Survey.Models;

namespace Survey.Services
{
    /// <summary>
    /// Service interface for generating survey results and reports.
    /// Provides functionality for aggregating survey data, exporting responses, and generating shareable links.
    /// </summary>
    public interface ISurveyResultsService
    {
        /// <summary>
        /// Gets aggregated results for a survey including response counts and statistics.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <returns>Aggregated survey results with question statistics</returns>
        /// <exception cref="ArgumentException">Thrown when survey not found</exception>
        Task<SurveyResultsModel> GetSurveyResults(int surveyId);

        /// <summary>
        /// Exports survey responses to CSV format for data analysis.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <returns>CSV file content as byte array</returns>
        /// <exception cref="ArgumentException">Thrown when survey not found</exception>
        Task<byte[]> ExportToCsv(int surveyId);

        /// <summary>
        /// Exports survey responses to PDF format for reporting.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <returns>PDF file content as byte array</returns>
        /// <exception cref="ArgumentException">Thrown when survey not found</exception>
        Task<byte[]> ExportToPdf(int surveyId);

        /// <summary>
        /// Generates a shareable link for a survey.
        /// Updates the survey's ShareLink property in the database.
        /// </summary>
        /// <param name="surveyId">The unique identifier of the survey</param>
        /// <returns>The shareable survey link</returns>
        /// <exception cref="ArgumentException">Thrown when survey not found</exception>
        Task<string> GenerateShareLink(int surveyId);
    }
} 