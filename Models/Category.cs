using System.Collections.Generic;

namespace cuahanggiay.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public List<Shoe> Shoes { get; set; } = new();
    }
}