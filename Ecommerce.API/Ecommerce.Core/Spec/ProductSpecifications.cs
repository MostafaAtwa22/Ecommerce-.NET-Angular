using Ecommerce.Core.Entities;
using Ecommerce.Core.Params;

namespace Ecommerce.Core.Spec
{
    public static class ProductSpecifications
    {
        public static ISpecifications<Product> BuildListingSpec(ProductSpecParams specParams)
        {
            var builder = new SpecificationBuilder<Product>();

            if (!string.IsNullOrEmpty(specParams.Search))
            {
                builder.Where(p => p.Name.ToLower().Contains(specParams.Search.ToLower()));
            }

            if (specParams.BrandId.HasValue)
            {
                builder.And(p => p.ProductBrandId == specParams.BrandId);
            }

            if (specParams.TypeId.HasValue)
            {
                builder.And(p => p.ProductTypeId == specParams.TypeId);
            }

            if (specParams.MinAverageRating.HasValue)
            {
                builder.And(p => p.AverageRating >= specParams.MinAverageRating);
            }

            builder
                .Include(p => p.ProductType)
                .Include(p => p.ProductBrand);

            switch (specParams.Sort)
            {
                case "PriceAsc":
                    builder.OrderByAsc(p => p.Price);
                    break;
                case "PriceDesc":
                    builder.OrderByDesc(p => p.Price);
                    break;
                case "RatingAsc":
                    builder.OrderByAsc(p => p.AverageRating);
                    break;
                case "RatingDesc":
                    builder.OrderByDesc(p => p.AverageRating);
                    break;
                default:
                    builder.OrderByAsc(p => p.Name);
                    break;
            }

            builder.Paginate(specParams.PageIndex, specParams.PageSize);

            return builder;
        }

        public static ISpecifications<Product> BuildListingCountSpec(ProductSpecParams specParams)
        {
            var builder = new SpecificationBuilder<Product>();

            if (!string.IsNullOrEmpty(specParams.Search))
            {
                builder.Where(p => p.Name.ToLower().Contains(specParams.Search.ToLower()));
            }

            if (specParams.BrandId.HasValue)
            {
                builder.And(p => p.ProductBrandId == specParams.BrandId);
            }

            if (specParams.TypeId.HasValue)
            {
                builder.And(p => p.ProductTypeId == specParams.TypeId);
            }

            if (specParams.MinAverageRating.HasValue)
            {
                builder.And(p => p.AverageRating >= specParams.MinAverageRating);
            }

            // Count spec does not need includes or paging

            return builder;
        }

        public static ISpecifications<Product> BuildDetailsSpec(int id)
        {
            return new SpecificationBuilder<Product>()
                .Where(p => p.Id == id)
                .Include(p => p.ProductType)
                .Include(p => p.ProductBrand);
        }
    }
}

