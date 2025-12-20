import { Component, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IAllOrders } from '../../shared/modules/order';
import { HttpErrorResponse } from '@angular/common/http';
import { OrdersParams } from '../../shared/modules/OrdersParams';
import { CheckoutService } from '../../checkout/checkout-service';
import { getOrderStatusLabel } from '../../shared/modules/order-status';

@Component({
  selector: 'app-dashboard-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, DecimalPipe],
  templateUrl: './dashboard-orders.component.html',
  styleUrls: ['./dashboard-orders.component.scss']
})
export class DashboardOrdersComponent implements OnInit {
  orders: IAllOrders[] = [];
  loading = false;
  errorMessage: string | null = null;

  // Statistics
  totalOrders = 0;
  totalRevenue = 0;
  pendingCount = 0;
  shippedCount = 0;
  completeCount = 0;
  canceledCount = 0;

  // Filter/Search
  ordersParams = new OrdersParams();
  showFilters = false;
  showOrderDetails = false;
  selectedOrder: any = null;

  // Pagination
  totalPages = 0;

  // Sort options
  sortOptions = [
    { value: 'DateDesc', name: 'Newest First' },
    { value: 'DateAsc', name: 'Oldest First' },
    { value: 'AmountDesc', name: 'Amount: High to Low' },
    { value: 'AmountAsc', name: 'Amount: Low to High' },
    { value: 'Status', name: 'By Status' }
  ];

  // Status filter options
  statusOptions = [
    { value: '', name: 'All Statuses' },
    { value: 'Pending', name: 'Pending' },
    { value: 'Shipped', name: 'Shipped' },
    { value: 'Complete', name: 'Complete' },
    { value: 'Canceled', name: 'Canceled' }
  ];

  constructor(private checkoutService: CheckoutService) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.errorMessage = null;

    this.checkoutService.getAllOrders(false).subscribe({
      next: (response) => {
        this.orders = response.data;
        this.totalOrders = response.totalData;
        this.totalPages = this.calculateTotalPages();
        this.calculateStatistics();
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        if (err.status === 401 || err.status === 403) {
          this.errorMessage = 'You don\'t have permission to access orders.';
        } else if (err.status === 0) {
          this.errorMessage = 'Unable to connect to the server. Please check if the API is running.';
        } else {
          this.errorMessage = err.error?.message || 'An unexpected error occurred while loading orders.';
        }
      }
    });
  }

  calculateStatistics(): void {
    this.totalRevenue = 0;
    this.pendingCount = 0;
    this.shippedCount = 0;
    this.completeCount = 0;
    this.canceledCount = 0;

    this.orders.forEach(order => {
      // Convert status to string to handle cases where API returns non-string types
      const statusStr = String(order.status || '').toLowerCase();
      
      // Calculate total revenue (only from completed orders)
      if (statusStr === 'complete') {
        this.totalRevenue += order.subTotal;
      }

      // Count by status
      switch(statusStr) {
        case 'pending':
          this.pendingCount++;
          break;
        case 'shipped':
          this.shippedCount++;
          break;
        case 'complete':
          this.completeCount++;
          break;
        case 'canceled':
          this.canceledCount++;
          break;
      }
    });
  }

  calculateTotalPages(): number {
    if (this.totalOrders <= 0 || this.ordersParams.pageSize <= 0) {
      return 0;
    }
    return Math.ceil(this.totalOrders / this.ordersParams.pageSize);
  }

  onSearch(): void {
    this.ordersParams.pageIndex = 1;
    this.checkoutService.setOrdersParams(this.ordersParams);
    this.loadOrders();
  }

  resetSearch(): void {
    this.ordersParams = new OrdersParams();
    this.checkoutService.setOrdersParams(this.ordersParams);
    this.loadOrders();
  }

  onSortSelected(sort: string): void {
    this.ordersParams.sort = sort;
    this.ordersParams.pageIndex = 1;
    this.checkoutService.setOrdersParams(this.ordersParams);
    this.loadOrders();
  }

  onStatusSelected(status: string): void {

    this.ordersParams.pageIndex = 1;
    this.checkoutService.setOrdersParams(this.ordersParams);
    this.loadOrders();
  }

  onPageChanged(pageIndex: number): void {
    this.ordersParams.pageIndex = pageIndex;
    this.checkoutService.setOrdersParams(this.ordersParams);
    this.loadOrders();
  }

  toggleFilters(): void {
    this.showFilters = !this.showFilters;
  }

  resetFilters(): void {
    this.resetSearch();
    this.showFilters = false;
  }

  viewOrderDetails(order: IAllOrders): void {
    // You would typically fetch full order details here
    this.selectedOrder = {
      ...order,
      items: [], // Add order items here if available
      shippingAddress: {}, // Add shipping address here
      deliveryMethod: {} // Add delivery method here
    };
    this.showOrderDetails = true;
  }

  closeOrderDetails(): void {
    this.showOrderDetails = false;
    this.selectedOrder = null;
  }

  updateOrderStatus(orderId: number, newStatus: string): void {
    // Implement order status update logic here
    console.log(`Update order ${orderId} to status: ${newStatus}`);
    // Call your service to update order status
  }

  getStatusLabel(status: string | number): string {
    return getOrderStatusLabel(status);
  }

  getStatusBadgeClass(status: string): string {
    const statusStr = String(status || '').toLowerCase();
    switch(statusStr) {
      case 'pending':
        return 'badge-warning';
      case 'shipped':
        return 'badge-info';
      case 'complete':
        return 'badge-success';
      case 'canceled':
        return 'badge-danger';
      default:
        return 'badge-secondary';
    }
  }

  getStatusIcon(status: string): string {
    const statusStr = String(status || '').toLowerCase();
    switch(statusStr) {
      case 'pending':
        return 'fa-clock';
      case 'shipped':
        return 'fa-truck';
      case 'complete':
        return 'fa-check-circle';
      case 'canceled':
        return 'fa-times-circle';
      default:
        return 'fa-question-circle';
    }
  }

  getInitials(firstName: string, lastName: string): string {
    return `${firstName?.charAt(0) || ''}${lastName?.charAt(0) || ''}`.toUpperCase();
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getMaxDisplayNumber(): number {
    return Math.min(this.ordersParams.pageIndex * this.ordersParams.pageSize, this.totalOrders);
  }
}
