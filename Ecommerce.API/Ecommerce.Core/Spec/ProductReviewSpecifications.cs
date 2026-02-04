using Ecommerce.Core.Entities;
using Ecommerce.Core.Params;

namespace Ecommerce.Core.Spec
{
    public static class ProductReviewSpecifications
    {
        public static ISpecifications<ProductReview> BuildListingSpec(
            int productId,
            ProductReviewsSpecParams specParams)
        {
            var builder = new SpecificationBuilder<ProductReview>(r => r.ProductId == productId)
                .Include(r => r.ApplicationUser);

            if (!string.IsNullOrEmpty(specParams.Sort))
            {
                switch (specParams.Sort.ToLower())
                {
                    case "dateasc":
                        builder.OrderByAsc(r => r.CreatedAt);
                        break;

                    case "datedesc":
                        builder.OrderByDesc(r => r.CreatedAt);
                        break;

                    case "ratingasc":
                        builder.OrderByAsc(r => r.Rating);
                        break;

                    case "ratingdesc":
                        builder.OrderByDesc(r => r.Rating);
                        break;

                    default:
                        builder.OrderByDesc(r => r.CreatedAt);
                        break;
                }
            }
            else
            {
                builder.OrderByDesc(r => r.CreatedAt);
            }

            builder.Paginate(specParams.PageIndex, specParams.PageSize);

            return builder;
        }

        public static ISpecifications<ProductReview> BuildListingCountSpec(
            int productId,
            ProductReviewsSpecParams specParams)
        {
            var builder = new SpecificationBuilder<ProductReview>(r => r.ProductId == productId);

            return builder;
        }

        public static ISpecifications<ProductReview> BuildDetailsSpec(int productId)
        {
            return new SpecificationBuilder<ProductReview>(r => r.ProductId == productId)
                .Include(r => r.ApplicationUser);
        }
    }
}

