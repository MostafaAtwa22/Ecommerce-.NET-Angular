import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { BasketService } from '../../shared/services/basket-service';
import { DeliveryMethodService } from '../../shared/services/delivery-method-service';
import { IDeliveryMethod } from '../../shared/modules/deliveryMethod';

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
              private basketService: BasketService) {}

  ngOnInit(): void {
    this.deliveryMethodService.getAllDeliveryMethods().subscribe({
      next: dm => this.deliveryMethods = dm,
      error: err => console.log(err)
    });
  }

  trackById(index: number, method: IDeliveryMethod) {
    return method.id;
  }

  selectDelivery(method: IDeliveryMethod) {
    this.checkoutForm.get('deliveryMethod')?.setValue(method.id);
    this.basketService.setShippingPrice(method);
    this.deliverySelected.emit(method);
  }
}
