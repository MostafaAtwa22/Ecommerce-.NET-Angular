import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chart, registerables } from 'chart.js';

import { IProduct } from '../../shared/modules/product';
import { IAllOrders } from '../../shared/modules/order';
import { ShopService } from '../../shop/shop-service';
import { CheckoutService } from '../../checkout/checkout-service';

@Component({
  selector: 'app-dashboard-main',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard-main.component.html',
  styleUrl: './dashboard-main.component.scss',
})
export class DashboardMainComponent implements OnInit, AfterViewInit {
  @ViewChild('salesChart') salesChartRef: any;
  @ViewChild('categoryChart') categoryChartRef: any;

  // Dashboard data
  totalOrders = 0;
  totalUsers = 0; // You'll need a users service for this
  totalProducts = 0;
  totalRevenue = 0;

  orderGrowth = 12.5;
  userGrowth = 8.2;
  productGrowth = 5.7;
  revenueGrowth = 15.3;

  // Sample data (will be replaced with real data)
  topProducts: any[] = [];
  recentOrders: any[] = [];

  // Charts
  salesChart: any;
  categoryChart: any;

  constructor(
    private shopService: ShopService,
    private checkoutService: CheckoutService
  ) {
    Chart.register(...registerables);
  }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  ngAfterViewInit(): void {
    // Delay chart creation to ensure data is loaded
    setTimeout(() => {
      this.createSalesChart();
      this.createCategoryChart();
    }, 100);
  }

  loadDashboardData(): void {
    // Load products
    this.shopService.getAllProducts().subscribe({
      next: (response) => {
        this.totalProducts = response.totalData;
        this.processProductsData(response.data);
      },
      error: (error) => {
        console.error('Error loading products:', error);
      }
    });

    // Load orders
    this.checkoutService.getAllOrders().subscribe({
      next: (response) => {
        this.totalOrders = response.totalData;
        this.processOrdersData(response.data);
        this.calculateRevenue(response.data);
      },
      error: (error) => {
        console.error('Error loading orders:', error);
      }
    });

    // Load users (you'll need to implement this service)
    // this.userService.getAllUsers().subscribe(...)
  }

  processProductsData(products: IProduct[]): void {
    // Get top 5 products (simulated - you might want to sort by sales or revenue)
    this.topProducts = products.slice(0, 5).map(product => ({
      name: product.name,
      category: product.productBrandName || 'Uncategorized',
      sales: Math.floor(Math.random() * 1000) + 100, // Simulated sales
      revenue: (product.price || 0) * (Math.floor(Math.random() * 1000) + 100), // Simulated revenue
      image: product.pictureUrl || 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=100&h=100&fit=crop'
    }));
  }

  processOrdersData(orders: IAllOrders[]): void {
    // Get recent orders
    this.recentOrders = orders.slice(0, 5).map(order => ({
      id: `ORD${order.id.toString().padStart(3, '0')}`,
      customer: {
        name: order.firstName.split('@')[0] || 'Customer',
        avatar: `https://ui-avatars.com/api/?name=${order.firstName.split('@')[0]}&background=5624d0&color=fff`
      },
      date: new Date(order.orderDate).toISOString().split('T')[0],
      amount: order.subTotal,
      status: this.getOrderStatus(order)
    }));
  }

  calculateRevenue(orders: IAllOrders[]): void {
    this.totalRevenue = orders.reduce((sum, order) => sum + order.subTotal, 0);
  }

  getOrderStatus(order: IAllOrders): string {
    // Map your order status to dashboard statuses
    // This is a simple mapping - adjust based on your actual order statuses
    const statusMap: { [key: string]: string } = {
      'Pending': 'pending',
      'Processing': 'processing',
      'Shipped': 'shipped',
      'Delivered': 'completed',
      'Cancelled': 'cancelled'
    };

    return statusMap[order.status] || 'pending';
  }

  createSalesChart(): void {
    const ctx = this.salesChartRef?.nativeElement?.getContext('2d');
    if (!ctx) return;

    this.salesChart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
        datasets: [{
          label: 'Sales',
          data: [12000, 19000, 15000, 25000, 22000, 30000, 28000, 32000, 29000, 35000, 40000, 42000],
          borderColor: '#5624d0',
          backgroundColor: 'rgba(86, 36, 208, 0.1)',
          borderWidth: 2,
          fill: true,
          tension: 0.4
        }, {
          label: 'Revenue',
          data: [8000, 12000, 10000, 18000, 15000, 22000, 20000, 25000, 23000, 28000, 32000, 35000],
          borderColor: '#48bb78',
          backgroundColor: 'rgba(72, 187, 120, 0.1)',
          borderWidth: 2,
          fill: true,
          tension: 0.4
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'top',
          }
        },
        scales: {
          y: {
            beginAtZero: true,
            grid: {
              drawOnChartArea: false
            },
            ticks: {
              callback: function(value: any) {
                return '$' + value;
              }
            }
          },
          x: {
            grid: {
              display: false
            }
          }
        }
      }
    });
  }

  createCategoryChart(): void {
    const ctx = this.categoryChartRef?.nativeElement?.getContext('2d');
    if (!ctx) return;

    this.categoryChart = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: ['Electronics', 'Fashion', 'Home & Garden', 'Sports', 'Books', 'Other'],
        datasets: [{
          data: [35, 25, 15, 12, 8, 5],
          backgroundColor: [
            '#5624d0',
            '#48bb78',
            '#ed8936',
            '#9f7aea',
            '#4299e1',
            '#cbd5e0'
          ],
          borderWidth: 2,
          borderColor: '#ffffff'
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'right',
          }
        },
        cutout: '70%'
      }
    });
  }

  refreshDashboard(): void {
    this.loadDashboardData();
    if (this.salesChart) {
      this.salesChart.destroy();
    }
    if (this.categoryChart) {
      this.categoryChart.destroy();
    }
    setTimeout(() => {
      this.createSalesChart();
      this.createCategoryChart();
    }, 100);
  }
}
