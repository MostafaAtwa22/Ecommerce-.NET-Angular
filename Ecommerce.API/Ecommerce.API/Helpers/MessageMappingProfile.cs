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
            CreateMap<OnlineUserDto, ApplicationUser>();
        }
    }
}