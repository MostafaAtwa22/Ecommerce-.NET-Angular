import { Component, OnInit, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; // IMPORTANT: Add this for ngModel
import { Chart, registerables } from 'chart.js';
import { Subscription } from 'rxjs';

import { IProduct } from '../../shared/modules/product';
import { IAllOrders } from '../../shared/modules/order';
import { IBrand } from '../../shared/modules/brand';
import { IType } from '../../shared/modules/type';
import { IRole } from '../../shared/modules/roles';
import { IProfile } from '../../shared/modules/profile';
import { ShopService } from '../../shop/shop-service';
import { CheckoutService } from '../../checkout/checkout-service';
import { ProfileService } from '../../shared/services/profile-service';
import { BrandService } from '../../shared/services/brand-service';
import { TypeService } from '../../shared/services/type-service';
import { RoleService } from '../../shared/services/role.service';
import { getOrderStatusLabel } from '../../shared/modules/order-status';
import { HasPermissionDirective } from '../../shared/directives/has-permission.directive';

@Component({
  selector: 'app-dashboard-main',
  standalone: true,
  imports: [CommonModule, FormsModule, HasPermissionDirective], // Added FormsModule here
  templateUrl: './dashboard-main.component.html',
  styleUrls: ['./dashboard-main.component.scss']
})
export class DashboardMainComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('ordersChart') ordersChartRef: any;
  @ViewChild('distributionChart') distributionChartRef: any;

  // Statistics
  totalUsers: number = 0;
  totalRoles: number = 0;
  totalOrders: number = 0;
  totalProducts: number = 0;

  // Charts Data
  selectedPeriod: string = '7';
  chartType: string = 'brand';

  ordersChart: Chart | undefined;
  distributionChart: Chart | undefined;

  ordersData: IAllOrders[] = [];
  productsData: IProduct[] = [];
  brandsData: IBrand[] = [];
  typesData: IType[] = [];
  rolesData: IRole[] = [];
  usersData: IProfile[] = [];

  // Recent Data
  recentProducts: IProduct[] = [];
  recentOrders: IAllOrders[] = [];
  topBoughtProducts: IProduct[] = [];

  // Loading States
  loadingProducts: boolean = true;
  loadingOrders: boolean = true;
  loadingStats: boolean = true;
  loadingTopBought: boolean = true;

  // Subscriptions
  private subscriptions: Subscription[] = [];

  constructor(
    private shopService: ShopService,
    private checkoutService: CheckoutService,
    private profileService: ProfileService,
    private brandService: BrandService,
    private typeService: TypeService,
    private roleService: RoleService
  ) {
    Chart.register(...registerables);
  }

  ngOnInit(): void {
    this.loadAllData();
  }

  ngAfterViewInit(): void {
    // Charts will be initialized after data loads
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    if (this.ordersChart) this.ordersChart.destroy();
    if (this.distributionChart) this.distributionChart.destroy();
  }

  loadAllData(): void {
    this.loadStatistics();
    this.loadRecentProducts();
    this.loadRecentOrders();
    this.loadBrandsAndTypes();
    this.loadRoles();
    this.loadTopBoughtProducts();
  }

  loadStatistics(): void {
    this.loadingStats = true;

    // Save current shop params
    const originalPageIndex = this.shopService.shopParams.pageIndex;
    const originalPageSize = this.shopService.shopParams.pageSize;
    const originalSort = this.shopService.shopParams.sort;
    const originalBrandId = this.shopService.shopParams.brandId;
    const originalTypeId = this.shopService.shopParams.typeId;
    const originalSearch = this.shopService.shopParams.search;

    // Set params to get ALL products (large pageSize)
    this.shopService.shopParams.pageIndex = 1;
    this.shopService.shopParams.pageSize = 10000; // Large number to get all products
    this.shopService.shopParams.sort = '';
    delete this.shopService.shopParams.brandId;
    delete this.shopService.shopParams.typeId;
    delete this.shopService.shopParams.search;

    // Load products count and ALL products data
    const productsSub = this.shopService.getAllProducts(false).subscribe({
      next: (response) => {
        this.totalProducts = response.totalData;
        this.productsData = response.data; // All products for statistics and distribution chart
        this.loadingStats = false;

        // Restore original params
        this.shopService.shopParams.pageIndex = originalPageIndex;
        this.shopService.shopParams.pageSize = originalPageSize;
        this.shopService.shopParams.sort = originalSort;
        this.shopService.shopParams.brandId = originalBrandId;
        this.shopService.shopParams.typeId = originalTypeId;
        this.shopService.shopParams.search = originalSearch;

        // Update distribution chart after products load
        setTimeout(() => this.updateDistributionChart(), 100);
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.loadingStats = false;
        // Restore original params on error
        this.shopService.shopParams.pageIndex = originalPageIndex;
        this.shopService.shopParams.pageSize = originalPageSize;
        this.shopService.shopParams.sort = originalSort;
        this.shopService.shopParams.brandId = originalBrandId;
        this.shopService.shopParams.typeId = originalTypeId;
        this.shopService.shopParams.search = originalSearch;
      }
    });

    // Load orders count
    const ordersSub = this.checkoutService.getAllOrders(false).subscribe({
      next: (response) => {
        this.totalOrders = response.totalData;
        this.ordersData = response.data;
      },
      error: (error) => console.error('Error loading orders:', error)
    });

    // Load users count
    const usersSub = this.profileService.getAllUsers(false).subscribe({
      next: (response) => {
        this.totalUsers = response.totalData;
        this.usersData = response.data;
      },
      error: (error) => console.error('Error loading users:', error)
    });

    this.subscriptions.push(productsSub, ordersSub, usersSub);
  }

  loadRecentProducts(): void {
    this.loadingProducts = true;
    this.shopService.shopParams.pageIndex = 1;
    this.shopService.shopParams.pageSize = 5;
    this.shopService.shopParams.sort = 'createdAtDesc';

    const sub = this.shopService.getAllProducts().subscribe({
      next: (response) => {
        this.recentProducts = response.data;
        this.loadingProducts = false;
        // Initialize charts after products load
        setTimeout(() => this.initializeCharts(), 100);
      },
      error: (error) => {
        console.error('Error loading recent products:', error);
        this.loadingProducts = false;
      }
    });
    this.subscriptions.push(sub);
  }

  loadRecentOrders(): void {
    this.loadingOrders = true;
    this.checkoutService.ordersParams.pageIndex = 1;
    this.checkoutService.ordersParams.pageSize = 5;
    this.checkoutService.ordersParams.sort = 'orderDateDesc';

    const sub = this.checkoutService.getAllOrders().subscribe({
      next: (response) => {
        this.recentOrders = response.data;
        this.loadingOrders = false;
      },
      error: (error) => {
        console.error('Error loading recent orders:', error);
        this.loadingOrders = false;
      }
    });
    this.subscriptions.push(sub);
  }

  loadBrandsAndTypes(): void {
    // Load brands
    const brandsSub = this.brandService.getAllBrands().subscribe({
      next: (brands) => {
        this.brandsData = brands;
        this.updateDistributionChart();
      },
      error: (error) => console.error('Error loading brands:', error)
    });

    // Load types
    const typesSub = this.typeService.getAllTypes().subscribe({
      next: (types) => {
        this.typesData = types;
      },
      error: (error) => console.error('Error loading types:', error)
    });

    this.subscriptions.push(brandsSub, typesSub);
  }

  loadRoles(): void {
    const rolesSub = this.roleService.getAllRoles().subscribe({
      next: (roles) => {
        this.totalRoles = roles.length;
        this.rolesData = roles;
      },
      error: (error) => console.error('Error loading roles:', error)
    });
    this.subscriptions.push(rolesSub);
  }

  loadTopBoughtProducts(): void {
    this.loadingTopBought = true;

    // Save current shop params
    const originalPageIndex = this.shopService.shopParams.pageIndex;
    const originalPageSize = this.shopService.shopParams.pageSize;
    const originalSort = this.shopService.shopParams.sort;
    const originalBrandId = this.shopService.shopParams.brandId;
    const originalTypeId = this.shopService.shopParams.typeId;
    const originalSearch = this.shopService.shopParams.search;

    // Set params to get ALL products
    this.shopService.shopParams.pageIndex = 1;
    this.shopService.shopParams.pageSize = 10000;
    this.shopService.shopParams.sort = '';
    delete this.shopService.shopParams.brandId;
    delete this.shopService.shopParams.typeId;
    delete this.shopService.shopParams.search;

    const sub = this.shopService.getAllProducts(false).subscribe({
      next: (response) => {
        // Sort by BoughtQuantity descending and take top 5
        this.topBoughtProducts = response.data
          .filter(p => p.boughtQuantity > 0) // Only products that have been bought
          .sort((a, b) => b.boughtQuantity - a.boughtQuantity)
          .slice(0, 5);

        this.loadingTopBought = false;

        // Restore original params
        this.shopService.shopParams.pageIndex = originalPageIndex;
        this.shopService.shopParams.pageSize = originalPageSize;
        this.shopService.shopParams.sort = originalSort;
        this.shopService.shopParams.brandId = originalBrandId;
        this.shopService.shopParams.typeId = originalTypeId;
        this.shopService.shopParams.search = originalSearch;
      },
      error: (error) => {
        console.error('Error loading top bought products:', error);
        this.loadingTopBought = false;
        // Restore original params on error
        this.shopService.shopParams.pageIndex = originalPageIndex;
        this.shopService.shopParams.pageSize = originalPageSize;
        this.shopService.shopParams.sort = originalSort;
        this.shopService.shopParams.brandId = originalBrandId;
        this.shopService.shopParams.typeId = originalTypeId;
        this.shopService.shopParams.search = originalSearch;
      }
    });

    this.subscriptions.push(sub);
  }

  getBoughtPercentage(product: IProduct): number {
    if (product.quantity === 0) return 0;
    return (product.boughtQuantity / product.quantity) * 100;
  }

  initializeCharts(): void {
    this.initializeOrdersChart();
    this.initializeDistributionChart();
  }

  initializeOrdersChart(): void {
    if (!this.ordersChartRef?.nativeElement) return;

    if (this.ordersChart) {
      this.ordersChart.destroy();
    }

    const ctx = this.ordersChartRef.nativeElement.getContext('2d');
    const period = parseInt(this.selectedPeriod);

    // Generate sample data for orders over time
    const labels = this.generateDateLabels(period);
    const data = this.generateOrdersData(period);

    this.ordersChart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: labels,
        datasets: [{
          label: 'Orders',
          data: data,
          borderColor: '#5624d0',
          backgroundColor: 'rgba(86, 36, 208, 0.1)',
          borderWidth: 2,
          fill: true,
          tension: 0.4,
          pointBackgroundColor: '#5624d0',
          pointBorderColor: '#fff',
          pointBorderWidth: 2,
          pointRadius: 4
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: true,
            position: 'top',
            labels: {
              color: '#6a6f73',
              font: {
                size: 12
              }
            }
          },
          tooltip: {
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            titleColor: '#fff',
            bodyColor: '#fff',
            padding: 12,
            cornerRadius: 4
          }
        },
        scales: {
          x: {
            grid: {
              color: 'rgba(209, 215, 220, 0.3)'
            },
            ticks: {
              color: '#6a6f73'
            }
          },
          y: {
            beginAtZero: true,
            grid: {
              color: 'rgba(209, 215, 220, 0.3)'
            },
            ticks: {
              color: '#6a6f73',
              precision: 0
            }
          }
        }
      }
    });
  }

  initializeDistributionChart(): void {
    if (!this.distributionChartRef?.nativeElement) return;

    if (this.distributionChart) {
      this.distributionChart.destroy();
    }

    const ctx = this.distributionChartRef.nativeElement.getContext('2d');

    const isBrandChart = this.chartType === 'brand';
    const labels = isBrandChart
      ? this.brandsData.map(b => b.name)
      : this.typesData.map(t => t.name);

    const data = isBrandChart
      ? this.getProductsByBrand()
      : this.getProductsByType();

    const backgroundColors = this.generateChartColors(labels.length);

    this.distributionChart = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: labels,
        datasets: [{
          data: data,
          backgroundColor: backgroundColors,
          borderColor: '#fff',
          borderWidth: 2,
          hoverOffset: 15
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'right',
            labels: {
              color: '#6a6f73',
              font: {
                size: 11
              },
              padding: 20
            }
          },
          tooltip: {
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            titleColor: '#fff',
            bodyColor: '#fff',
            padding: 12,
            cornerRadius: 4
          }
        },
        cutout: '60%'
      }
    });
  }

  updateOrdersChart(): void {
    if (this.ordersChart) {
      this.ordersChart.destroy();
    }
    this.initializeOrdersChart();
  }

  updateDistributionChart(): void {
    if (this.distributionChart) {
      this.distributionChart.destroy();
    }
    this.initializeDistributionChart();
  }

  // Helper Methods
  generateDateLabels(days: number): string[] {
    const labels: string[] = [];
    const today = new Date();

    for (let i = days - 1; i >= 0; i--) {
      const date = new Date(today);
      date.setDate(date.getDate() - i);
      labels.push(date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }));
    }

    return labels;
  }

  generateOrdersData(days: number): number[] {
    const data: number[] = [];

    // Generate realistic order data (replace with real data)
    for (let i = 0; i < days; i++) {
      const base = 10 + Math.floor(Math.random() * 20);
      const variation = Math.floor(Math.random() * 15);
      data.push(base + variation);
    }

    return data;
  }

  getProductsByBrand(): number[] {
    const brandCounts: { [key: string]: number } = {};

    // Initialize counts
    this.brandsData.forEach(brand => {
      brandCounts[brand.name] = 0;
    });

    // Count products by brand
    this.productsData.forEach(product => {
      const brandName = product.productBrandName;
      if (brandName && brandCounts[brandName] !== undefined) {
        brandCounts[brandName]++;
      }
    });

    return Object.values(brandCounts);
  }

  getProductsByType(): number[] {
    const typeCounts: { [key: string]: number } = {};

    // Initialize counts
    this.typesData.forEach(type => {
      typeCounts[type.name] = 0;
    });

    // Count products by type
    this.productsData.forEach(product => {
      const typeName = product.productTypeName;
      if (typeName && typeCounts[typeName] !== undefined) {
        typeCounts[typeName]++;
      }
    });

    return Object.values(typeCounts);
  }

  generateChartColors(count: number): string[] {
    const colors = [
      '#5624d0', '#4caf50', '#ff9800', '#2196f3', '#f44336',
      '#9c27b0', '#00bcd4', '#8bc34a', '#ff5722', '#607d8b'
    ];

    return colors.slice(0, count);
  }

  formatDate(dateString: string | Date): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  getStatusLabel(status: string | number): string {
    return getOrderStatusLabel(status);
  }

  getStatusBadgeClass(status: string | number): string {
    const statusStr = String(status || '').toLowerCase();
    switch (statusStr) {
      case 'pending':
        return 'bg-warning text-dark';
      case 'shipped':
        return 'bg-info text-dark';
      case 'complete':
        return 'bg-success';
      case 'canceled':
        return 'bg-danger';
      default:
        return 'bg-secondary';
    }
  }

  getStatusIcon(status: string | number): string {
    const statusStr = String(status || '').toLowerCase();
    switch (statusStr) {
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
}
