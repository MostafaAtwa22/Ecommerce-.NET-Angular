// order-status.utils.ts
export enum OrderStatus {
  Pending = 0,
  PaymentReceived = 1,
  PaymentFailed = 2,
  Shipped = 3,
  Complete = 4,
  ReturnRequested = 5,
  Refunded = 6,
  Cancel = 7  
}

export function getOrderStatusLabel(status: string | number): string {
  if (typeof status === 'string' && isNaN(Number(status))) {
    return status;
  }

  // Convert to number
  const statusNum = typeof status === 'number' ? status : parseInt(status, 10);

  // Map number to label
  switch (statusNum) {
    case OrderStatus.Pending:
      return 'Pending';
    case OrderStatus.PaymentReceived:
      return 'Payment Received';
    case OrderStatus.PaymentFailed:
      return 'Payment Failed';
    case OrderStatus.Shipped:
      return 'Shipped';
    case OrderStatus.Complete:
      return 'Complete';
    case OrderStatus.ReturnRequested:
      return 'Return Requested';
    case OrderStatus.Refunded:
      return 'Refunded';
    case OrderStatus.Cancel:
      return 'Canceled';  // Display as "Canceled" even though enum is "Cancel"
    default:
      return 'Unknown';
  }
}
