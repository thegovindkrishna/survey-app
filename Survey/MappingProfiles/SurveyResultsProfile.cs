using AutoMapper;
using Survey.Models;
using Survey.Models.Dtos;

namespace Survey.MappingProfiles
{
    public class SurveyResultsProfile : Profile
    {
        public SurveyResultsProfile()
        {
            CreateMap<QuestionResultModel, QuestionResultDto>();
            CreateMap<SurveyResultsModel, SurveyResultsDto>();
            CreateMap<QuestionResultModel, QuestionResultDto>();
            CreateMap<QuestionResultModel, QuestionResultDto>();
        }
    }
}
