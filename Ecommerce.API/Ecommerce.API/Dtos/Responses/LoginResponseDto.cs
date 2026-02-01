using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.API.Dtos.Responses
{
    public class LoginResponseDto
    {
        public bool RequiresTwoFactor { get; set; }
        public string? Message { get; set; }
        public UserDto? User { get; set; }
    }
}