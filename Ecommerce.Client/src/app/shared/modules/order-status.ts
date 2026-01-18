// order-status.utils.ts
export enum OrderStatus {
  Pending = 0,
  PaymentReceived = 1,
  PaymentFailed = 2,
  Shipped = 3,
  Complete = 4,
  Canceled = 5  // Changed from 'Cancelled' to 'Canceled'
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
    case OrderStatus.Canceled:
      return 'Canceled';  // Changed from 'Cancelled' to 'Canceled'
    default:
      return 'Unknown';
  }
}
