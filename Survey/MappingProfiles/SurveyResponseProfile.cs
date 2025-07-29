using AutoMapper;
using Survey.Models;
using Survey.Models.Dtos;
using Survey.Services;

namespace Survey.MappingProfiles
{
    public class SurveyResponseProfile : Profile
    {
        public SurveyResponseProfile()
        {
            CreateMap<SubmitQuestionResponseDto, QuestionResponseModel>();
            CreateMap<SubmitResponseDto, SurveyResponseModel>()
                .ForMember(dest => dest.responses, opt => opt.MapFrom(src => src.Responses));

            CreateMap<QuestionResponseModel, QuestionResponseDetailDto>();
            CreateMap<SurveyResponseModel, SurveyResponseDto>()
                .ForMember(dest => dest.Responses, opt => opt.MapFrom(src => src.responses));

            CreateMap<SurveyResponseModel, UserResponseDto>()
                .ForMember(dest => dest.SurveyId, opt => opt.MapFrom(src => src.SurveyId))
                .ForMember(dest => dest.SubmissionDate, opt => opt.MapFrom(src => src.SubmissionDate))
                .ForMember(dest => dest.ResponseId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Responses, opt => opt.MapFrom(src => src.responses));
            CreateMap<QuestionResponseModel, QuestionResponseDto>();
        }
    }
}