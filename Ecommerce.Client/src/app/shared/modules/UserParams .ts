export class UserParams {
  search?: string;
  role?: string;
  sort: string = 'name';
  pageIndex: number = 1;
  pageSize: number = 10;
}
