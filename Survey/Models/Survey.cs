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
    }

    public class Question
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public bool required { get; set; }
        public List<string>? options { get; set; }
        public int? maxRating { get; set; }
    }
}
