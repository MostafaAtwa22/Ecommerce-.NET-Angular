import { Component, OnDestroy, OnInit, AfterViewInit } from '@angular/core';
import { IProduct, IProductSuggestion } from '../shared/modules/product';
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
import { Subject, catchError, debounceTime, distinctUntilChanged, of, switchMap, takeUntil } from 'rxjs';

declare var bootstrap: any;

@Component({
  selector: 'app-shop-component',
  imports: [CommonModule, ProductItemComponent, FormsModule, PaginationComponent],
  templateUrl: './shop-component.html',
  styleUrl: './shop-component.scss',
})
export class ShopComponent implements OnInit, AfterViewInit, OnDestroy {
  products: IProduct[] = [];
  brands: IBrand[] = [];
  types: IType[] = [];
  totalCount = 0;
  showFilters = false;
  isLoading = true;
  shopParams!: ShopParams;
  searchSuggestions: IProductSuggestion[] = [];
  showSuggestions = false;
  activeSuggestionIndex = -1;

  private destroy$ = new Subject<void>();
  private searchInput$ = new Subject<string>();
  private blurTimeout: ReturnType<typeof setTimeout> | null = null;

  sortOptions = [
    { name: 'Alphabetical', value: 'name' },
    { name: 'Price: Low to High', value: 'PriceAsc' },
    { name: 'Price: High to Low', value: 'PriceDesc' }
  ];

  constructor(
    private _shopService: ShopService,
    private _brandService: BrandService,
    private _typeService: TypeService
  ) {
    this.shopParams = this._shopService.getShopParams();
  }

  ngOnInit(): void {
    this.setupSearchSuggestions();
    this.getAllBrands();
    this.getAllTypes();
    this.getAllProducts(true);
  }

  ngOnDestroy(): void {
    if (this.blurTimeout) {
      clearTimeout(this.blurTimeout);
    }

    this.destroy$.next();
    this.destroy$.complete();
  }

  ngAfterViewInit(): void {
    const dropdownElementList = [].slice.call(document.querySelectorAll('.dropdown-toggle'));
    dropdownElementList.map((el) => new bootstrap.Dropdown(el));

    this.observeProductItems();
  }

  toggleFilters() {
    this.showFilters = !this.showFilters;
  }

  getAllProducts(useCache = false) {
    this.isLoading = true;

    this._shopService.getAllProducts(useCache).subscribe({
      next: (res: IPagination<IProduct>) => {
        this.products = res.data;
        this.totalCount = res.totalData;
        this.isLoading = false;

        setTimeout(() => this.observeProductItems(), 100);
      },
      error: (err) => {
        console.error('Error loading products:', err);
        this.isLoading = false;
      }
    });
  }

  getAllBrands() {
    this._brandService.getAllBrands().subscribe({
      next: res => this.brands = res
    });
  }

  getAllTypes() {
    this._typeService.getAllTypes().subscribe({
      next: res => this.types = res
    });
  }

  onBrandSelected(brandId: number) {
    const params = this._shopService.getShopParams();

    params.brandId = (params.brandId === brandId) ? undefined : brandId;
    params.pageIndex = 1;

    this._shopService.setShopParams(params);
    this.refreshSuggestionsForCurrentSearch();
    this.getAllProducts();
  }

  onTypeSelected(typeId: number) {
    const params = this._shopService.getShopParams();

    params.typeId = (params.typeId === typeId) ? undefined : typeId;
    params.pageIndex = 1;

    this._shopService.setShopParams(params);
    this.refreshSuggestionsForCurrentSearch();
    this.getAllProducts();
  }

  onSortSelected(sort: string) {
    const params = this._shopService.getShopParams();

    params.sort = sort;
    params.pageIndex = 1;

    this._shopService.setShopParams(params);
    this.getAllProducts();
  }

  onSearch() {
    this.hideSuggestions();

    const params = this._shopService.getShopParams();
    params.pageIndex = 1;

    this._shopService.setShopParams(params);
    this.getAllProducts();
  }

  resetSearch() {
    this.clearSuggestions();
    this.searchInput$.next('');

    const params = this._shopService.getShopParams();
    params.search = '';
    params.pageIndex = 1;

    this._shopService.setShopParams(params);
    this.getAllProducts();
  }

  resetFilters() {
    this.shopParams = new ShopParams();
    this.clearSuggestions();
    this.searchInput$.next('');
    this._shopService.setShopParams(this.shopParams);
    this.getAllProducts();
  }

  onPageChanged(page: number) {
    const params = this._shopService.getShopParams();
    params.pageIndex = page;

    this._shopService.setShopParams(params);
    this.getAllProducts(true);
  }

  onSearchInput(): void {
    this.searchInput$.next(this.shopParams.search || '');
  }

  onSearchFocus(): void {
    if (this.searchSuggestions.length > 0) {
      this.showSuggestions = true;
    }
  }

  onSearchBlur(): void {
    if (this.blurTimeout) {
      clearTimeout(this.blurTimeout);
    }

    this.blurTimeout = setTimeout(() => {
      this.hideSuggestions();
    }, 120);
  }

  onSearchKeyDown(event: KeyboardEvent): void {
    const hasSuggestions = this.showSuggestions && this.searchSuggestions.length > 0;

    if (event.key === 'Escape') {
      this.hideSuggestions();
      return;
    }

    if (event.key === 'ArrowDown' && hasSuggestions) {
      event.preventDefault();
      this.activeSuggestionIndex = (this.activeSuggestionIndex + 1) % this.searchSuggestions.length;
      return;
    }

    if (event.key === 'ArrowUp' && hasSuggestions) {
      event.preventDefault();
      this.activeSuggestionIndex =
        this.activeSuggestionIndex <= 0 ? this.searchSuggestions.length - 1 : this.activeSuggestionIndex - 1;
      return;
    }

    if (event.key === 'Enter') {
      event.preventDefault();

      if (hasSuggestions && this.activeSuggestionIndex >= 0) {
        this.selectSuggestion(this.searchSuggestions[this.activeSuggestionIndex]);
        return;
      }

      this.onSearch();
    }
  }

  selectSuggestion(suggestion: IProductSuggestion): void {
    this.shopParams.search = suggestion.name;
    this.onSearch();
  }

  private observeProductItems() {
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            entry.target.classList.add('fade-in');
          }
        });
      },
      {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
      }
    );

    const existingItems = document.querySelectorAll('.fade-item');
    existingItems.forEach(item => item.classList.remove('fade-in'));

    setTimeout(() => {
      const productItems = document.querySelectorAll('.fade-item');
      productItems.forEach(item => observer.observe(item));
    }, 50);
  }

  private setupSearchSuggestions(): void {
    this.searchInput$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((value) => {
          const term = value.trim();

          if (term.length < 2) {
            return of([] as IProductSuggestion[]);
          }

          const params = this._shopService.getShopParams();
          return this._shopService
            .getProductSuggestions(term, params.brandId, params.typeId, 8)
            .pipe(catchError(() => of([] as IProductSuggestion[])));
        }),
        takeUntil(this.destroy$)
      )
      .subscribe((suggestions) => {
        this.searchSuggestions = suggestions;
        this.activeSuggestionIndex = -1;
        this.showSuggestions = suggestions.length > 0;
      });
  }

  private hideSuggestions(): void {
    this.showSuggestions = false;
    this.activeSuggestionIndex = -1;
  }

  private clearSuggestions(): void {
    this.searchSuggestions = [];
    this.hideSuggestions();
  }

  private refreshSuggestionsForCurrentSearch(): void {
    if ((this.shopParams.search || '').trim().length >= 2) {
      this.searchInput$.next(this.shopParams.search || '');
    } else {
      this.clearSuggestions();
    }
  }
}
