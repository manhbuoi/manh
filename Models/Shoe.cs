using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cuahanggiay.Models
{
    public class Shoe
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên giày không được để trống")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal? OldPrice { get; set; }

        public string? ImageUrl { get; set; }

        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public int? BrandId { get; set; }
        [ForeignKey("BrandId")]
        public virtual Brand? Brand { get; set; }

        public bool IsFeatured { get; set; } = false;

        public int Rating { get; set; } = 5;

        [Range(0, int.MaxValue)]
        public int Tonkho { get; set; } = 0; 

        public int Daban { get; set; } = 0; 

   
        public Shoe()
        {
        }
    }
}