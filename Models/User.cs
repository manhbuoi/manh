using System.ComponentModel.DataAnnotations;

namespace cuahanggiay.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(255)] // Độ dài lớn để chứa chuỗi đã mã hóa BCrypt
        public string Password { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public string Role { get; set; } = "Customer"; // Phân quyền người dùng
    }
}