using System.ComponentModel.DataAnnotations;

namespace cuahanggiay.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên không được dài quá 100 ký tự.")]
        [Display(Name = "Họ và tên")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tiêu đề là bắt buộc.")]
        [StringLength(150, ErrorMessage = "Tiêu đề không được dài quá 150 ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung là bắt buộc.")]
        [StringLength(2000, ErrorMessage = "Nội dung không được dài quá 2000 ký tự.")]
        [Display(Name = "Nội dung")]
        public string Message { get; set; } = string.Empty;
    }
}