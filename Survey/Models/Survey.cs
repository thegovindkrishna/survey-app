using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Survey.Models
{
    public class SurveyModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public List<QuestionModel> Questions { get; set; } = new List<QuestionModel>();
        public string? ShareLink { get; set; }
    }

    public class QuestionModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Required { get; set; }
        public List<string>? Options { get; set; }
        public int? MaxRating { get; set; }
        public int SurveyId { get; set; }
        [JsonIgnore]
        public SurveyModel? Survey { get; set; }
    }

    public class SurveyResponseModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public string RespondentEmail { get; set; } = string.Empty;
        public DateTime SubmissionDate { get; set; }
        public List<QuestionResponseModel> responses { get; set; } = new List<QuestionResponseModel>();
    }

    public class QuestionResponseModel
    {
        public int QuestionId { get; set; }
        public string response { get; set; } = string.Empty;
    }

    public class SurveyResultsModel
    {
        public int SurveyId { get; set; }
        public string SurveyTitle { get; set; } = string.Empty;
        public int TotalResponses { get; set; }
        public List<QuestionResultModel> QuestionResults { get; set; } = new List<QuestionResultModel>();
    }

    public class QuestionResultModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public Dictionary<string, int> ResponseCounts { get; set; } = new Dictionary<string, int>();
        public double? AverageRating { get; set; }
    }
}
