
namespace Ecommerce.Core.Spec
{
    public class OrderByPaymentIntentIdSpecification : BaseSepcifications<Order>
    {
        public OrderByPaymentIntentIdSpecification(string paymentIntentId)
            : base (o => o.PaymentIntenId == paymentIntentId)
        {
            
        }
    }
}
