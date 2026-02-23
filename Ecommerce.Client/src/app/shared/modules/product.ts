export interface IProduct {
  id: number
  name: string
  quantity: number
  boughtQuantity: number
  averageRating: number
  description: string
  pictureUrl: string
  price: number
  numberOfReviews: number
  productBrandName: string
  productTypeName: string
  productBrandId: number
  productTypeId: number
  createdAt: Date
}

export interface IProductSuggestion {
  id: number;
  name: string;
  productBrandName: string;
  productTypeName: string;
}

interface IProductForm {
  name: string;
  description: string;
  price: number;
  quantity: number;
  productTypeId: number;
  productBrandId: number;
}

export interface IProductCreate extends IProductForm {
  imageFile: File;
}

export interface IProductUpdate extends IProductForm  {
  productId: number;
  imageFile?: File | null;
}
