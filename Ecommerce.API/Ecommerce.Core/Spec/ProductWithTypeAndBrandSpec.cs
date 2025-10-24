using System.Linq.Expressions;
using Ecommerce.Core.Entities;

namespace Ecommerce.Core.Spec
{
    public class ProductWithTypeAndBrandSpec : BaseSepcifications<Product>
    {
        public ProductWithTypeAndBrandSpec(string? sort, int? brandId, int? typeId)
            : base (p =>
                (!brandId.HasValue || p.ProductBrandId == brandId) &&
                (!typeId.HasValue || p.ProductTypeId == typeId))
        {
            AddIncludes(p => p.ProductType);
            AddIncludes(p => p.ProductBrand);
            AddOrderBy(p => p.Name);
        }

        public ProductWithTypeAndBrandSpec(int id) 
            : base(x => x.Id == id)
        {
            AddIncludes(p => p.ProductType);
            AddIncludes(p => p.ProductBrand);
        }
    }
}