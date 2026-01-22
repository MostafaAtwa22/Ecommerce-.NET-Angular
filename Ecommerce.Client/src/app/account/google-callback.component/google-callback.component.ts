import { Component, OnInit } from '@angular/core';
import { AccountService } from '../account-service';
import { Router } from '@angular/router';
import { AnimatedOverlayComponent } from "../animated-overlay-component/animated-overlay-component";

@Component({
  selector: 'app-google-callback',
  imports: [AnimatedOverlayComponent],
  templateUrl: './google-callback.component.html',
  styleUrl: './google-callback.component.scss',
})
export class GoogleCallbackComponent implements OnInit {
  constructor(private accountService: AccountService, private router: Router) {
  }

  async ngOnInit() {
    const result = await this.accountService.processGoogleLogin();
    console.log("From google");
    if (result) {
      result.subscribe({
        next: () => {
          this.router.navigate(['/home']);
        },
        error: (err) => {
          console.error('Google login failed:', err);
          this.router.navigate(['/login']);
        }
      });
    } else {
      this.router.navigate(['/login']);
    }
  }
}
