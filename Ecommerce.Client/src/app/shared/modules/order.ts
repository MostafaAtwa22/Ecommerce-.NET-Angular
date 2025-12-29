import { IAddress } from "./address"

export interface IOrderToCreate {
  basketId: string
  deliveryMethodId: number
  shipToAddress: IAddress
}

export interface IAllOrders {
  id: number;
  orderDate: string;
  status: string;
  total: number;
  firstName: string;
  lastName: string;
  profilePictureUrl?: string | null;
  createdAt: Date;
  buyerEmail: string;
}

export interface IOrder {
  id: number
  buyerEmail: string
  orderDate: string
  shippingPrice: number
  subTotal: number
  total: number
  status: string
  orderItems: IOrderItem[]
  addressToShip: IAddress
  deliveryMethod: string
}

export interface IOrderItem {
  productItemId: number
  productName: string
  pictureUrl: string
  price: number
  quantity: number
}

export interface IUpdateOrderStatusDto {
  status: string;
}
