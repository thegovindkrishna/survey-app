using AutoMapper;
using Survey.Models;
using Survey.Models.Dtos;
using SurveyModel = Survey.Models.SurveyModel;

namespace Survey.MappingProfiles
{
    public class SurveyProfile : Profile
    {
        public SurveyProfile()
        {
            CreateMap<SurveyModel, SurveyDto>()
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));
            CreateMap<QuestionModel, QuestionDto>();

            CreateMap<SurveyCreateDto, SurveyModel>()
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));
            CreateMap<QuestionCreateDto, QuestionModel>();

            CreateMap<SurveyUpdateDto, SurveyModel>()
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));
            CreateMap<QuestionUpdateDto, QuestionModel>();
        }
    }   
}
