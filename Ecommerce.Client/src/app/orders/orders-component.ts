import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CheckoutService } from '../checkout/checkout-service';
import { IOrder } from '../shared/modules/order';
import { CurrencyPipe, DatePipe } from '@angular/common';

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

  viewOrderDetails(orderId: number) {
    this.router.navigate(['orders', orderId]);
  }
}
