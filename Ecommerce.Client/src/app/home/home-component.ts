import { Component } from '@angular/core';
import { SliderComponent } from "../shared/components/slider-component/slider-component";
import { ShopComponent } from "../shop/shop-component";
import { ReviewsComponent } from "../shared/components/reviews-component/reviews-component";
import { ContactUsComponent } from "../shared/components/contact-us-component/contact-us-component";
import { FooterComponent } from "../core/footer-component/footer-component";
import { ScrollAnimateDirective } from '../shared/directives/scroll-animate-directive';

@Component({
  selector: 'app-home-component',
  imports: [SliderComponent, ShopComponent,
    ReviewsComponent, ContactUsComponent, FooterComponent,
  ScrollAnimateDirective],
  templateUrl: './home-component.html',
  styleUrl: './home-component.scss',
})
export class HomeComponent {

}
