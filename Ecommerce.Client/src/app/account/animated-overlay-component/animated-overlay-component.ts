import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-animated-overlay-component',
  imports: [CommonModule],
  templateUrl: './animated-overlay-component.html',
  styleUrl: './animated-overlay-component.scss',
})
export class AnimatedOverlayComponent {
  @Input() logoText: string = 'Tasaqolli';
  @Input() logoIcon: string = 'fas fa-shopping-bag';
  @Input() particleCount: number = 10;

  particles = Array(this.particleCount).fill(0);

  getParticleDelay(index: number): string {
    const delays = ['0s', '-2s', '-4s', '-6s', '-8s', '-10s', '-12s', '-14s', '-16s', '-18s'];
    return delays[index] || '0s';
  }

  getParticleDuration(index: number): string {
    const durations = ['20s', '18s', '22s', '16s', '19s', '21s', '17s', '23s', '15s', '24s'];
    return durations[index] || '20s';
  }
}
