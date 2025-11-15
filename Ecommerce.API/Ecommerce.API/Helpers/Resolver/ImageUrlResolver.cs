using AutoMapper;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Ecommerce.API.Helpers.Resolver
{
    public class ImageUrlResolver<TSource, TDestination> 
        : IValueResolver<TSource, TDestination, string>
    {
        private readonly IConfiguration _config;
        private readonly string _propertyName;

        public ImageUrlResolver(IConfiguration config, string propertyName)
        {
            _config = config;
            _propertyName = propertyName;
        }

        public string Resolve(TSource source, TDestination destination,
            string destMember, ResolutionContext context)
        {
            PropertyInfo? prop = typeof(TSource).GetProperty(_propertyName);
            if (prop == null)
                return null!;

            var imagePath = prop.GetValue(source) as string;

            if (!string.IsNullOrEmpty(imagePath))
                return $"{_config["ApiUrl"]}/{imagePath}";

            return null!;
        }
    }
}
