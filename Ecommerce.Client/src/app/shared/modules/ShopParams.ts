export class ShopParams {
  brandId?: number;
  typeId?: number;
  search?: string;
  sort: string = 'name';
  pageIndex: number = 1;
  pageSize: number = 12;
}
