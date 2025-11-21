import { Directive, ElementRef, HostBinding, OnInit } from '@angular/core';

@Directive({
  selector: '[scrollAnimate]'
})
export class ScrollAnimateDirective {
  @HostBinding('class.visible') isVisible = false;

  constructor(private el: ElementRef) {}

  ngOnInit() {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          this.isVisible = true;
          observer.unobserve(this.el.nativeElement);
        }
      },
      { threshold: 0.1 }
    );

    observer.observe(this.el.nativeElement);
  }

}
