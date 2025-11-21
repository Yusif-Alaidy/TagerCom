namespace TagerCom.Models
{
    public class Category
    {
        public Guid     Id          { get; set; } = Guid.NewGuid();
        public string   Name        { get; set; } = null!;
        public Guid?    ParentId    { get; set; }

        // Navigation
        public Category? Parent { get; set; }
    }
}
