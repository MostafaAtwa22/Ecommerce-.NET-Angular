import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { IOrder } from '../../shared/modules/order';
import { AnimatedOverlayComponent } from "../../account/animated-overlay-component/animated-overlay-component";
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-checkout-success-component',
  imports: [AnimatedOverlayComponent],
  templateUrl: './checkout-success-component.html',
  styleUrl: './checkout-success-component.scss',
})
export class CheckoutSuccessComponent {
  order!: IOrder;
  orderId!: string;
  deliveryMethod!: string;

  constructor(private router: Router, private toastr: ToastrService) {
    const nav = this.router.getCurrentNavigation();
    const state = nav?.extras?.state;
    if (state) {
      this.order = state as IOrder;
      this.orderId = String(this.order.id) || '';
    }
  }

  ngOnInit(): void {
    console.log(this.order);

    if (!this.order) {
      this.toastr.error('Order details are missing. Please check your orders history.', 'Error', {
        timeOut: 6000,
        positionClass: 'toast-top-center',
        closeButton: true,
      });
      return;
    }

    if (!this.orderId || this.orderId === '') {
      this.orderId = 'ORD-' + Math.floor(100000 + Math.random() * 900000);
    }
    this.deliveryMethod = this.order.deliveryMethod;

    this.toastr.success('Your order has been placed successfully!', 'Order Confirmed', {
      timeOut: 5000,
      positionClass: 'toast-top-right',
      progressBar: true,
      closeButton: true,
    });
  }

  viewOrder(): void {
    this.router.navigate(['/orders', this.orderId]);
  }

  continueShopping(): void {
    this.router.navigate(['/']);
  }
}
