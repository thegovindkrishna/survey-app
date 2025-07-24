using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Survey.Models
{
    public class Survey
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public List<Question> Questions { get; set; } = new List<Question>();
        public string? ShareLink { get; set; }
    }

    public class Question
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
        public Survey? Survey { get; set; }
    }

    public class SurveyResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public string RespondentEmail { get; set; } = string.Empty;
        public DateTime SubmissionDate { get; set; }
        public List<QuestionResponse> responses { get; set; } = new List<QuestionResponse>();
    }

    public class QuestionResponse
    {
        public int QuestionId { get; set; }
        public string response { get; set; } = string.Empty;
    }

    public class SurveyResults
    {
        public int SurveyId { get; set; }
        public string SurveyTitle { get; set; } = string.Empty;
        public int TotalResponses { get; set; }
        public List<QuestionResult> QuestionResults { get; set; } = new List<QuestionResult>();
    }

    public class QuestionResult
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public Dictionary<string, int> ResponseCounts { get; set; } = new Dictionary<string, int>();
        public double? AverageRating { get; set; }
    }
}
