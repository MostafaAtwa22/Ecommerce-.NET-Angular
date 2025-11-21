import { Component, OnInit, AfterViewInit } from '@angular/core';
import { IProduct } from '../shared/modules/product';
import { IPagination } from '../shared/modules/pagination';
import { ShopService } from './shop-service';
import { ProductItemComponent } from "./product-item-component/product-item-component";
import { IBrand } from '../shared/modules/brand';
import { IType } from '../shared/modules/type';
import { BrandService } from '../shared/services/brand-service';
import { TypeService } from '../shared/services/type-service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ShopParams } from '../shared/modules/ShopParams';
import { PaginationComponent } from '../shared/components/pagination-component/pagination-component';

declare var bootstrap: any;

@Component({
  selector: 'app-shop-component',
  imports: [CommonModule, ProductItemComponent, FormsModule, PaginationComponent],
  templateUrl: './shop-component.html',
  styleUrl: './shop-component.scss',
})
export class ShopComponent implements OnInit, AfterViewInit {
  products: IProduct[] = [];
  brands: IBrand[] = [];
  types: IType[] = [];
  totalCount = 0;
  showFilters = false;
  isLoading = true;
  shopParams = new ShopParams();

  sortOptions = [
    { name: 'Alphabetical', value: 'name' },
    { name: 'Price: Low to High', value: 'PriceAsc' },
    { name: 'Price: High to Low', value: 'PriceDesc' },
  ];

  constructor(
    private _shopService: ShopService,
    private _brandService: BrandService,
    private _typeService: TypeService
  ) {}

  ngOnInit(): void {
    this.getAllBrands();
    this.getAllTypes();
    this.getAllProducts();
  }

  ngAfterViewInit(): void {
    const dropdownElementList = [].slice.call(document.querySelectorAll('.dropdown-toggle'));
    dropdownElementList.map((el) => new bootstrap.Dropdown(el));
    this.observeProductItems();
  }

  toggleFilters() {
    this.showFilters = !this.showFilters;
  }

  getAllProducts() {
    this.isLoading = true;

    this._shopService.getAllProducts(this.shopParams).subscribe({
      next: (res: IPagination<IProduct>) => {
        this.products = res.data;
        this.totalCount = res.totalData;
        this.isLoading = false;

        // Re-observe product items after products load
        setTimeout(() => {
          this.observeProductItems();
        }, 100);
      },
      error: (err) => {
        console.error('Error loading products:', err);
        this.isLoading = false;
      },
    });
  }

  getAllBrands() {
    this._brandService.getAllBrands().subscribe({
      next: (res: IBrand[]) => (this.brands = res),
    });
  }

  getAllTypes() {
    this._typeService.getAllTypes().subscribe({
      next: (res: IType[]) => (this.types = res),
    });
  }

  onBrandSelected(brandId: number) {
    this.shopParams.brandId = this.shopParams.brandId === brandId ? undefined : brandId;
    this.shopParams.pageIndex = 1;
    this.getAllProducts();
  }

  onTypeSelected(typeId: number) {
    this.shopParams.typeId = this.shopParams.typeId === typeId ? undefined : typeId;
    this.shopParams.pageIndex = 1;
    this.getAllProducts();
  }

  onSortSelected(sort: string) {
    this.shopParams.sort = sort;
    this.shopParams.pageIndex = 1;
    this.getAllProducts();
  }

  onSearch() {
    this.shopParams.pageIndex = 1;
    this.getAllProducts();
  }

  resetSearch() {
    this.shopParams.search = '';
    this.shopParams.pageIndex = 1;
    this.getAllProducts();
  }

  resetFilters() {
    this.shopParams = new ShopParams();
    this.getAllProducts();
  }

  onPageChanged(page: number) {
    this.shopParams.pageIndex = page;
    this.getAllProducts();
  }

  private observeProductItems() {
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            entry.target.classList.add('fade-in');
            // Optional: Unobserve after animation to improve performance
            // observer.unobserve(entry.target);
          }
        });
      },
      {
        threshold: 0.1, // Trigger when 10% of the element is visible
        rootMargin: '0px 0px -50px 0px' // Adjust trigger point (negative bottom margin)
      }
    );

    // Clear previous observations
    const existingItems = document.querySelectorAll('.fade-item');
    existingItems.forEach(item => {
      item.classList.remove('fade-in'); // Reset animation state
    });

    // Observe all product items after a short delay
    setTimeout(() => {
      const productItems = document.querySelectorAll('.fade-item');
      productItems.forEach(item => observer.observe(item));
    }, 50);
  }
}
