export class OrdersParams {
  search?: string;
  status?: string;
  sort: string = 'DateDesc';
  pageIndex: number = 1;
  pageSize: number = 10;
}
