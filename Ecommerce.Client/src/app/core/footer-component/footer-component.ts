import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-footer-component',
  imports: [CommonModule, RouterLink],
  templateUrl: './footer-component.html',
  styleUrl: './footer-component.scss',
})
export class FooterComponent {
  currentYear = new Date().getFullYear();

  quickLinks = [
    { name: 'Home', path: '/home' },
    { name: 'Shop', path: '/shop' },
    { name: 'About Us', path: '/about' },
    { name: 'Contact', path: '/contact' }
  ];

  categories = [
    { name: 'Men\'s Fashion', path: '/shop?category=men' },
    { name: 'Women\'s Fashion', path: '/shop?category=women' },
    { name: 'Accessories', path: '/shop?category=accessories' },
    { name: 'New Arrivals', path: '/shop?category=new' }
  ];

  socialLinks = [
    { name: 'Facebook', icon: 'fab fa-facebook-f', url: '#' },
    { name: 'Instagram', icon: 'fab fa-instagram', url: '#' },
    { name: 'Twitter', icon: 'fab fa-twitter', url: '#' },
    { name: 'Pinterest', icon: 'fab fa-pinterest-p', url: '#' }
  ];
}
