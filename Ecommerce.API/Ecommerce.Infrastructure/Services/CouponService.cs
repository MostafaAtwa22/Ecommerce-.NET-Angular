using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Spec;

namespace Ecommerce.Infrastructure.Services
{
    public class CouponService : ICouponService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CouponService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Coupon?> GetValidCouponAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            var spec = new SpecificationBuilder<Coupon>(c =>
                c.Code == code &&
                c.IsActive &&
                c.ExpiryDate > DateTimeOffset.UtcNow)
                .WithTracking();

            return await _unitOfWork.Repository<Coupon>().GetWithSpecAsync(spec);
        }
    }
}
