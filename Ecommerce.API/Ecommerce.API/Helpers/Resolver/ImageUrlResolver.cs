using AutoMapper;

namespace Ecommerce.API.Helpers.Resolver
{
    public class ImageUrlResolver<TSource, TDestination>
        : IValueResolver<TSource, TDestination, string?>
    {
        private readonly IConfiguration _config;
        private readonly string _propertyName;

        public ImageUrlResolver(IConfiguration config, string propertyName)
        {
            _config = config;
            _propertyName = propertyName;
        }

        public string? Resolve(TSource source, TDestination destination,
            string? destMember, ResolutionContext context)
        {
            if (source == null)
                return null!;

            string[] parts = _propertyName.Split('.');

            object? current = source;

            foreach (var part in parts)
            {
                if (current == null) return null!;

                var prop = current.GetType().GetProperty(part);
                if (prop == null) return null!;

                current = prop.GetValue(current);
            }

            var imagePath = current as string;

            if (!string.IsNullOrEmpty(imagePath))
                return $"{_config["ApiUrl"]}/{imagePath}";

            return null!;
        }
    }
}
