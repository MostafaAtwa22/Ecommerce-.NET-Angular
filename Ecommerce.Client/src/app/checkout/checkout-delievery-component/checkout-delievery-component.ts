import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { BasketService } from '../../shared/services/basket-service';
import { DeliveryMethodService } from '../../shared/services/delivery-method-service';
import { IDeliveryMethod } from '../../shared/modules/deliveryMethod';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-checkout-delievery-component',
  templateUrl: './checkout-delievery-component.html',
  styleUrl: './checkout-delievery-component.scss',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, CurrencyPipe]
})
export class CheckoutDelieveryComponent implements OnInit {
  @Input() checkoutForm!: FormGroup;
  @Output() deliverySelected = new EventEmitter<IDeliveryMethod>();

  deliveryMethods: IDeliveryMethod[] = [];

  constructor(private deliveryMethodService: DeliveryMethodService,
              private basketService: BasketService,
              private toastr: ToastrService) {}

  ngOnInit(): void {
    this.deliveryMethodService.getAllDeliveryMethods().subscribe({
      next: dm => this.deliveryMethods = dm,
      error: err => {
        console.log(err);
        this.toastr.error('Failed to load delivery methods. Please refresh the page.', 'Error', {
          timeOut: 6000,
          positionClass: 'toast-top-center',
          closeButton: true,
        });
      }
    });
  }

  trackById(index: number, method: IDeliveryMethod) {
    return method.id;
  }

  selectDelivery(method: IDeliveryMethod) {
    this.checkoutForm.get('deliveryMethod')?.setValue(method.id);
    this.basketService.setShippingPrice(method);
    this.deliverySelected.emit(method);

    this.toastr.success(`Delivery method selected: ${method.shortName}`, 'Delivery Updated', {
      timeOut: 3000,
      positionClass: 'toast-bottom-right',
      progressBar: true,
    });
  }
}
