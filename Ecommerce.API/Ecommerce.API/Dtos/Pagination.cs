namespace Ecommerce.API.Dtos
{
    public class Pagination<T> where T : class
    { 
        public int PageIndex { get; set; } 
        public int PageSize { get; set; } 
        public int TotalData { get; set; } 
        public IReadOnlyList<T> Data { get; set; } 

        public Pagination(int pageIndex, int pageSize, int count, IReadOnlyList<T> data) 
        { 
            PageIndex = pageIndex;
            PageSize = pageSize; 
            TotalData = count; 
            Data = data; 
        } 
    }
}