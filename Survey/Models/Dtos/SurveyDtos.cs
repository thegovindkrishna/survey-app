using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Survey.Models.Dtos
{
    // --- Display DTOs --- //

    public record QuestionDto(
        int Id,
        string QuestionText,
        string Type,
        bool Required,
        List<string>? Options,
        int? MaxRating
    );

    public record SurveyDto(
        int Id,
        string Title,
        string Description,
        DateTime StartDate,
        DateTime EndDate,
        string CreatedBy,
        string? ShareLink,
        List<QuestionDto> Questions
    );

    // --- Create DTOs --- //

    public record QuestionCreateDto(
        [Required]
        [StringLength(500, MinimumLength = 3)]
        string QuestionText,

        [Required]
        string Type,

        bool Required,

        List<string>? Options,

        [Range(1, 10)]
        int? MaxRating
    );

    public record SurveyCreateDto(
        [Required]
        [StringLength(100, MinimumLength = 5)]
        string Title,

        [StringLength(1000)]
        string Description,

        [Required]
        DateTime StartDate,

        [Required]
        DateTime EndDate,

        [Required]
        [MinLength(1)]
        List<QuestionCreateDto> Questions
    );

    // --- Update DTOs --- //

    public record QuestionUpdateDto(
        [Required]
        [StringLength(500, MinimumLength = 3)]
        string QuestionText,

        [Required]
        string Type,

        bool Required,

        List<string>? Options,

        [Range(1, 10)]
        int? MaxRating
    );

    public record SurveyUpdateDto(
        [Required]
        [StringLength(100, MinimumLength = 5)]
        string Title,

        [StringLength(1000)]
        string Description,

        [Required]
        DateTime StartDate,

        [Required]
        DateTime EndDate,

        [Required]
        [MinLength(1)]
        List<QuestionUpdateDto> Questions
    );

    // --- User Response DTOs --- //

    public class QuestionResponseDto
    {
        public int QuestionId { get; set; }
        public string Response { get; set; }
    }

    public class UserResponseDto
    {
        public int SurveyId { get; set; }
        public string SurveyTitle { get; set; }
        public string SurveyDescription { get; set; }
        public DateTime SubmissionDate { get; set; }
        public int ResponseId { get; set; }
        public List<QuestionResponseDto> Responses { get; set; }
    }

    // --- Response DTOs for Admin/Detailed View --- //

    public class QuestionResponseDetailDto
    {
        public int QuestionId { get; set; }
        public string Response { get; set; }
        public string QuestionText { get; set; }
    }

    public class SurveyResponseDto
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public string RespondentEmail { get; set; }
        public DateTime SubmissionDate { get; set; }
        public List<QuestionResponseDetailDto> Responses { get; set; }
    }

    // --- DTOs for Submitting Responses --- //

    public record SubmitQuestionResponseDto(
        [Required]
        int QuestionId,
        [Required]
        string Response
    );

    public record SubmitResponseDto(
        [Required]
        [MinLength(1)]
        List<SubmitQuestionResponseDto> Responses
    );

    // --- User DTOs --- //

    public record UserDto(
        string Email,
        string Role
    );

    // --- Survey Results DTOs --- //

    public class QuestionResultDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public Dictionary<string, int> ResponseCounts { get; set; }
        public double? AverageRating { get; set; }
    }

    public class SurveyResultsDto
    {
        public int SurveyId { get; set; }
        public string SurveyTitle { get; set; }
        public int TotalResponses { get; set; }
        public List<QuestionResultDto> QuestionResults { get; set; }
    }
}
