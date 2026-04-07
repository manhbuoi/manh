using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace cuahanggiay.ViewModels
{
    // Thông tin 1 sản phẩm trong giỏ để hiển thị tóm tắt đơn hàng
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Size { get; set; } // Thêm size
        public decimal Total => Price * Quantity;
    }

    // Model tổng hợp cho trang Thanh toán (Checkout)
    public class CheckoutViewModel
    {
        // --- Phần thông tin khách hàng ---

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận.")]
        [StringLength(100, ErrorMessage = "Tên quá dài.")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        // Regex chuẩn cho các đầu số nhà mạng Việt Nam
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không hợp lệ (ví dụ: 0912345678).")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ Email")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
        public string? Email { get; set; }

        [Display(Name = "Địa chỉ nhận hàng")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng.")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        // --- Phần dữ liệu giỏ hàng ---

        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();

        public decimal CartTotal { get; set; }
    }
}