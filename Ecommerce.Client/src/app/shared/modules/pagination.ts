export interface IPagination<T> {
  pageIndex: number;
  pageSize: number;
  totalData: number;
  data: T[];
}

export class Pagination<T> implements IPagination<T> {
  pageIndex: number = 0;
  pageSize: number = 0;
  totalData: number = 0;
  data: T[] = [];
}
