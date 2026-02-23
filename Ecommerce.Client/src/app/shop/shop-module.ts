import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ShopComponent } from './shop-component';
import { ProductDetailsComponent } from './product-details-component/product-details-component';



@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    ShopComponent,
    ProductDetailsComponent
  ],
  exports: [
    ShopComponent,
    ProductDetailsComponent
  ]
})
export class ShopModule { }
