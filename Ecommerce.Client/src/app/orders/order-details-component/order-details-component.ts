import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { IOrder } from '../../shared/modules/order';
import { CheckoutService } from '../../checkout/checkout-service';
import { CommonModule } from '@angular/common';
import { getOrderStatusLabel, OrderStatus } from '../../shared/modules/order-status';
import { SweetAlertService } from '../../shared/services/sweet-alert.service';

@Component({
  selector: 'app-order-details-component',
  imports: [CommonModule, RouterLink],
  templateUrl: './order-details-component.html',
  styleUrls: ['./order-details-component.scss'],
})
export class OrderDetailsComponent implements OnInit {
  order!: IOrder;
  orderId!: number;

  // OrderStatus enum for template access
  OrderStatus = OrderStatus;

  // Order progress steps - now mapped to OrderStatus enum
  orderSteps = [
    {
      label: 'Pending',
      status: OrderStatus.Pending,
      title: 'Order Pending',
      description: 'Your order has been placed and is awaiting payment confirmation.',
      date: ''
    },
    {
      label: 'Payment Received',
      status: OrderStatus.PaymentReceived,
      title: 'Payment Confirmed',
      description: 'Payment has been received. We\'re preparing your items for shipment.',
      date: ''
    },
    {
      label: 'Shipped',
      status: OrderStatus.Shipped,
      title: 'Order Shipped',
      description: 'Your order is on the way to you.',
      date: ''
    },
    {
      label: 'Complete',
      status: OrderStatus.Complete,
      title: 'Order Complete',
      description: 'Your order has been successfully delivered.',
      date: ''
    }
  ];

  currentStepIndex = 0;
  estimatedDeliveryDate: string = 'N/A';
  isCancelled: boolean = false;
  isComplete: boolean = false;
  isReturnRequested: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private checkoutService: CheckoutService,
    private sweetAlert: SweetAlertService
  ) {}

  ngOnInit(): void {
    this.orderId = +this.route.snapshot.paramMap.get('id')!;
    this.loadOrderDetails();
  }

  loadOrderDetails(): void {
    this.checkoutService.getUserOrderById(this.orderId).subscribe({
      next: (order) => {
        this.order = order;
        this.updateStepProgress();
        this.calculateEstimatedDeliveryDate();
        this.checkIfCancelled();
        this.checkIfComplete();
        this.checkIfReturnRequested();
      },
      error: (err) => {
        console.error('Failed to load order details', err);
      }
    });
  }

  checkIfCancelled(): void {
    if (!this.order?.status) {
      this.isCancelled = false;
      return;
    }
    const statusNum = this.getOrderStatusNumber(this.order.status);

    this.isCancelled = statusNum === OrderStatus.Cancel;
  }

  checkIfReturnRequested(): void {
    if (!this.order?.status) {
      this.isReturnRequested = false;
      return;
    }

    const statusNum = this.getOrderStatusNumber(this.order.status);
    this.isReturnRequested = statusNum === OrderStatus.ReturnRequested;
  }

  checkIfComplete(): void {
    if (!this.order?.status) {
      this.isComplete = false;
      return;
    }
    const statusNum = this.getOrderStatusNumber(this.order.status);
    this.isComplete = statusNum === OrderStatus.Complete;
  }

  updateStepProgress(): void {
    if (!this.order || !this.order.status) {
      this.currentStepIndex = 0;
      return;
    }

    // Convert status to number
    const statusNum = this.getOrderStatusNumber(this.order.status);

    // Check if order is cancelled - show different flow
    if (statusNum === OrderStatus.Cancel) {
      this.currentStepIndex = -1; // Special case for cancelled
      return;
    }

    // Check if payment failed - show different flow
    if (statusNum === OrderStatus.PaymentFailed) {
      this.currentStepIndex = 0; // Reset to pending but show payment failed
      return;
    }

    // Return/refund states should display the last normal step (complete)
    if (statusNum === OrderStatus.ReturnRequested || statusNum === OrderStatus.Refunded) {
      this.currentStepIndex = 3;
    }

    // Find the matching step based on status
    const stepIndex = this.orderSteps.findIndex(step => step.status === statusNum);

    // If exact status found, use that step
    if (stepIndex !== -1) {
      this.currentStepIndex = stepIndex;
    } else {
      // Otherwise, determine step based on status value
      if (statusNum === OrderStatus.Complete) {
        this.currentStepIndex = 3; // Complete
      } else if (statusNum === OrderStatus.Shipped) {
        this.currentStepIndex = 2; // Shipped
      } else if (statusNum === OrderStatus.PaymentReceived) {
        this.currentStepIndex = 1; // Payment Received
      } else {
        this.currentStepIndex = 0; // Pending
      }
    }

    // Set dates for completed steps
    if (!this.order?.orderDate) {
      return;
    }

    const orderDate = new Date(this.order.orderDate);
    if (isNaN(orderDate.getTime())) {
      return;
    }

    // Set date for current step
    if (this.currentStepIndex >= 0) {
      this.orderSteps[0].date = orderDate.toLocaleDateString();
    }
    if (this.currentStepIndex >= 1) {
      this.orderSteps[1].date = new Date(orderDate.getTime() + 24 * 60 * 60 * 1000).toLocaleDateString();
    }
    if (this.currentStepIndex >= 2) {
      this.orderSteps[2].date = new Date(orderDate.getTime() + 48 * 60 * 60 * 1000).toLocaleDateString();
    }
    if (this.currentStepIndex >= 3) {
      this.orderSteps[3].date = new Date(orderDate.getTime() + 72 * 60 * 60 * 1000).toLocaleDateString();
    }
  }

  // Helper to convert status to number - change to public
  getOrderStatusNumber(status: string | number): number {
    if (typeof status === 'number') {
      return status;
    }

    // If it's already a number string, parse it
    if (!isNaN(Number(status))) {
      return parseInt(status, 10);
    }

    // If it's a string label, map it to enum value
    switch (status.toLowerCase()) {
      case 'pending':
        return OrderStatus.Pending;
      case 'paymentreceived':
      case 'payment received':
        return OrderStatus.PaymentReceived;
      case 'paymentfailed':
      case 'payment failed':
        return OrderStatus.PaymentFailed;
      case 'shipped':
        return OrderStatus.Shipped;
      case 'complete':
        return OrderStatus.Complete;
      case 'returnrequested':
      case 'return requested':
        return OrderStatus.ReturnRequested;
      case 'refunded':
        return OrderStatus.Refunded;
      case 'cancel':
      case 'cancelled':
        return OrderStatus.Cancel;
      default:
        return OrderStatus.Pending;
    }
  }

  canCancel(): boolean {
    if (!this.order?.status) return false;
    const statusNum = this.getOrderStatusNumber(this.order.status);
    return statusNum === OrderStatus.Pending || statusNum === OrderStatus.PaymentReceived;
  }

  canReturn(): boolean {
    if (!this.order?.status) return false;
    const statusNum = this.getOrderStatusNumber(this.order.status);
    return statusNum === OrderStatus.Complete;
  }

  cancelOrder(): void {
    if (!this.order) return;

    this.sweetAlert
      .confirm({
        title: 'Cancel order?',
        text: 'You can cancel only before it is shipped. If payment was received, we will refund you.',
        confirmButtonText: 'Yes, cancel it'
      })
      .then(result => {
        if (!result.isConfirmed) return;

        this.checkoutService.cancelOrder(this.order.id).subscribe({
          next: (updated) => {
            this.order = updated;
            this.checkIfCancelled();
            this.checkIfComplete();
            this.checkIfReturnRequested();
            this.updateStepProgress();
            this.calculateEstimatedDeliveryDate();
            this.sweetAlert.success('Order cancelled successfully');
          },
          error: (err) => {
            this.sweetAlert.error(err.error?.message || 'Failed to cancel order');
          }
        });
      });
  }

  requestReturn(): void {
    if (!this.order) return;

    this.sweetAlert
      .confirm({
        title: 'Request return?',
        text: 'Return requests must be approved. After approval, your refund will be processed.',
        confirmButtonText: 'Yes, request return'
      })
      .then(result => {
        if (!result.isConfirmed) return;

        this.checkoutService.requestReturn(this.order.id).subscribe({
          next: (updated) => {
            this.order = updated;
            this.checkIfCancelled();
            this.checkIfComplete();
            this.checkIfReturnRequested();
            this.updateStepProgress();
            this.calculateEstimatedDeliveryDate();
            this.sweetAlert.success('Return request submitted');
          },
          error: (err) => {
            this.sweetAlert.error(err.error?.message || 'Failed to request return');
          }
        });
      });
  }

  // Helper to check if status is PaymentFailed
  isPaymentFailed(): boolean {
    if (!this.order?.status) return false;
    return this.getOrderStatusNumber(this.order.status) === OrderStatus.PaymentFailed;
  }

  // Helper to get status label for display
  getOrderStatusLabel(status: string | number): string {
    return getOrderStatusLabel(status);
  }

  // Get CSS class for status badge
  getStatusClass(status: string | number): string {
    const statusNum = this.getOrderStatusNumber(status);

    switch (statusNum) {
      case OrderStatus.Pending:
        return 'pending';
      case OrderStatus.PaymentReceived:
        return 'processing';
      case OrderStatus.PaymentFailed:
        return 'cancelled';
      case OrderStatus.Shipped:
        return 'shipped';
      case OrderStatus.Complete:
        return 'completed';
      case OrderStatus.ReturnRequested:
        return 'processing';
      case OrderStatus.Refunded:
        return 'completed';
      case OrderStatus.Cancel:
        return 'cancelled';
      default:
        return 'pending';
    }
  }

  getStepDisplay(stepIndex: number): string {
    if (stepIndex < 0) return '✗'; // Cancelled icon

    if (stepIndex <= this.currentStepIndex) {
      return '✓';
    }
    return (stepIndex + 1).toString();
  }

  getStepState(stepIndex: number): string {
    if (this.isCancelled) {
      return 'cancelled';
    }

    if (this.isPaymentFailed()) {
      return 'error';
    }

    if (stepIndex < this.currentStepIndex) {
      return 'completed';
    } else if (stepIndex === this.currentStepIndex) {
      return 'active';
    } else {
      return 'pending';
    }
  }

  getCurrentStep() {
    // Handle cancelled order
    if (this.isCancelled) {
      return {
        title: 'Order Cancelled',
        description: 'This order has been cancelled. If you have any questions, please contact customer support.',
        date: this.order?.orderDate ? new Date(this.order.orderDate).toLocaleDateString() : ''
      };
    }

    // Handle payment failed
    if (this.isPaymentFailed()) {
      return {
        title: 'Payment Failed',
        description: 'There was an issue processing your payment. Please update your payment method or contact support.',
        date: this.order?.orderDate ? new Date(this.order.orderDate).toLocaleDateString() : ''
      };
    }

    const index = this.currentStepIndex >= 0 && this.currentStepIndex < this.orderSteps.length
      ? this.currentStepIndex
      : 0;
    return this.orderSteps[index] || this.orderSteps[0];
  }

  trackByItemId(index: number, item: any) {
    return item.productItemId;
  }

  calculateEstimatedDeliveryDate(): void {
    if (!this.order?.orderDate) {
      this.estimatedDeliveryDate = 'N/A';
      return;
    }

    const orderDate = new Date(this.order.orderDate);
    if (isNaN(orderDate.getTime())) {
      this.estimatedDeliveryDate = 'N/A';
      return;
    }

    // Convert status to number before comparison
    const statusNum = this.getOrderStatusNumber(this.order.status);

    // Handle different statuses
    if (statusNum === OrderStatus.Cancel) {
      this.estimatedDeliveryDate = 'Order Cancelled';
    } else if (statusNum === OrderStatus.ReturnRequested) {
      this.estimatedDeliveryDate = 'Return Requested';
    } else if (statusNum === OrderStatus.Refunded) {
      this.estimatedDeliveryDate = 'Refunded';
    } else if (statusNum === OrderStatus.PaymentFailed) {
      this.estimatedDeliveryDate = 'Payment Required';
    } else if (statusNum === OrderStatus.Shipped || statusNum === OrderStatus.Complete) {
      const deliveryDays = 3 + Math.floor(Math.random() * 5);
      orderDate.setDate(orderDate.getDate() + deliveryDays);
      this.estimatedDeliveryDate = orderDate.toLocaleDateString('en-US', {
        month: 'short',
        day: '2-digit',
        year: 'numeric'
      });
    } else if (statusNum === OrderStatus.PaymentReceived) {
      this.estimatedDeliveryDate = 'Processing - Ships in 1-2 business days';
    } else {
      this.estimatedDeliveryDate = 'Will be updated after payment';
    }
  }
}
