import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { IOrder } from '../../shared/modules/order';
import { CheckoutService } from '../../checkout/checkout-service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-order-details-component',
  imports: [CommonModule, RouterLink],
  templateUrl: './order-details-component.html',
  styleUrls: ['./order-details-component.scss'],
})
export class OrderDetailsComponent implements OnInit {
  order!: IOrder;
  orderId!: number;

  // Order progress steps
  orderSteps = [
    { label: 'Ordered', status: 'ordered', title: 'Order Placed', description: 'Your order has been successfully placed and confirmed.', date: '' },
    { label: 'Confirmed', status: 'confirmed', title: 'Order Confirmed', description: 'We\'re preparing your items for shipment.', date: '' },
    { label: 'Shipped', status: 'shipped', title: 'Order Shipped', description: 'Your order is on the way to you.', date: '' },
    { label: 'Delivered', status: 'delivered', title: 'Order Delivered', description: 'Your order has been successfully delivered.', date: '' }
  ];

  currentStepIndex = 0;
  estimatedDeliveryDate: string = 'N/A';

  constructor(
    private route: ActivatedRoute,
    private checkoutService: CheckoutService
  ) {}

  ngOnInit(): void {
    this.orderId = +this.route.snapshot.paramMap.get('id')!;
    this.loadOrderDetails();
  }

  loadOrderDetails(): void {
    this.checkoutService.getUserOrderById(this.orderId).subscribe({
      next: (order) => {
        console.log('Order loaded:', order); // <- Add this
        this.order = order;
        this.updateStepProgress();
        this.calculateEstimatedDeliveryDate();
      },
      error: (err) => {
        console.error('Failed to load order details', err);
      }
    });
}

  updateStepProgress(): void {
    const statusOrder = ['ordered', 'confirmed', 'shipped', 'delivered'];
    const foundIndex = statusOrder.indexOf(this.order.status?.toLowerCase() || 'ordered');
    this.currentStepIndex = foundIndex >= 0 ? foundIndex : 0;

    if (!this.order?.orderDate) {
      return;
    }

    const orderDate = new Date(this.order.orderDate);
    if (isNaN(orderDate.getTime())) {
      return;
    }

    this.orderSteps[0].date = orderDate.toLocaleDateString();

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

  getStatusClass(status: string): string {
    const statusMap: { [key: string]: string } = {
      'pending': 'pending',
      'processing': 'processing',
      'shipped': 'shipped',
      'delivered': 'delivered',
      'completed': 'completed',
      'cancelled': 'cancelled'
    };
    return statusMap[status.toLowerCase()] || 'pending';
  }

  getStepDisplay(stepIndex: number): string {
    if (stepIndex <= this.currentStepIndex) {
      return '✓';
    }
    return (stepIndex + 1).toString();
  }

  getStepState(stepIndex: number): string {
    if (stepIndex < this.currentStepIndex) {
      return 'completed';
    } else if (stepIndex === this.currentStepIndex) {
      return 'active';
    } else {
      return 'pending';
    }
  }

  getCurrentStep() {
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

    // Calculate delivery days once and store the result
    const deliveryDays = 3 + Math.floor(Math.random() * 5);
    orderDate.setDate(orderDate.getDate() + deliveryDays);
    this.estimatedDeliveryDate = orderDate.toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' });
  }
}
