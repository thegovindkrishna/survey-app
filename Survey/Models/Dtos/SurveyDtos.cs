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

    public record QuestionResponseDto(
        int QuestionId,
        string Response
    );

    public record UserResponseDto(
        int SurveyId,
        string SurveyTitle,
        string SurveyDescription,
        DateTime SubmissionDate,
        int ResponseId,
        List<QuestionResponseDto> Responses
    );

    // --- Response DTOs for Admin/Detailed View --- //

    public record QuestionResponseDetailDto(
        int QuestionId,
        string Response,
        string QuestionText // Include question text for context
    );

    public record SurveyResponseDto(
        int Id,
        int SurveyId,
        string RespondentEmail,
        DateTime SubmissionDate,
        List<QuestionResponseDetailDto> Responses
    );

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

    public record QuestionResultDto(
        int QuestionId,
        string QuestionText,
        string QuestionType,
        Dictionary<string, int> ResponseCounts,
        double? AverageRating
    );

    public record SurveyResultsDto(
        int SurveyId,
        string SurveyTitle,
        int TotalResponses,
        List<QuestionResultDto> QuestionResults
    );
}