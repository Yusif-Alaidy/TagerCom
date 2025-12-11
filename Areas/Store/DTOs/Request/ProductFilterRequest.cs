namespace TagerCom.Areas.Store.DTOs.Request
{
    public class ProductFilterRequest
    {
        public int?     maxPrice    { set; get; }
        public int?     minPrice    { set; get; }
        public string?  search      { set; get; }
        public int      page        { set; get; } = 1;
        public int      pageSize    { set; get; } = 20;
        public bool     isActive    { get; set; } = true;
        public bool     inStock     { set; get; } = true;
        public bool     sortByDate  { set; get; } = false;
        public bool     sortByPrice { set; get; } = false;
        public bool     sortBySales { set; get; } = false;
        public bool     sortByStock { set; get; } = false;
        public bool     descending  { set; get; } = false;
    }
}
