export interface IPagination<T> {
  pageIndex: number
  pageSize: number
  totalData: number
  data: T[]
}
