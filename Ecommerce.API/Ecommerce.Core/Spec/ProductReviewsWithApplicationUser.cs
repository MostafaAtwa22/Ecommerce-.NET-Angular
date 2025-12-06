using Ecommerce.Core.Entities;
using Ecommerce.Core.Params;

namespace Ecommerce.Core.Spec
{
    public class ProductReviewsWithApplicationUser 
        : BaseSepcifications<ProductReview>
    {
        public ProductReviewsWithApplicationUser(
            int productId,
            ProductReviewsSpecParams specParams,
            bool forCount = false)
            : base(r => r.ProductId == productId)
        {
            AddIncludes(r => r.ApplicationUser);

            if (!string.IsNullOrEmpty(specParams.Sort))
            {
                switch (specParams.Sort.ToLower())
                {
                    case "dateasc":      
                        AddOrderBy(r => r.CreatedAt);
                        break;

                    case "datedesc":  
                        AddOrderByDesc(r => r.CreatedAt);
                        break;

                    case "ratingasc":    
                        AddOrderBy(r => r.Rating);
                        break;

                    case "ratingdesc":   
                        AddOrderByDesc(r => r.Rating);
                        break;

                    default:
                        AddOrderByDesc(r => r.CreatedAt);
                        break;
                }
            }
            else
            {
                AddOrderByDesc(r => r.CreatedAt);
            }

            if (!forCount)
            {
                ApplyPaging(
                    (specParams.PageIndex - 1) * specParams.PageSize,
                    specParams.PageSize
                );
            }
        }

        public ProductReviewsWithApplicationUser(int productId)
            : base(r => r.ProductId == productId)
        {
            AddIncludes(r => r.ApplicationUser);
        }
    }
}
