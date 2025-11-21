import { Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-server-error',
  imports: [RouterLink],
  templateUrl: './server-error-component.html',
  styleUrl: './server-error-component.scss',
})
export class ServerErrorComponent implements OnInit {
  error: any;
  constructor(private _router: Router) {
    const nav = this._router.getCurrentNavigation();
    this.error = nav?.extras?.state?.['error'];
  }

  ngOnInit(): void {
  }
}
