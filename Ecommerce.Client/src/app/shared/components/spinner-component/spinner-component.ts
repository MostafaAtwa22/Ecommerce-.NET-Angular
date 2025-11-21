import { Component } from '@angular/core';
import { BusyService } from '../../services/busy-service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-spinner-component',
  imports: [CommonModule],
  templateUrl: './spinner-component.html',
  styleUrl: './spinner-component.scss',
})
export class SpinnerComponent {
  constructor(public busyService: BusyService) {}
}
