using Ecommerce.Core.Entities;
using Ecommerce.Core.Params;

namespace Ecommerce.Core.Spec
{
    public class ProductWithTypeAndBrandSpec : BaseSepcifications<Product>
    {
        public ProductWithTypeAndBrandSpec(ProductSpecParams specParams, bool forCount = false)
            : base(p =>
                (string.IsNullOrEmpty(specParams.Search) || p.Name.ToLower().Contains(specParams.Search.ToLower())) &&
                (!specParams.BrandId.HasValue || p.ProductBrandId == specParams.BrandId) &&
                (!specParams.TypeId.HasValue || p.ProductTypeId == specParams.TypeId) &&
                (!specParams.MinAverageRating.HasValue || p.AverageRating >= specParams.MinAverageRating) 
            )
        {
            if (!forCount)
            {
                AddIncludes(p => p.ProductType);
                AddIncludes(p => p.ProductBrand);

                switch (specParams.Sort)
                {
                    case "PriceAsc":
                        AddOrderBy(p => p.Price);
                        break;
                    case "PriceDesc":
                        AddOrderByDesc(p => p.Price);
                        break;
                    case "RatingAsc":
                        AddOrderBy(p => p.AverageRating); 
                        break;
                    case "RatingDesc":
                        AddOrderByDesc(p => p.AverageRating);
                        break;
                    default:
                        AddOrderBy(p => p.Name);
                        break;
                }

                ApplyPaging((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
            }
        }

        public ProductWithTypeAndBrandSpec(int id)
            : base(x => x.Id == id)
        {
            AddIncludes(p => p.ProductType);
            AddIncludes(p => p.ProductBrand);
        }
    }
}
