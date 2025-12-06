import { Injectable } from '@angular/core';
import { Environment } from '../environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of, tap } from 'rxjs';
import { Pagination } from '../shared/modules/pagination';
import { IProductReview, IProductReviewFrom } from '../shared/modules/product-reviews';
import { ReviewParams } from '../shared/modules/ReviewParams';

@Injectable({
  providedIn: 'root',
})
export class ProductReviewService {
  private baseUrl = `${Environment.baseUrl}/api/ProductReviews`;

  private reviewsCache = new Map<number, IProductReview[]>();

  // Required fields (strict mode)
  reviewParams: ReviewParams = new ReviewParams();
  pagination: Pagination<IProductReview> = new Pagination<IProductReview>();

  constructor(private http: HttpClient) {}

  getReviews(productId: number, useCache: boolean = true): Observable<Pagination<IProductReview>> {
    if (!useCache || !this.reviewsCache.has(productId)) {
      this.reviewsCache.set(productId, []);
    }

    const cached = this.reviewsCache.get(productId)!;

    const pagesReceived = Math.ceil(cached.length / this.reviewParams.pageSize);

    // Return data from cache
    if (useCache && cached.length > 0 && this.reviewParams.pageIndex <= pagesReceived) {
      const start = (this.reviewParams.pageIndex - 1) * this.reviewParams.pageSize;
      const end = this.reviewParams.pageIndex * this.reviewParams.pageSize;

      this.pagination.data = cached.slice(start, end);
      this.pagination.pageIndex = this.reviewParams.pageIndex;
      this.pagination.pageSize = this.reviewParams.pageSize;
      this.pagination.totalData = cached.length;

      return of(this.pagination);
    }

    // Fetch from API
    let params = new HttpParams()
      .set('pageIndex', this.reviewParams.pageIndex)
      .set('pageSize', this.reviewParams.pageSize);

    if (this.reviewParams.minRating != null) {
      params = params.set('minRating', this.reviewParams.minRating);
    }

    if (this.reviewParams.sort) {
      params = params.set('sort', this.reviewParams.sort);
    }

    return this.http
      .get<Pagination<IProductReview>>(`${this.baseUrl}/${productId}`, { params })
      .pipe(
        tap((response) => {
          const merged = [...cached, ...response.data];
          this.reviewsCache.set(productId, merged);
          this.pagination = response;
        })
      );
  }

  createReview(review: IProductReviewFrom): Observable<IProductReview> {
    return this.http.post<IProductReview>(this.baseUrl, review).pipe(
      tap((created) => {
        const cached = this.reviewsCache.get(review.productId) ?? [];
        this.reviewsCache.set(review.productId, [created, ...cached]);
      })
    );
  }

  updateReview(id: number, review: IProductReviewFrom): Observable<IProductReview> {
    return this.http.put<IProductReview>(`${this.baseUrl}/${id}`, review).pipe(
      tap((updated) => {
        const cached = this.reviewsCache.get(review.productId);
        if (!cached) return;

        const index = cached.findIndex((r) => r.id === updated.id);
        if (index !== -1) cached[index] = updated;

        this.reviewsCache.set(review.productId, [...cached]);
      })
    );
  }

  deleteReview(id: number, productId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      tap(() => {
        const cached = this.reviewsCache.get(productId);
        if (!cached) return;

        this.reviewsCache.set(
          productId,
          cached.filter((r) => r.id !== id)
        );
      })
    );
  }

  markHelpful(reviewId: number, productId: number): Observable<IProductReview> {
    return this.http.post<IProductReview>(`${this.baseUrl}/${reviewId}/helpful`, {}).pipe(
      tap((updated) => {
        const cached = this.reviewsCache.get(productId);
        if (!cached) return;

        const index = cached.findIndex((r) => r.id === updated.id);
        if (index !== -1) {
          cached[index] = updated;
          this.reviewsCache.set(productId, [...cached]);
        }
      })
    );
  }

  markNotHelpful(reviewId: number, productId: number): Observable<IProductReview> {
    return this.http.post<IProductReview>(`${this.baseUrl}/${reviewId}/not-helpful`, {}).pipe(
      tap((updated) => {
        const cached = this.reviewsCache.get(productId);
        if (!cached) return;

        const index = cached.findIndex((r) => r.id === updated.id);
        if (index !== -1) {
          cached[index] = updated;
          this.reviewsCache.set(productId, [...cached]);
        }
      })
    );
  }

  setReviewParams(params: ReviewParams) {
    this.reviewParams = params;
  }

  getReviewParams() {
    return this.reviewParams;
  }

  resetReviewParams() {
    this.reviewParams = new ReviewParams();
    return this.reviewParams;
  }

  clearProductCache(productId: number) {
    this.reviewsCache.delete(productId);
  }

  getAverageRating(productId: number): number {
    const cached = this.reviewsCache.get(productId);
    if (!cached?.length) return 0;

    const sum = cached.reduce((acc, r) => acc + r.rating, 0);
    return Math.round((sum / cached.length) * 10) / 10;
  }
}
