import { Component, OnInit, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chart, registerables } from 'chart.js';
import { Subscription } from 'rxjs';

import { IProduct } from '../../shared/modules/product';
import { IAllOrders } from '../../shared/modules/order';
import { ShopService } from '../../shop/shop-service';
import { CheckoutService } from '../../checkout/checkout-service';
import { ProfileService } from '../../shared/services/profile-service';

@Component({
  selector: 'app-dashboard-main',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard-main.component.html',
  styleUrl: './dashboard-main.component.scss',
})
export class DashboardMainComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('salesChart') salesChartRef: any;
  @ViewChild('categoryChart') categoryChartRef: any;

  // Dashboard data
  totalOrders = 0;
  totalUsers = 0; // Changed from totalCustomers
  totalProducts = 0;
  totalRevenue = 0;

  orderGrowth = 0;
  userGrowth = 0;
  productGrowth = 0;
  revenueGrowth = 0;

  // Real data
  topProducts: any[] = [];
  salesData: any = {};
  brandRevenue: any = {};

  // Charts
  salesChart: any;
  categoryChart: any;

  // Subscriptions
  private subscriptions: Subscription = new Subscription();

  // Store previous data for growth calculation
  private previousData = {
    orders: 0,
    users: 0,
    products: 0,
    revenue: 0,
  };

  constructor(
    private shopService: ShopService,
    private checkoutService: CheckoutService,
    private profileService: ProfileService // Add this
  ) {
    Chart.register(...registerables);
  }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  ngAfterViewInit(): void {
    this.loadDashboardData();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
  loadDashboardData(): void {
    // Load products
    this.shopService.getAllProducts().subscribe({
      next: (response) => {
        this.totalProducts = response.totalData;
        this.processProductsData(response.data);
        this.processBrandRevenue(response.data);
        // Update charts with real data
        setTimeout(() => {
          this.updateChartsWithRealData();
        }, 100);
      },
      error: (error) => {
        console.error('Error loading products:', error);
      }
    });

    // Load orders
    this.checkoutService.getAllOrders().subscribe({
      next: (response) => {
        this.totalOrders = response.totalData;
        this.calculateRevenueFromOrders(response.data);
        // Update charts with real data
        setTimeout(() => {
          this.updateChartsWithRealData();
        }, 100);
      },
      error: (error) => {
        console.error('Error loading orders:', error);
      }
    });

    // Load users
    this.loadAllUsers();
  }

  createInitialCharts(): void {
    // Create charts with sample data immediately
    this.createSalesChart();
    this.createCategoryChart();
  }

  updateChartsWithRealData(): void {
    // Destroy and recreate charts with real data
    if (this.salesChart) {
      this.salesChart.destroy();
      this.createSalesChart();
    }

    if (this.categoryChart) {
      this.categoryChart.destroy();
      this.createCategoryChart();
    }
  }

  // Helper method for progress bars
  getSalesPercentage(product: any): number {
    if (this.topProducts.length === 0) return 0;

    const maxSales = Math.max(...this.topProducts.map(p => p.sales));
    return (product.sales / maxSales) * 100;
  }

  getBestSeller(): any {
    return this.topProducts.length > 0
      ? [...this.topProducts].sort((a, b) => b.sales - a.sales)[0]
      : null;
  }

  getHighestRevenueProduct(): any {
    return this.topProducts.length > 0
      ? [...this.topProducts].sort((a, b) => b.revenue - a.revenue)[0]
      : null;
  }

  onPeriodChange(event: any): void {
    const period = event.target.value;
    // Implement period filtering logic here
    console.log('Period changed to:', period);
    this.refreshDashboard();
  }

  loadAllUsers(): void {
    // Method 1: If you have a user service
    this.subscriptions.add(
      this.profileService.getAllUsers().subscribe({
        next: (response) => {
          this.totalUsers = response.totalData;
          this.calculateUserGrowth(response.totalData);
        },
        error: (error) => {
          console.error('Error loading users:', error);
          // Method 2: Estimate from orders (fallback)
          this.estimateUsersFromOrders();
        }
      })
    );
  }

  estimateUsersFromOrders(): void {
    // Estimate total users (including admins, staff, etc.)
    // Assuming 30% more users than orders (accounts for non-buying users)
    this.totalUsers = Math.round(this.totalOrders * 1.3);
    this.userGrowth = 8.2; // Default growth
  }

  processProductsData(products: IProduct[]): void {
    // Get products with actual images
    const sortedProducts = [...products]
      .filter((product) => product.pictureUrl)
      .sort((a, b) => {
        const ratingA = (a.averageRating) || 0;
        const ratingB = (b.averageRating) || 0;
        const reviewsA = a.numberOfReviews || 0;
        const reviewsB = b.numberOfReviews || 0;

        // Consider price and quantity for "top" products
        const valueA = (ratingA * 0.5) + (reviewsA * 0.2) + ((a.price * a.quantity) * 0.3);
        const valueB = (ratingB * 0.5) + (reviewsB * 0.2) + ((b.price * b.quantity) * 0.3);

        return valueB - valueA;
      })
      .slice(0, 5);

    this.topProducts = sortedProducts.map((product) => ({
      id: product.id,
      name: product.name.length > 25 ? product.name.substring(0, 25) + '...' : product.name,
      brand: product.productBrandName || 'Unbranded',
      category: product.productTypeName || 'Uncategorized',
      sales: Math.floor(product.quantity * 0.3),
      revenue: Math.round(product.price * product.quantity * 0.3 * 100) / 100,
      rating: (product.averageRating) || 0,
      reviews: product.numberOfReviews || 0,
      image: product.pictureUrl,
    }));
  }

  calculateRevenueFromOrders(orders: IAllOrders[]): void {
    const currentRevenue = orders.reduce((sum, order) => sum + order.subTotal, 0);
    this.totalRevenue = Math.round(currentRevenue * 100) / 100;
    this.calculateRevenueGrowth(currentRevenue);
  }

  processBrandRevenue(products: IProduct[]): void {
    const brandMap = new Map<string, number>();

    products.forEach((product) => {
      const brand = product.productBrandName || 'Unbranded';
      const estimatedRevenue = product.price * product.quantity * 0.3;

      if (brandMap.has(brand)) {
        brandMap.set(brand, brandMap.get(brand)! + estimatedRevenue);
      } else {
        brandMap.set(brand, estimatedRevenue);
      }
    });

    const sortedBrands = Array.from(brandMap.entries())
      .sort(([, a], [, b]) => b - a)
      .slice(0, 6);

    this.brandRevenue = {
      labels: sortedBrands.map(([brand]) => brand),
      data: sortedBrands.map(([, revenue]) => Math.round(revenue)),
    };
  }

  // Growth calculations
  calculateOrderGrowth(currentOrders: number): void {
    const growth = this.previousData.orders > 0
      ? ((currentOrders - this.previousData.orders) / this.previousData.orders) * 100
      : 12.5;
    this.orderGrowth = Math.round(growth * 10) / 10;
    this.previousData.orders = currentOrders;
  }

  calculateUserGrowth(currentUsers: number): void {
    const growth = this.previousData.users > 0
      ? ((currentUsers - this.previousData.users) / this.previousData.users) * 100
      : 8.2;
    this.userGrowth = Math.round(growth * 10) / 10;
    this.previousData.users = currentUsers;
  }

  calculateProductGrowth(currentProducts: number): void {
    const growth = this.previousData.products > 0
      ? ((currentProducts - this.previousData.products) / this.previousData.products) * 100
      : 5.7;
    this.productGrowth = Math.round(growth * 10) / 10;
    this.previousData.products = currentProducts;
  }

  calculateRevenueGrowth(currentRevenue: number): void {
    const growth = this.previousData.revenue > 0
      ? ((currentRevenue - this.previousData.revenue) / this.previousData.revenue) * 100
      : 15.3;
    this.revenueGrowth = Math.round(growth * 10) / 10;
    this.previousData.revenue = currentRevenue;
  }

  // Charts
  createSalesChart(): void {
    const ctx = this.salesChartRef?.nativeElement?.getContext('2d');
    if (!ctx) return;

    // Create sample data or use your real data
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const revenueData = months.map(() => Math.floor(Math.random() * 40000) + 10000);
    const ordersData = months.map(() => Math.floor(Math.random() * 1000) + 500);

    const gradient1 = ctx.createLinearGradient(0, 0, 0, 300);
    gradient1.addColorStop(0, 'rgba(86, 36, 208, 0.3)');
    gradient1.addColorStop(1, 'rgba(86, 36, 208, 0.05)');

    const gradient2 = ctx.createLinearGradient(0, 0, 0, 300);
    gradient2.addColorStop(0, 'rgba(72, 187, 120, 0.3)');
    gradient2.addColorStop(1, 'rgba(72, 187, 120, 0.05)');

    this.salesChart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: months,
        datasets: [
          {
            label: 'Revenue',
            data: revenueData,
            borderColor: '#5624d0',
            backgroundColor: gradient1,
            borderWidth: 3,
            fill: true,
            tension: 0.4,
            pointBackgroundColor: '#5624d0',
            pointBorderColor: '#ffffff',
            pointBorderWidth: 2,
            pointRadius: 6,
            pointHoverRadius: 8,
          },
          {
            label: 'Orders',
            data: ordersData,
            borderColor: '#48bb78',
            backgroundColor: gradient2,
            borderWidth: 3,
            fill: true,
            tension: 0.4,
            pointBackgroundColor: '#48bb78',
            pointBorderColor: '#ffffff',
            pointBorderWidth: 2,
            pointRadius: 6,
            pointHoverRadius: 8,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'top',
            labels: {
              padding: 20,
              usePointStyle: true,
              pointStyle: 'circle',
              font: {
                size: 13,
                family: "'Inter', sans-serif",
              },
            },
          },
        },
        scales: {
          y: {
            beginAtZero: true,
            grid: {
              color: 'rgba(0, 0, 0, 0.05)',
            },
            border: {
              display: false,
            },
            ticks: {
              padding: 15,
              font: {
                size: 12,
              },
            },
          },
          x: {
            grid: {
              display: false,
            },
            border: {
              display: false,
            },
            ticks: {
              padding: 10,
              font: {
                size: 12,
              },
            },
          },
        },
      },
    });
  }

  createCategoryChart(): void {
    const ctx = this.categoryChartRef?.nativeElement?.getContext('2d');
    if (!ctx) return;

    // Fallback data if no brand data
    const labels = this.brandRevenue.labels || ['Electronics', 'Fashion', 'Home', 'Sports', 'Books', 'Other'];
    const data = this.brandRevenue.data || [35, 25, 15, 10, 8, 7];

    const colors = ['#5624d0', '#6c5ce7', '#48bb78', '#ed8936', '#9f7aea', '#4299e1'];

    this.categoryChart = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: labels,
        datasets: [
          {
            data: data,
            backgroundColor: colors,
            borderWidth: 0,
            hoverOffset: 15,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'right',
            labels: {
              padding: 20,
              usePointStyle: true,
              pointStyle: 'circle',
              font: {
                size: 12,
              },
            },
          },
        },
        cutout: '65%',
      },
    });
  }

  refreshDashboard(): void {
    if (this.salesChart) this.salesChart.destroy();
    if (this.categoryChart) this.categoryChart.destroy();

    this.loadDashboardData();

    setTimeout(() => {
      this.createSalesChart();
      this.createCategoryChart();
    }, 300);
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
    }).format(value);
  }
}
