import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil, finalize } from 'rxjs';
import { SweetAlertService } from '../../shared/services/sweet-alert.service';
import { IDeliveryMethod } from '../../shared/modules/deliveryMethod';
import { DeliveryMethodService } from '../../shared/services/delivery-method-service';
import { DeliveryMethodFormComponent } from './delivery-method-form.component/delivery-method-form.component';
import { HasPermissionDirective } from '../../shared/directives/has-permission.directive';


@Component({
  selector: 'app-dashboard-delivery-method',
  standalone: true,
  imports: [CommonModule, FormsModule, DecimalPipe, DeliveryMethodFormComponent, HasPermissionDirective],
  templateUrl: './dashboard-delivery-method.component.html',
  styleUrls: ['./dashboard-delivery-method.component.scss']
})
export class DashboardDeliveryMethodComponent implements OnInit, OnDestroy {
  deliveryMethods: IDeliveryMethod[] = [];
  filteredMethods: IDeliveryMethod[] = [];
  loading = false;
  errorMessage: string | null = null;

  // Statistics
  totalMethods = 0;
  totalPrice = 0;
  averagePrice = 0;
  averageDeliveryTime = 0;
  freeShippingCount = 0;
  fastestDeliveryTime = 0;

  // UI State
  showFormModal = false;
  selectedMethod: IDeliveryMethod | null = null;
  isEditing = false;
  searchQuery = '';
  sortField: 'shortName' | 'price' | 'deliveryTime' = 'shortName';
  sortDirection: 'asc' | 'desc' = 'asc';

  private destroy$ = new Subject<void>();

  constructor(
    private deliveryService: DeliveryMethodService,
    private sweetAlert: SweetAlertService
  ) {}

  ngOnInit(): void {
    this.loadDeliveryMethods();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadDeliveryMethods(): void {
    this.loading = true;
    this.errorMessage = null;

    this.deliveryService.getAllDeliveryMethods()
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.loading = false)
      )
      .subscribe({
        next: (methods) => {
          this.deliveryMethods = methods;
          this.calculateStatistics();
          this.filterAndSort();
        },
        error: (err) => {
          console.error('Failed to load delivery methods:', err);
          this.errorMessage = 'Failed to load delivery methods. Please try again.';
          this.deliveryMethods = [];
          this.filteredMethods = [];
        }
      });
  }

  calculateStatistics(): void {
    this.totalMethods = this.deliveryMethods.length;

    if (this.totalMethods === 0) {
      this.totalPrice = 0;
      this.averagePrice = 0;
      this.averageDeliveryTime = 0;
      this.freeShippingCount = 0;
      this.fastestDeliveryTime = 0;
      return;
    }

    // Calculate total and average price
    this.totalPrice = this.deliveryMethods.reduce((sum, method) => sum + method.price, 0);
    this.averagePrice = this.totalPrice / this.totalMethods;

    // Calculate average delivery time
    const totalDeliveryTime = this.deliveryMethods.reduce((sum, method) => sum + method.deliveryTime, 0);
    this.averageDeliveryTime = totalDeliveryTime / this.totalMethods;

    // Count free shipping methods
    this.freeShippingCount = this.deliveryMethods.filter(m => m.price === 0).length;

    // Find fastest delivery time
    this.fastestDeliveryTime = Math.min(...this.deliveryMethods.map(m => m.deliveryTime));
  }

  filterAndSort(): void {
    // Filter by search query
    let filtered = this.deliveryMethods;

    if (this.searchQuery) {
      const query = this.searchQuery.toLowerCase();
      filtered = filtered.filter(method =>
        method.shortName.toLowerCase().includes(query) ||
        method.description.toLowerCase().includes(query)
      );
    }

    // Sort the results
    filtered.sort((a, b) => {
      let aValue: any = a[this.sortField];
      let bValue: any = b[this.sortField];

      // For string sorting
      if (this.sortField === 'shortName') {
        aValue = aValue.toLowerCase();
        bValue = bValue.toLowerCase();
      }

      if (aValue < bValue) return this.sortDirection === 'asc' ? -1 : 1;
      if (aValue > bValue) return this.sortDirection === 'asc' ? 1 : -1;
      return 0;
    });

    this.filteredMethods = filtered;
  }

  onSearch(): void {
    this.filterAndSort();
  }

  onSort(field: 'shortName' | 'price' | 'deliveryTime'): void {
    if (this.sortField === field) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortField = field;
      this.sortDirection = 'asc';
    }
    this.filterAndSort();
  }

  getSortIcon(field: 'shortName' | 'price' | 'deliveryTime'): string {
    if (this.sortField !== field) return 'fa-sort';
    return this.sortDirection === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
  }

  openAddMethod(): void {
    this.selectedMethod = null;
    this.isEditing = false;
    this.showFormModal = true;
  }

  openEditMethod(method: IDeliveryMethod): void {
    this.selectedMethod = { ...method };
    this.isEditing = true;
    this.showFormModal = true;
  }

  closeFormModal(): void {
    this.showFormModal = false;
    this.selectedMethod = null;
  }

  onMethodSubmit(methodData: Partial<IDeliveryMethod>): void {
    if (this.isEditing && this.selectedMethod) {
      const updatedMethod: IDeliveryMethod = {
        ...this.selectedMethod,
        ...methodData
      };
      this.deliveryService.updateDeliveryMethod(this.selectedMethod.id, updatedMethod)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.sweetAlert.success('Delivery method updated successfully');
            this.loadDeliveryMethods();
            this.closeFormModal();
          },
          error: () => this.sweetAlert.error('Failed to update delivery method')
        });
    } else {
      this.deliveryService.createDeliveryMethod(methodData as IDeliveryMethod)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.sweetAlert.success('Delivery method created successfully');
            this.loadDeliveryMethods();
            this.closeFormModal();
          },
          error: () => this.sweetAlert.error('Failed to create delivery method')
        });
    }
  }

  deleteMethod(method: IDeliveryMethod): void {
    this.sweetAlert.confirm({
      title: 'Delete Delivery Method',
      text: `Are you sure you want to delete "${method.shortName}"? This action cannot be undone.`,
      icon: 'warning',
      confirmButtonText: 'Yes, delete it!',
      cancelButtonText: 'Cancel'
    }).then(result => {
      if (result.isConfirmed) {
        this.deliveryService.deleteDeliveryMethod(method.id)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: () => {
              this.sweetAlert.success('Delivery method deleted successfully');
              this.loadDeliveryMethods();
            },
            error: () => this.sweetAlert.error('Failed to delete delivery method')
          });
      }
    });
  }

  getPriceColor(price: number): string {
    if (price === 0) return 'text-success';
    if (price < 10) return 'text-primary';
    if (price < 25) return 'text-info';
    return 'text-warning';
  }

  getDeliveryTimeClass(deliveryTime: number): string {
    if (deliveryTime <= 1) return 'badge-success';
    if (deliveryTime <= 3) return 'badge-warning';
    return 'badge-info';
  }

  getDeliveryTimeText(deliveryTime: number): string {
    if (deliveryTime === 1) return 'Next day';
    if (deliveryTime <= 3) return `${deliveryTime} days`;
    if (deliveryTime <= 7) return `${deliveryTime} days`;
    return `${deliveryTime} days`;
  }
}
