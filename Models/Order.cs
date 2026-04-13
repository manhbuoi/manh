using System;
using System.Collections.Generic; // BẮT BUỘC PHẢI CÓ DÒNG NÀY
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cuahanggiay.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        // Mã đơn hàng (OrderCode)
        public string OrderCode { get; set; } = string.Empty;

        // Ngày đặt hàng (OrderDate)
        public DateTime OrderDate { get; set; } = DateTime.Now;

        // Tổng tiền (TotalAmount)
        public decimal TotalAmount { get; set; }

        // Trạng thái đơn hàng (Pending, Processing, Shipping, Completed, Cancelled)
        public string Status { get; set; } = "Pending";

        // Phương thức thanh toán ("Online" hoặc "Offline")
        public string PaymentMethod { get; set; } = "Offline";

        // Người giao hàng (Shipper) được chỉ định
        public int? ShipperId { get; set; }

        [ForeignKey("ShipperId")]
        public virtual User? Shipper { get; set; }

        // Tên người nhận (Đã có để khớp với giao diện)
        public string? ReceiverName { get; set; }

        // Số điện thoại người nhận
        public string? ReceiverPhone { get; set; }

        // Địa chỉ giao hàng
        public string? ShippingAddress { get; set; }

        // Ghi chú đơn hàng
        public string? Notes { get; set; }

        // KHÓA NGOẠI 1: Liên kết với tài khoản người dùng (User)
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        // ========================================================
        // ĐÃ THÊM: Liên kết 1 Đơn hàng -> Nhiều Chi tiết đơn hàng
        // ========================================================
        public virtual ICollection<OrderItem>? OrderItems { get; set; }
    }
}