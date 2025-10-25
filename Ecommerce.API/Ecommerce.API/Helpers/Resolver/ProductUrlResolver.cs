using AutoMapper;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.Core.Entities;

namespace Ecommerce.API.Helpers.Resolver
{
    public class ProductUrlResolver : IValueResolver<Product, ProductResponseDto, string>
    {
        private readonly IConfiguration _config;

        public ProductUrlResolver(IConfiguration config)
        {
            _config = config;
        }
        
        public string Resolve(Product source, ProductResponseDto destination,
            string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.PictureUrl))
                return $"{_config["ApiUrl"]}/{source.PictureUrl}";
            return null!;
        }
    }
}