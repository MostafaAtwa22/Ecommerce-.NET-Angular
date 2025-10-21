using System.Linq.Expressions;
using Ecommerce.Core.Entities;

namespace Ecommerce.Core.Spec
{
    public class ProductWithTypeAndBrandSpec : BaseSepcifications<Product>
    {
        public ProductWithTypeAndBrandSpec()
        {
            AddIncludes(p => p.ProductType);
            AddIncludes(p => p.ProductBrand);
        }

        public ProductWithTypeAndBrandSpec(int id) 
            : base(x => x.Id == id)
        {
            AddIncludes(p => p.ProductType);
            AddIncludes(p => p.ProductBrand);
        }
    }
}