import { v4 as uuidv4 } from 'uuid';

export interface IWishList {
  id: string
  items: IWishListItem[]
}

export interface IWishListItem {
  id: number
  productName: string
  brand: string
  type: string
  pictureUrl: string
  price: number,
  quantity: number
}

export class WishList implements IWishList {
  id = uuidv4();
  items: IWishListItem[] = [];
}
