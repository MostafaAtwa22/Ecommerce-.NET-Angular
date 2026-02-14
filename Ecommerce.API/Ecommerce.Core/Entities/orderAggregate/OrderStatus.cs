using System.Runtime.Serialization;

namespace Ecommerce.Core.Entities.orderAggregate
{
    public enum OrderStatus
    {
        [EnumMember(Value = "Pending")]
        Pending,

        [EnumMember(Value = "Payment Received")]
        PaymentReceived,

        [EnumMember(Value = "Payment Failed")]
        PaymentFailed,

        [EnumMember(Value = "Shipped")]
        Shipped,

        [EnumMember(Value = "Complete")]
        Complete,

        [EnumMember(Value = "Return Requested")]
        ReturnRequested,

        [EnumMember(Value = "Refunded")]
        Refunded,

        [EnumMember(Value = "Cancel")]
        Cancel
    }
}