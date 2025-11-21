import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-reviews-component',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reviews-component.html',
  styleUrls: ['./reviews-component.scss'],
})
export class ReviewsComponent {
  reviews = [
    {
      name: 'Sarah Johnson',
      role: 'Fashion Enthusiast',
      message: 'I\'ve been shopping here for over a year now, and I\'m consistently amazed by the quality and variety of products. The website is easy to navigate, the checkout process is smooth, and the delivery is always on time. Every purchase feels like a premium experience. It\'s so rare to find an online store that genuinely cares about both design and customer satisfaction. Highly recommended!',
      rating: 5,
    },
    {
      name: 'Michael Brown',
      role: 'Frequent Buyer',
      message: 'This platform has completely changed the way I shop online. The product descriptions are clear, and what you see is exactly what you get. I\'ve ordered multiple times and never been disappointed. The packaging is neat, and the return process is hassle-free. I love how responsive their customer support team is—quick replies and genuine help every time. Easily my favorite online store.',
      rating: 4,
    },
    {
      name: 'Emily Davis',
      role: 'Verified Customer',
      message: 'It\'s rare to find an eCommerce site that combines style, speed, and great service all in one place. Every time I place an order, I\'m confident it will arrive quickly and exactly as shown. The sales and discounts are a fantastic bonus! Their designs are fresh and trendy without being overpriced. I\'ve already recommended it to several friends, and they all love it too!',
      rating: 5,
    },
    {
      name: 'Daniel Lee',
      role: 'Tech Professional',
      message: 'I spend most of my time working remotely, and this store has made upgrading my wardrobe effortless. The layout is intuitive, making it easy to find exactly what I need. I appreciate the attention to detail in both the products and the service. Shipping updates are prompt, and the items always arrive in perfect condition. The whole experience feels premium without being expensive.',
      rating: 4,
    },
    {
      name: 'Sophia Martinez',
      role: 'Online Shopper',
      message: 'I joined during one of their flash sales and instantly became a loyal customer. The variety of items, the amazing deals, and the sleek presentation make shopping here enjoyable. I\'m always excited to see new arrivals, and the discounts make it even better. The checkout process is fast and secure, and I\'ve never had an issue with my orders. Fantastic brand and amazing value!',
      rating: 5,
    },
  ];

  currentSlide = 0;

  nextSlide() {
    this.currentSlide = (this.currentSlide + 1) % this.reviews.length;
  }

  prevSlide() {
    this.currentSlide = (this.currentSlide - 1 + this.reviews.length) % this.reviews.length;
  }

  goToSlide(index: number) {
    this.currentSlide = index;
  }
}
