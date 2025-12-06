export interface IProductReview {
  id: number;
  userName: string;
  firstName: string;
  lastName: string;
  rating: number;
  profilePictureUrl?: string;
  comment?: string;
  headline?: string;
  createdAt: Date;
  helpfulCount?: number;
  notHelpfulCount?: number;
}

export interface IProductReviewFrom {
  productId: number;
  rating: number;
  comment?: string;
  headline: string;
}
