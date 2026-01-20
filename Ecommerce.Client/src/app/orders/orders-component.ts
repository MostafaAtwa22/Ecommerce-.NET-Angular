import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CheckoutService } from '../checkout/checkout-service';
import { IOrder } from '../shared/modules/order';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { getOrderStatusLabel } from '../shared/modules/order-status';

@Component({
  selector: 'app-orders-component',
  imports: [RouterLink, CurrencyPipe, DatePipe],
  templateUrl: './orders-component.html',
  styleUrl: './orders-component.scss',
})
export class OrdersComponent implements OnInit {
  orders!: IOrder[];
  private orderService = inject(CheckoutService);
  private router = inject(Router);

  ngOnInit(): void {
    this.getAllUserOrders();
  }

  getAllUserOrders() {
    this.orderService.getAllUserOrders().subscribe({
      next: (orders) => {
        this.orders = orders;
      },
      error: (err) => {
        console.error('Failed to fetch orders', err);
      }
    });
  }

  getStatusLabel(status: string | number): string {
    return getOrderStatusLabel(status);
  }

  getStatusClass(status: string | number): string {
    const statusLabel = getOrderStatusLabel(status).toLowerCase();
    const statusMap: { [key: string]: string } = {
      'pending': 'pending',
      'processing': 'processing',
      'payment received': 'processing',
      'payment failed': 'cancelled',
      'shipped': 'shipped',
      'delivered': 'delivered',
      'complete': 'completed',
      'completed': 'completed',
      'cancelled': 'cancelled',
      'canceled': 'canceled'
    };
    return statusMap[statusLabel] || 'cancelled';
  }

  viewOrderDetails(orderId: number) {
    this.router.navigate(['orders', orderId]);
  }
}
