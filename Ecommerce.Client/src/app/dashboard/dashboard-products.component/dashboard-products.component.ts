import { Component, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SweetAlertService } from '../../shared/services/sweet-alert.service';
import { IProduct, IProductCreate, IProductUpdate } from '../../shared/modules/product';
import { ShopParams } from '../../shared/modules/ShopParams';
import { HttpErrorResponse } from '@angular/common/http';
import { ShopService } from '../../shop/shop-service';
import { RouterLink } from '@angular/router';
import { IBrand } from '../../shared/modules/brand';
import { IType } from '../../shared/modules/type';
import { BrandService } from '../../shared/services/brand-service';
import { TypeService } from '../../shared/services/type-service';
import { ProductFormComponent } from './product-form.component/product-form.component';
import { HasPermissionDirective } from '../../shared/directives/has-permission.directive';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-dashboard-products',
  standalone: true,
  imports: [CommonModule, FormsModule, DecimalPipe, RouterLink, ProductFormComponent, HasPermissionDirective],
  templateUrl: './dashboard-products.component.html',
  styleUrls: ['./dashboard-products.component.scss'],
})
export class DashboardProductsComponent implements OnInit {
  products: IProduct[] = [];
  loading = false;
  errorMessage: string | null = null;

  // Statistics
  totalProducts = 0;
  totalRevenue = 0;
  averageRating = 0;
  outOfStockCount = 0;
  lowStockCount = 0;
  highRatedCount = 0;

  // Filter/Search
  shopParams = new ShopParams();
  showFilters = false;
  showProductForm = false;
  selectedProduct: IProduct | null = null;
  isEditing = false;

  // Pagination
  totalPages = 0;

  // Sort options
  sortOptions = [
    { value: 'Name', name: 'Name: A-Z' },
    { value: 'NameDesc', name: 'Name: Z-A' },
    { value: 'PriceAsc', name: 'Price: Low to High' },
    { value: 'PriceDesc', name: 'Price: High to Low' },
    { value: 'RatingDesc', name: 'Rating: High to Low' },
    { value: 'RatingAsc', name: 'Rating: Low to High' },
  ];

  // Brand and Type options (fetched from API)
  brands: IBrand[] = [];
  types: IType[] = [];
  loadingFilters = false;

  constructor(
    private shopService: ShopService,
    private brandService: BrandService,
    private typeService: TypeService,
    private sweetAlert: SweetAlertService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.loadFilters();
  }

  loadProducts(): void {
    this.loading = true;
    this.errorMessage = null;

    // Store original params and temporarily set dashboard-specific params
    const originalParams = { ...this.shopService.shopParams };
    this.shopService.shopParams = { ...this.shopParams };
    this.shopService.shopParams.pageSize = this.shopParams.pageSize;

    this.shopService.getAllProducts(false).subscribe({
      next: (response) => {
        this.products = response.data;
        this.totalProducts = response.totalData;
        this.totalPages = this.calculateTotalPages();
        this.calculateStatistics();
        this.loading = false;
        console.log(this.products);
        // Restore original params
        this.shopService.shopParams = originalParams;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        // Restore original params on error as well
        this.shopService.shopParams = originalParams;
        if (err.status === 401 || err.status === 403) {
          this.errorMessage = "You don't have permission to access products.";
        } else if (err.status === 0) {
          this.errorMessage =
            'Unable to connect to the server. Please check if the API is running.';
        } else {
          this.errorMessage =
            err.error?.message || 'An unexpected error occurred while loading products.';
        }

        this.toastr.error(this.errorMessage ?? "Error", 'Products Error', {
          timeOut: 6000,
          positionClass: 'toast-top-center',
          closeButton: true,
        });
      },
    });
  }

  loadFilters(): void {
    this.loadingFilters = true;

    // Load brands and types in parallel
    const loadBrands$ = this.brandService.getAllBrands();
    const loadTypes$ = this.typeService.getAllTypes();

    // Use Promise.all or forkJoin to load both simultaneously
    loadBrands$.subscribe({
      next: (brands) => {
        this.brands = brands;
        this.loadingFilters = false;
      },
      error: (err) => {
        console.error('Failed to load brands:', err);
        this.brands = [];
        this.loadingFilters = false;

        this.toastr.error('Failed to load brands list.', 'Filters Error', {
          timeOut: 5000,
          positionClass: 'toast-bottom-right',
          closeButton: true,
        });
      },
    });

    loadTypes$.subscribe({
      next: (types) => {
        this.types = types;
        this.loadingFilters = false;
      },
      error: (err) => {
        console.error('Failed to load types:', err);
        this.types = [];
        this.loadingFilters = false;

        this.toastr.error('Failed to load types list.', 'Filters Error', {
          timeOut: 5000,
          positionClass: 'toast-bottom-right',
          closeButton: true,
        });
      },
    });
  }

  calculateStatistics(): void {
    this.totalRevenue = 0;
    this.averageRating = 0;
    this.outOfStockCount = 0;
    this.lowStockCount = 0;
    this.highRatedCount = 0;

    let totalRating = 0;
    let ratedProducts = 0;

    this.products.forEach((product) => {
      const stock = product.quantity - product.boughtQuantity;

      // revenue = sold quantity * price
      this.totalRevenue += product.price * product.boughtQuantity;

      if (stock === 0) this.outOfStockCount++;
      if (stock > 0 && stock < 10) this.lowStockCount++;

      if (product.averageRating > 0) {
        totalRating += product.averageRating;
        ratedProducts++;

        if (product.averageRating >= 4) {
          this.highRatedCount++;
        }
      }
    });

    this.averageRating = ratedProducts > 0 ? totalRating / ratedProducts : 0;
  }

  calculateTotalPages(): number {
    if (this.totalProducts <= 0 || this.shopParams.pageSize <= 0) {
      return 0;
    }
    return Math.ceil(this.totalProducts / this.shopParams.pageSize);
  }

  onSearch(): void {
    this.shopParams.pageIndex = 1;
    this.shopService.setShopParams(this.shopParams);
    this.loadProducts();
  }

  resetSearch(): void {
    this.shopParams = new ShopParams();
    this.shopService.resetShopParams();
    this.loadProducts();
    this.showFilters = false;
  }

  onSortSelected(sort: string): void {
    this.shopParams.sort = sort;
    this.shopParams.pageIndex = 1;
    this.shopService.setShopParams(this.shopParams);
    this.loadProducts();
  }

  onBrandSelected(brandId: number): void {
    this.shopParams.brandId = brandId;
    this.shopParams.pageIndex = 1;
    this.shopService.setShopParams(this.shopParams);
    this.loadProducts();
  }

  onTypeSelected(typeId: number): void {
    this.shopParams.typeId = typeId;
    this.shopParams.pageIndex = 1;
    this.shopService.setShopParams(this.shopParams);
    this.loadProducts();
  }

  onPageChanged(pageIndex: number): void {
    this.shopParams.pageIndex = pageIndex;
    this.shopService.setShopParams(this.shopParams);
    this.loadProducts();
  }

  toggleFilters(): void {
    this.showFilters = !this.showFilters;
  }

  openAddProduct(): void {
    this.selectedProduct = null;
    this.isEditing = false;
    this.showProductForm = true;
  }

  openEditProduct(product: IProduct): void {
    this.selectedProduct = { ...product };
    this.isEditing = true;
    this.showProductForm = true;
  }

  closeProductForm(): void {
    this.showProductForm = false;
    this.selectedProduct = null;
  }

  onProductSubmit(payload: IProductCreate | IProductUpdate): void {
    if (this.isEditing && this.selectedProduct) {
      const updatePayload: IProductUpdate = {
        ...(payload as IProductUpdate),
        productId: this.selectedProduct.id,
      };

      this.shopService.updateProduct(updatePayload).subscribe({
        next: () => {
          this.sweetAlert.success('Product updated successfully');
          this.toastr.success('Product updated successfully.', 'Success', {
            timeOut: 3000,
            positionClass: 'toast-top-right',
            progressBar: true,
          });
          this.loadProducts();
          this.closeProductForm();
        },
        error: (err) => {
          this.sweetAlert.error('Update failed');
          const message = err?.error?.message || 'Failed to update product.';
          this.toastr.error(message, 'Update Failed', {
            timeOut: 6000,
            positionClass: 'toast-top-center',
            closeButton: true,
          });
        },
      });
    } else {
      const createPayload: IProductCreate = {
        ...(payload as IProductCreate),
        imageFile: (payload as IProductCreate).imageFile!,
      };

      this.shopService.createProduct(createPayload).subscribe({
        next: () => {
          this.sweetAlert.success('Product created successfully');
          this.toastr.success('Product created successfully.', 'Success', {
            timeOut: 3000,
            positionClass: 'toast-top-right',
            progressBar: true,
          });
          this.loadProducts();
          this.closeProductForm();
        },
        error: (err) => {
          this.sweetAlert.error('Creation failed');
          const message = err?.error?.message || 'Failed to create product.';
          this.toastr.error(message, 'Creation Failed', {
            timeOut: 6000,
            positionClass: 'toast-top-center',
            closeButton: true,
          });
        },
      });
    }
  }

  deleteProduct(id: number): void {
    this.sweetAlert
      .confirm({
        title: 'Delete Product',
        text: 'Are you sure you want to delete this product? This action cannot be undone.',
        icon: 'warning',
        confirmButtonText: 'Yes, delete it!',
      })
      .then((result) => {
        if (result.isConfirmed) {
          this.shopService.deleteProduct(id).subscribe({
            next: () => {
              this.sweetAlert.success('Product deleted successfully!');
              this.toastr.success('Product deleted successfully.', 'Success', {
                timeOut: 3000,
                positionClass: 'toast-top-right',
                progressBar: true,
              });
              this.loadProducts();
            },
            error: (err) => {
              this.sweetAlert.error('Failed to delete product. Please try again.');
              const message = err?.error?.message || 'Failed to delete product.';
              this.toastr.error(message, 'Delete Failed', {
                timeOut: 6000,
                positionClass: 'toast-top-center',
                closeButton: true,
              });
            },
          });
        }
      });
  }

  getStockStatusClass(product: IProduct): string {
    const stock = product.quantity - product.boughtQuantity;

    if (stock === 0) return 'badge-danger';
    if (stock < 10) return 'badge-warning';
    return 'badge-success';
  }


  getStockStatusText(product: IProduct): string {
    const stock = product.quantity - product.boughtQuantity;

    if (stock === 0) return 'Out of Stock';
    if (stock < 10) return 'Low Stock';
    return 'In Stock';
  }

  getRatingStars(rating: number | undefined): string {
    if (!rating) return 'No ratings';
    const stars = '★'.repeat(Math.floor(rating)) + '☆'.repeat(5 - Math.floor(rating));
    return `${stars} (${rating.toFixed(1)})`;
  }

  getRatingColor(rating: number | undefined): string {
    if (!rating) return 'text-muted';
    if (rating >= 4) return 'text-success';
    if (rating >= 3) return 'text-warning';
    return 'text-danger';
  }

  getMaxDisplayNumber(): number {
    return Math.min(this.shopParams.pageIndex * this.shopParams.pageSize, this.totalProducts);
  }

  getBrandName(brandId: number | undefined): string {
    if (!brandId) return 'N/A';
    const brand = this.brands.find((b) => b.id === brandId);
    return brand?.name ? String(brand.name) : 'Unknown Brand';
  }

  getTypeName(typeId: number | undefined): string {
    if (!typeId) return 'N/A';
    const type = this.types.find((t) => t.id === typeId);
    return type?.name ? String(type.name) : 'Unknown Type';
  }
}
