import { Component, inject, Input } from '@angular/core';
import { IProductReview, IProductReviewFrom } from '../shared/modules/product-reviews';
import { ProductReviewService } from './product-review.service';
import { ToastrService } from 'ngx-toastr';
import { Subscription } from 'rxjs';
import { AccountService } from '../account/account-service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SweetAlertService } from '../shared/services/sweet-alert.service';

@Component({
  selector: 'app-product-review',
  imports: [CommonModule, FormsModule],
  templateUrl: './product-review.component.html',
  styleUrl: './product-review.component.scss',
})
export class ProductReviewComponent {
  private votingStates = new Map<number, boolean>();
  @Input() productId!: number;
  @Input() productName: string = '';

  reviews: IProductReview[] = [];
  loading = false;
  loadingMore = false;
  isSubmitting = false;
  currentUserReview: IProductReview | null = null;
  showReviewForm = false;
  hoverRating = 0;

  // Review form with headline
  newReview: IProductReviewFrom & { headline?: string } = {
    productId: 0,
    rating: 5,
    comment: '',
    headline: '',
  };

  // Form validation errors
  formErrors = {
    headline: '',
    comment: '',
  };

  // Filters and sort
  selectedRatingFilter: number | null = null;
  selectedSort = 'datedesc';

  // Pagination
  pagination: any = {};
  totalReviews = 0;
  pageSize = 10;
  currentPage = 1;

  private subscriptions = new Subscription();
  public accountService = inject(AccountService);

  constructor(
    private reviewService: ProductReviewService,
    private toastr: ToastrService,
    private sweetAlert: SweetAlertService
  ) {}

  ngOnInit(): void {
    this.loadReviews();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadReviews(loadMore = false): void {
    if (!this.productId) return;

    if (loadMore) {
      this.loadingMore = true;
    } else {
      this.loading = true;
      this.currentPage = 1;
    }

    this.newReview.productId = this.productId;

    // Clear cache when changing filters
    if (!loadMore) {
      this.reviewService.clearProductCache(this.productId);
      this.reviewService.resetReviewParams();
    }

    // Set params
    const params = this.reviewService.getReviewParams();
    params.pageIndex = this.currentPage;
    params.pageSize = this.pageSize;
    params.sort = this.selectedSort;
    params.minRating = this.selectedRatingFilter ?? 0;
    this.reviewService.setReviewParams(params);

    const sub = this.reviewService.getReviews(this.productId, !loadMore).subscribe({
      next: (response) => {
        if (loadMore) {
          this.reviews = [...this.reviews, ...response.data];
        } else {
          this.reviews = response.data;
        }

        this.pagination = response;
        this.totalReviews = response.totalData || 0;
        this.findCurrentUserReview();

        if (loadMore) {
          this.loadingMore = false;
          this.currentPage++;
        } else {
          this.loading = false;
        }
      },
      error: (error) => {
        console.error('Error loading reviews:', error);
        this.toastr.error('Failed to load reviews');
        this.loading = false;
        this.loadingMore = false;
      },
    });

    this.subscriptions.add(sub);
  }

  onFilterChange(): void {
    this.loadReviews();
  }

  onSortChange(): void {
    this.loadReviews();
  }

  findCurrentUserReview(): void {
    const currentUser = this.accountService.user();
    if (currentUser?.userName) {
      this.currentUserReview =
        this.reviews.find((review) => review.userName === currentUser.userName) || null;
    }
  }

  isCurrentUserReview(review: IProductReview): boolean {
    const currentUser = this.accountService.user();
    return currentUser?.userName === review.userName;
  }

  isCustomer(): boolean {
    const user = this.accountService.user();
    if (!user) return false;
    
    // Check if user has roles that should be excluded (Admin, SuperAdmin)
    const roles = user.roles || [];
    const isAdminOrSuper = roles.some(r => {
      const role = r.toLowerCase();
      return role === 'admin' || role === 'superadmin';
    });
    
    return !isAdminOrSuper;
  }

  getInitials(firstName: string, lastName: string): string {
    return `${firstName?.charAt(0) || ''}${lastName?.charAt(0) || ''}`.toUpperCase();
  }

  getRatingText(rating: number): string {
    const ratings = ['Poor', 'Fair', 'Average', 'Good', 'Excellent'];
    return ratings[rating - 1] || '';
  }

  getStarsArray(rating: number): string[] {
    const stars = [];
    const roundedRating = Math.round(rating);
    for (let i = 1; i <= 5; i++) {
      if (i <= roundedRating) {
        stars.push('fas fa-star active');
      } else {
        stars.push('far fa-star');
      }
    }
    return stars;
  }

  validateForm(): boolean {
    this.formErrors.headline = '';
    this.formErrors.comment = '';

    let isValid = true;

    // Validate headline
    if (!this.newReview.headline?.trim()) {
      this.formErrors.headline = 'Headline is required';
      isValid = false;
    } else if (this.newReview.headline.trim().length > 100) {
      this.formErrors.headline = 'Headline cannot exceed 100 characters';
      isValid = false;
    }

    // Validate comment
    if (!this.newReview.comment?.trim()) {
      this.formErrors.comment = 'Review comment is required';
      isValid = false;
    } else if (this.newReview.comment.trim().length > 3000) {
      this.formErrors.comment = 'Review cannot exceed 3000 characters';
      isValid = false;
    }

    // Validate rating
    if (this.newReview.rating < 1 || this.newReview.rating > 5) {
      isValid = false;
    }

    return isValid;
  }

  isReviewValid(): boolean {
    return (
      !!this.newReview.comment?.trim() &&
      !!this.newReview.headline?.trim() &&
      this.newReview.rating > 0
    );
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }

  getRelativeTime(date: Date): string {
    const now = new Date();
    const reviewDate = new Date(date);
    const diffMs = now.getTime() - reviewDate.getTime();
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

    if (diffDays === 0) return 'Today';
    if (diffDays === 1) return 'Yesterday';
    if (diffDays < 7) return `${diffDays} days ago`;
    if (diffDays < 30) return `${Math.floor(diffDays / 7)} weeks ago`;
    if (diffDays < 365) return `${Math.floor(diffDays / 30)} months ago`;
    return `${Math.floor(diffDays / 365)} years ago`;
  }

  setRating(rating: number): void {
    this.newReview.rating = rating;
  }

  markHelpful(review: IProductReview): void {
    if (this.isCurrentUserReview(review)) {
      this.toastr.error("You can't vote on your own review");
      return;
    }

    const sub = this.reviewService.markHelpful(review.id, this.productId).subscribe({
      next: () => {
        this.toastr.success('Marked as helpful');
        this.loadReviews();
      },
      error: (err) => {
        console.error(err);
        this.toastr.error(err.error?.message || 'Failed to submit feedback');
      },
    });

    this.subscriptions.add(sub);
  }

  markNotHelpful(review: IProductReview): void {
    if (this.isCurrentUserReview(review)) {
      this.toastr.error("You can't vote on your own review");
      return;
    }

    const sub = this.reviewService.markNotHelpful(review.id, this.productId).subscribe({
      next: () => {
        this.toastr.success('Marked as not helpful');
        this.loadReviews();
      },
      error: (err) => {
        console.error(err);
        this.toastr.error(err.error?.message || 'Failed to submit feedback');
      },
    });

    this.subscriptions.add(sub);
  }

  onSubmitReview(): void {
    if (!this.validateForm()) {
      this.toastr.error('Please fix the validation errors');
      return;
    }

    this.isSubmitting = true;

    const reviewToSubmit: IProductReviewFrom & { headline?: string } = {
      productId: this.productId,
      rating: this.newReview.rating,
      comment: this.newReview.comment!.trim(),
      headline: this.newReview.headline!.trim(),
    };

    const observable = this.currentUserReview
      ? this.reviewService.updateReview(this.currentUserReview.id, reviewToSubmit)
      : this.reviewService.createReview(reviewToSubmit);

    const sub = observable.subscribe({
      next: (review) => {
        this.toastr.success(this.currentUserReview ? 'Review updated!' : 'Review submitted!');
        this.resetForm();
        this.loadReviews();
        this.isSubmitting = false;
      },
      error: (error) => {
        console.error('Error:', error);
        this.toastr.error(error.message || 'Failed to submit review');
        this.isSubmitting = false;
      },
    });

    this.subscriptions.add(sub);
  }

  editReview(review: IProductReview): void {
    this.newReview.rating = review.rating;
    this.newReview.comment = review.comment || '';
    this.newReview.headline = (review as any).headline || '';
    this.showReviewForm = true;
  }

  deleteReview(review: IProductReview): void {
    this.sweetAlert
      .confirm({
        title: 'Delete Review',
        text: 'Are you sure you want to delete your review? This action cannot be undone.',
        confirmButtonText: 'Yes, delete it!',
      })
      .then((result) => {
        if (result.isConfirmed) {
          const sub = this.reviewService.deleteReview(review.id, this.productId).subscribe({
            next: () => {
              this.sweetAlert.success('Your review has been deleted successfully.');
              this.resetForm();
              this.loadReviews();
            },
            error: (error) => {
              console.error('Error deleting review:', error);
              this.sweetAlert.error('Failed to delete review. Please try again.');
            },
          });

          this.subscriptions.add(sub);
        }
      });
  }

  resetForm(): void {
    this.newReview = {
      productId: this.productId,
      rating: 5,
      comment: '',
      headline: '',
    };
    this.formErrors = {
      headline: '',
      comment: '',
    };
    this.showReviewForm = false;
  }

  toggleReviewForm(): void {
    this.showReviewForm = !this.showReviewForm;
    if (this.showReviewForm && this.currentUserReview) {
      this.newReview.rating = this.currentUserReview.rating;
      this.newReview.comment = this.currentUserReview.comment || '';
      this.newReview.headline = (this.currentUserReview as any).headline || '';
    } else if (!this.showReviewForm) {
      this.resetForm();
    }
  }

  getAverageRating(): number {
    if (this.reviews.length === 0) return 0;
    const sum = this.reviews.reduce((acc, review) => acc + review.rating, 0);
    return Math.round((sum / this.reviews.length) * 10) / 10;
  }

  getRatingDistribution(): any[] {
    const distribution = Array(5).fill(0);
    this.reviews.forEach((review) => {
      if (review.rating >= 1 && review.rating <= 5) {
        distribution[review.rating - 1]++;
      }
    });

    return distribution
      .map((count, index) => {
        const stars = index + 1;
        const percentage = this.reviews.length > 0 ? (count / this.reviews.length) * 100 : 0;
        return { stars, count, percentage };
      })
      .reverse(); // Show 5 stars first
  }

  loadMore(): void {
    if (this.pagination?.hasNextPage && !this.loadingMore) {
      this.loadReviews(true);
    }
  }
}
