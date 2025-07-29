using AutoMapper;
using Survey.Models;
using Survey.Models.Dtos;

namespace Survey.MappingProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserModel, UserDto>();
        }
    }
}
