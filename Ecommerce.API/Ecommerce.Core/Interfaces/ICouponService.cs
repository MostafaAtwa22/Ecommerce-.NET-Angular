using Ecommerce.Core.Entities;

namespace Ecommerce.Core.Interfaces
{
    public interface ICouponService
    {
        Task<Coupon?> GetValidCouponAsync(string code);
    }
}
