namespace TagerCom.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int? ParentId { get; set; }

        // Self Relation
        public Category? Parent { get; set; }
    }
}
