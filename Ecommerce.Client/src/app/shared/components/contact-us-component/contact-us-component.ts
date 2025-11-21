import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';

@Component({
  selector: 'app-contact-us-component',
  imports: [CommonModule, FormsModule],
  templateUrl: './contact-us-component.html',
  styleUrl: './contact-us-component.scss',
})
export class ContactUsComponent {
  contactForm : any = {
    name: '',
    email: '',
    subject: '',
    message: ''
  };

  isSubmitting = false;
  isSubmitted = false;

  onSubmit() {
    this.isSubmitting = true;

    // Simulate form submission
    setTimeout(() => {
      this.isSubmitting = false;
      this.isSubmitted = true;
      this.contactForm = { name: '', email: '', subject: '', message: '' };

      // Reset success message after 5 seconds
      setTimeout(() => {
        this.isSubmitted = false;
      }, 5000);
    }, 2000);
  }
}
