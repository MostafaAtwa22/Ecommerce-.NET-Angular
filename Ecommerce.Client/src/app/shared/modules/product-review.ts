export interface IProductReview {
  id: number;
  userName: string;
  firstName: string;
  lastName: string;
  rating: number;
  comment?: string;
  createdAt: Date;
}

export interface IProductReviewFrom {
  productId: number;
  rating: number;
  comment?: string;
}
