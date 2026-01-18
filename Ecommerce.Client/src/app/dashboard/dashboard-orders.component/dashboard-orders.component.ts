import { Component, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IAllOrders, IOrder, IUpdateOrderStatusDto } from '../../shared/modules/order';
import { HttpErrorResponse } from '@angular/common/http';
import { OrdersParams } from '../../shared/modules/OrdersParams';
import { CheckoutService } from '../../checkout/checkout-service';
import { getOrderStatusLabel, OrderStatus } from '../../shared/modules/order-status';

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
  loadingStatistics = false;
  errorMessage: string | null = null;
  updatingStatus = false;
  updateOrderId: number | null = null; // Track which order is being updated

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
  selectedOrder: IOrder | null = null;

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

  // Status filter options - update to match enum
  statusOptions = [
    { value: '', name: 'All Statuses' },
    { value: OrderStatus.Pending.toString(), name: 'Pending' },
    { value: OrderStatus.Shipped.toString(), name: 'Shipped' },
    { value: OrderStatus.Complete.toString(), name: 'Complete' },
    { value: OrderStatus.Canceled.toString(), name: 'Canceled' }
  ];

  // OrderStatus for template access
  OrderStatus = OrderStatus;

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
        this.loading = false;

        // Load statistics separately (all orders, no pagination)
        this.loadStatistics();
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

  loadStatistics(): void {
    this.loadingStatistics = true;

    // Load all orders without pagination for statistics
    this.checkoutService.getAllOrders(true).subscribe({
      next: (response) => {
        const allOrders = response.data;
        this.calculateStatisticsFromArray(allOrders);
        this.loadingStatistics = false;
      },
      error: (err: HttpErrorResponse) => {
        console.error('Error loading statistics:', err);
        this.loadingStatistics = false;
        // Fallback: calculate from current page if we can't get all orders
        this.calculateStatisticsFromArray(this.orders);
      }
    });
  }

  calculateStatisticsFromArray(ordersArray: IAllOrders[]): void {
    this.totalRevenue = 0;
    this.pendingCount = 0;
    this.shippedCount = 0;
    this.completeCount = 0;
    this.canceledCount = 0;

    ordersArray.forEach(order => {
      // Use helper to get consistent string representation
      const statusStr = getOrderStatusLabel(order.status).toLowerCase();

      // Only count revenue from completed orders
      if (statusStr === 'complete') {
        this.totalRevenue += order.total;
      }

      // Count orders by status
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

  updateStatisticsOnStatusChange(oldStatus: string, newStatus: string, orderTotal: number): void {
    const oldStatusStr = getOrderStatusLabel(oldStatus).toLowerCase();
    const newStatusStr = getOrderStatusLabel(newStatus).toLowerCase();

    switch(oldStatusStr) {
      case 'pending':
        this.pendingCount = Math.max(0, this.pendingCount - 1);
        break;
      case 'shipped':
        this.shippedCount = Math.max(0, this.shippedCount - 1);
        break;
      case 'complete':
        this.completeCount = Math.max(0, this.completeCount - 1);
        this.totalRevenue = Math.max(0, this.totalRevenue - orderTotal);
        break;
      case 'canceled':
        this.canceledCount = Math.max(0, this.canceledCount - 1);
        break;
    }

    switch(newStatusStr) {
      case 'pending':
        this.pendingCount++;
        break;
      case 'shipped':
        this.shippedCount++;
        break;
      case 'complete':
        this.completeCount++;
        this.totalRevenue += orderTotal;
        break;
      case 'canceled':
        this.canceledCount++;
        break;
    }
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
    this.ordersParams.status = status;
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
    this.loading = true;
    this.errorMessage = null;
    this.checkoutService.getOrderDetailsById(order.id).subscribe({
      next: (orderDetails: IOrder) => {
        this.selectedOrder = orderDetails;
        this.showOrderDetails = true;
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        console.error('Error loading order details:', err);
        this.errorMessage = 'Failed to load order details. Please try again.';
      }
    });
  }

  closeOrderDetails(): void {
    this.showOrderDetails = false;
    this.selectedOrder = null;
    this.errorMessage = null;
  }

  // Helper to convert status to number
  private getOrderStatusNumber(status: string | number): number {
    if (typeof status === 'number') {
      return status;
    }

    // If it's already a number string, parse it
    if (!isNaN(Number(status))) {
      return parseInt(status, 10);
    }

    // If it's a string label, map it to enum value
    switch (status.toLowerCase()) {
      case 'pending':
        return OrderStatus.Pending;
      case 'paymentreceived':
      case 'payment received':
        return OrderStatus.PaymentReceived;
      case 'paymentfailed':
      case 'payment failed':
        return OrderStatus.PaymentFailed;
      case 'shipped':
        return OrderStatus.Shipped;
      case 'complete':
        return OrderStatus.Complete;
      case 'canceled':
      case 'cancelled': // Handle both spellings
        return OrderStatus.Canceled;
      default:
        return OrderStatus.Pending;
    }
  }

  // Helper to check if order can be cancelled
  canCancelOrder(orderStatus: string | number): boolean {
    const statusNum = this.getOrderStatusNumber(orderStatus);
    // Can cancel if not already cancelled or complete
    return statusNum !== OrderStatus.Canceled && statusNum !== OrderStatus.Complete;
  }

  // Helper to check if order can be shipped
  canShipOrder(orderStatus: string | number): boolean {
    const statusNum = this.getOrderStatusNumber(orderStatus);
    // Can ship if status is Pending
    return statusNum === OrderStatus.Pending;
  }

  // Helper to check if order can be marked complete
  canCompleteOrder(orderStatus: string | number): boolean {
    const statusNum = this.getOrderStatusNumber(orderStatus);
    // Can mark complete if status is Shipped
    return statusNum === OrderStatus.Shipped;
  }

  updateOrderStatus(orderId: number, newStatus: string): void {
    if (!orderId || !newStatus) return;

    // Add confirmation for cancellation
    if (newStatus === OrderStatus.Canceled.toString()) {
      if (!confirm('Are you sure you want to cancel this order? This action cannot be undone.')) {
        return;
      }
    }

    this.updatingStatus = true;
    this.updateOrderId = orderId;
    this.errorMessage = null;

    this.checkoutService.updateOrderStatus(orderId, newStatus).subscribe({
      next: (updatedOrder: IOrder) => {
        if (this.selectedOrder && this.selectedOrder.id === orderId) {
          this.selectedOrder = updatedOrder;
        }

        // Update the order in the current page
        const orderIndex = this.orders.findIndex(o => o.id === orderId);
        if (orderIndex !== -1) {
          const oldStatus = this.orders[orderIndex].status;
          this.orders[orderIndex].status = newStatus;

          // Update statistics based on status change
          this.updateStatisticsOnStatusChange(oldStatus, newStatus, updatedOrder.total);
        }

        this.updatingStatus = false;
        this.updateOrderId = null;
      },
      error: (err: HttpErrorResponse) => {
        this.updatingStatus = false;
        this.updateOrderId = null;
        console.error('Error updating order status:', err);
        this.errorMessage = err.error?.message || 'Failed to update order status. Please try again.';
      }
    });
  }

  getStatusLabel(status: string | number): string {
    return getOrderStatusLabel(status);
  }

  getStatusBadgeClass(status: string | number): string {
    const statusNum = this.getOrderStatusNumber(status);
    switch(statusNum) {
      case OrderStatus.Pending:
        return 'badge-warning';
      case OrderStatus.PaymentReceived:
        return 'badge-info';
      case OrderStatus.PaymentFailed:
        return 'badge-danger';
      case OrderStatus.Shipped:
        return 'badge-primary';
      case OrderStatus.Complete:
        return 'badge-success';
      case OrderStatus.Canceled:
        return 'badge-danger';
      default:
        return 'badge-secondary';
    }
  }

  getStatusIcon(status: string | number): string {
    const statusNum = this.getOrderStatusNumber(status);
    switch(statusNum) {
      case OrderStatus.Pending:
        return 'fa-clock';
      case OrderStatus.PaymentReceived:
        return 'fa-credit-card';
      case OrderStatus.PaymentFailed:
        return 'fa-times-circle';
      case OrderStatus.Shipped:
        return 'fa-truck';
      case OrderStatus.Complete:
        return 'fa-check-circle';
      case OrderStatus.Canceled:
        return 'fa-ban';
      default:
        return 'fa-question-circle';
    }
  }

  getInitials(firstName?: string, lastName?: string): string {
    return `${firstName?.charAt(0) || ''}${lastName?.charAt(0) || ''}`.toUpperCase();
  }

  formatDate(dateString: string | Date): string {
    if (!dateString) return 'N/A';
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
