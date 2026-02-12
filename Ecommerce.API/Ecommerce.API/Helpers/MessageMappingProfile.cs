using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.Core.Entities.Chat;
using Ecommerce.Core.Entities.Identity;

namespace Ecommerce.API.Helpers
{
    public class MessageMappingProfile : Profile
    {
        public MessageMappingProfile()
        {
            CreateMap<MessageRequestDto, Message>();
            
            CreateMap<Message, MessageResponseDto>();
            
            CreateMap<ApplicationUser, OnlineUserDto>()
                .ForMember(dest => dest.ConnectionId, opt => opt.Ignore())
                .ForMember(dest => dest.IsOnline, opt => opt.Ignore())
                .ForMember(dest => dest.UnReadCount, opt => opt.Ignore())
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()));
        }
    }
}