import { Injectable } from '@angular/core';
import { Environment } from '../environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { IProductReview, IProductReviewFrom } from '../shared/modules/product-review';
import { Observable, of, tap } from 'rxjs';
import { Pagination } from '../shared/modules/pagination';
import { ReviewParams } from '../shared/modules/ReviewParams';

@Injectable({
  providedIn: 'root',
})
export class ProductReviewService {
  private baseUrl = `${Environment.baseUrl}/api/ProductReviews`;

  private reviewsCache = new Map<number, IProductReview[]>();
  reviewParams = new ReviewParams();
  pagination = new Pagination<IProductReview>();

  constructor(private http: HttpClient) {}

  getReviews(productId: number, useCache: boolean = true): Observable<Pagination<IProductReview>> {
    if (!useCache || !this.reviewsCache.has(productId)) {
      this.reviewsCache.set(productId, []);
    }

    const cachedReviews = this.reviewsCache.get(productId)!;
    const pagesReceived = Math.ceil(cachedReviews.length / this.reviewParams.pageSize);

    if (useCache && cachedReviews.length > 0 && this.reviewParams.pageIndex <= pagesReceived) {
      this.pagination.data = cachedReviews.slice(
        (this.reviewParams.pageIndex - 1) * this.reviewParams.pageSize,
        this.reviewParams.pageIndex * this.reviewParams.pageSize
      );
      this.pagination.pageIndex = this.reviewParams.pageIndex;
      this.pagination.pageSize = this.reviewParams.pageSize;
      this.pagination.totalData = cachedReviews.length;

      return of(this.pagination);
    }

    let params = new HttpParams()
      .set('pageIndex', this.reviewParams.pageIndex)
      .set('pageSize', this.reviewParams.pageSize);

    if (this.reviewParams.minRating != null) {
      params = params.set('minRating', this.reviewParams.minRating);
    }

    if (this.reviewParams.sort) {
      params = params.set('sort', this.reviewParams.sort);
    }

    return this.http.get<Pagination<IProductReview>>(`${this.baseUrl}/${productId}`, { params })
      .pipe(
        tap(response => {
          this.reviewsCache.set(productId, [...cachedReviews, ...response.data]);
          this.pagination = response;
        })
      );
  }

  createReview(review: IProductReviewFrom): Observable<IProductReview> {
    return this.http.post<IProductReview>(this.baseUrl, review)
      .pipe(
        tap(created => {
          const cached = this.reviewsCache.get(review.productId) || [];
          cached.unshift(created); // Add at beginning for newest first
          this.reviewsCache.set(review.productId, cached);
        })
      );
  }

  updateReview(id: number, review: IProductReviewFrom): Observable<IProductReview> {
    return this.http.put<IProductReview>(`${this.baseUrl}/${id}`, review)
      .pipe(
        tap(updated => {
          const cached = this.reviewsCache.get(review.productId);
          if (cached) {
            const index = cached.findIndex(r => r.id === updated.id);
            if (index !== -1) cached[index] = updated;
          }
        })
      );
  }

  deleteReview(id: number, productId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`)
      .pipe(
        tap(() => {
          const cached = this.reviewsCache.get(productId);
          if (cached) {
            this.reviewsCache.set(productId, cached.filter(r => r.id !== id));
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
    if (!cached || cached.length === 0) return 0;

    const sum = cached.reduce((acc, review) => acc + review.rating, 0);
    return Math.round((sum / cached.length) * 10) / 10;
  }
}
