import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { IOrder } from '../../shared/modules/order';

@Component({
  selector: 'app-checkout-success-component',
  imports: [],
  templateUrl: './checkout-success-component.html',
  styleUrl: './checkout-success-component.scss',
})
export class CheckoutSuccessComponent {
  order!: IOrder;
  orderId!: string;
  deliveryMethod!: string;

  constructor(private router: Router) {
    const nav = this.router.getCurrentNavigation();
    const state = nav?.extras?.state;
    if (state) {
      this.order = state as IOrder;
      this.orderId = String(this.order.id) || '';
    }
  }

  ngOnInit(): void {
    console.log(this.order);
    if (!this.orderId || this.orderId === '') {
      this.orderId = 'ORD-' + Math.floor(100000 + Math.random() * 900000);
    }
    this.deliveryMethod = this.order.deliveryMethod;
  }

  viewOrder(): void {
    this.router.navigate(['/orders', this.orderId]);
  }

  continueShopping(): void {
    this.router.navigate(['/']);
  }
}
