using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using cuahanggiay.Data;
using cuahanggiay.Models;
using cuahanggiay.ViewModels;
using cuahanggiay.Services;

namespace cuahanggiay.Controllers
{
    // BƯỚC 3: KHOÁ BẢO MẬT CẤP ĐỘ CONTROLLER
    // Chỉ những user có quyền Admin HOẶC StoreOwner mới được truy cập các Action bên trong
    // AuthenticationSchemes phải khớp với tên scheme bạn đã định nghĩa trong Program.cs
    [Authorize(Roles = "Admin,StoreOwner", AuthenticationSchemes = "CustomerSecurityScheme")]
    public class AdminOrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;

        public AdminOrderController(ApplicationDbContext db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        // ==========================================
        // 0. BẢNG ĐIỀU KHIỂN & THỐNG KÊ (DASHBOARD)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var model = new DashboardViewModel();

            // 1. Thống kê cơ bản
            model.TotalOrders = await _db.Orders.CountAsync();
            model.TotalCustomers = await _db.Users.CountAsync(u => u.Role == "Customer");
            model.TotalProducts = await _db.Shoes.CountAsync();

            // Tính doanh thu từ đơn hàng Đã xác nhận hoặc Đã giao hàng
            model.TotalRevenue = await _db.Orders
                .Where(o => o.Status == "Đã xác nhận" || o.Status == "Đã giao hàng")
                .SumAsync(o => o.TotalAmount);

            // 2. Lấy 5 đơn hàng mới nhất
            model.RecentOrders = await _db.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            // 3. Tính toán Doanh thu 7 ngày gần nhất
            DateTime today = DateTime.Today;
            DateTime startDate = today.AddDays(-6);
            
            var ordersInLast7Days = await _db.Orders
                .Where(o => o.OrderDate >= startDate && (o.Status == "Đã xác nhận" || o.Status == "Đã giao hàng"))
                .ToListAsync();

            for (int i = 0; i < 7; i++)
            {
                DateTime date = startDate.AddDays(i);
                model.ChartLabels.Add(date.ToString("dd/MM"));
                
                decimal revenueForDay = ordersInLast7Days
                    .Where(o => o.OrderDate.Date == date)
                    .Sum(o => o.TotalAmount);
                    
                model.ChartData.Add(revenueForDay);
            }

            return View(model);
        }

        // ==========================================
        // 1. QUẢN LÝ SẢN PHẨM (THÊM, SỬA, XÓA, XEM)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Shoes()
        {
            var shoes = await _db.Shoes.Include(s => s.Category).ToListAsync();
            return View(shoes);
        }

        [HttpGet]
        public IActionResult CreateShoe()
        {
            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateShoe(Shoe shoe)
        {
            // Bỏ qua lỗi xác thực ảo khi thêm mới
            ModelState.Remove("Category");
            ModelState.Remove("Brand");

            if (ModelState.IsValid)
            {
                _db.Shoes.Add(shoe);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Shoes");
            }
            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", shoe.CategoryId);
            return View(shoe);
        }

        [HttpGet]
        public async Task<IActionResult> EditShoe(int id)
        {
            var shoe = await _db.Shoes.FindAsync(id);
            if (shoe == null) return NotFound();

            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", shoe.CategoryId);
            return View(shoe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditShoe(Shoe shoe, IFormFile? ImageFile)
        {
            // Bỏ qua lỗi xác thực liên quan đến Category/Brand khi chỉnh sửa
            ModelState.Remove("Category");
            ModelState.Remove("Brand");
            ModelState.Remove("ImageUrl"); // Cho phép ImageUrl có thể null nếu không cập nhật ảnh mới

            if (ModelState.IsValid)
            {
                try
                {
                    // NẾU CÓ ẢNH MỚI ĐƯỢC UPLOAD
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                        string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        string filePath = Path.Combine(uploadDir, fileName);

                        if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }

                        // Cập nhật thuộc tính ImageUrl cho object shoe
                        shoe.ImageUrl = "/images/" + fileName;
                    }

                    _db.Update(shoe);
                    await _db.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction("Shoes"); 
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu ảnh: " + ex.Message);
                }
            }

            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", shoe.CategoryId);
            return View(shoe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteShoe(int id)
        {
            var shoe = await _db.Shoes.FindAsync(id);
            if (shoe != null)
            {
                _db.Shoes.Remove(shoe);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sản phẩm!";
            }
            return RedirectToAction("Shoes");
        }

        // ==========================================
        // 2. KIỂM TRA & QUẢN LÝ ĐƠN HÀNG
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var orders = await _db.Orders
                                  .Include(o => o.User)
                                  .Include(o => o.Shipper)
                                  .OrderByDescending(o => o.OrderDate)
                                  .ToListAsync();
            
            ViewBag.Shippers = await _db.Users.Where(u => u.Role == "Shipper").ToListAsync();
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Shoe)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null)
            {
                order.Status = status;
                await _db.SaveChangesAsync();

                if (status == "Đã xác nhận" && order.User != null && !string.IsNullOrEmpty(order.User.Email))
                {
                    string htmlContent = GenerateInvoiceHtml(order);
                    await _emailService.SendEmailAsync(order.User.Email, $"Xác nhận đơn hàng #{order.OrderCode} - Vua Giày Hiệu", htmlContent);
                }

                TempData["SuccessMessage"] = $"Đã cập nhật đơn hàng #{order.OrderCode} thành {status}";
            }
            return RedirectToAction("Orders");
        }

        private string GenerateInvoiceHtml(Order order)
        {
            var receiverName = !string.IsNullOrEmpty(order.ReceiverName) ? order.ReceiverName : order.User?.FullName;
            var tableRows = "";
            
            if (order.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {
                    tableRows += $@"
                        <tr>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd;'>
                                <strong>{item.Shoe?.Name}</strong>
                                {(!string.IsNullOrEmpty(item.Size) ? $"<br><small style='color: #666;'>Kích cỡ (Size): {item.Size}</small>" : "")}
                            </td>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: right;'>{item.UnitPrice.ToString("#,##0")} đ</td>
                            <td style='padding: 10px; border-bottom: 1px solid #ddd; text-align: right;'>{item.TotalPrice.ToString("#,##0")} đ</td>
                        </tr>";
                }
            }

            return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e9ecef; border-radius: 10px;'>
                <div style='text-align: center; margin-bottom: 20px;'>
                    <h2 style='color: #dc3545;'>Vua Giày Hiệu</h2>
                    <p style='color: #6c757d; font-size: 14px;'>Hóa Đơn Đặt Hàng</p>
                </div>
                
                <div style='margin-bottom: 20px; padding: 15px; background-color: #f8f9fa; border-radius: 5px;'>
                    <h4 style='margin-top: 0; color: #343a40;'>Thông tin nhận hàng #{order.OrderCode}</h4>
                    <p style='margin: 5px 0;'><strong>Khách hàng:</strong> {receiverName}</p>
                    <p style='margin: 5px 0;'><strong>SĐT:</strong> {order.ReceiverPhone}</p>
                    <p style='margin: 5px 0;'><strong>Địa chỉ:</strong> {order.ShippingAddress}</p>
                    <p style='margin: 5px 0;'><strong>Ngày đặt:</strong> {order.OrderDate:dd/MM/yyyy HH:mm}</p>
                    <p style='margin: 5px 0;'><strong>Phương thức thanh toán:</strong> {order.PaymentMethod}</p>
                    {(!string.IsNullOrEmpty(order.Notes) ? $"<p style='margin: 5px 0;'><strong>Ghi chú:</strong> {order.Notes}</p>" : "")}
                </div>

                <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                    <thead>
                        <tr style='background-color: #f8f9fa;'>
                            <th style='padding: 10px; text-align: left; border-bottom: 2px solid #ddd;'>Sản phẩm</th>
                            <th style='padding: 10px; text-align: center; border-bottom: 2px solid #ddd;'>SL</th>
                            <th style='padding: 10px; text-align: right; border-bottom: 2px solid #ddd;'>Đơn Giá</th>
                            <th style='padding: 10px; text-align: right; border-bottom: 2px solid #ddd;'>Thành Tiền</th>
                        </tr>
                    </thead>
                    <tbody>
                        {tableRows}
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan='3' style='padding: 15px 10px 10px; text-align: right; font-weight: bold; border-top: 2px solid #ddd; font-size: 16px;'>TỔNG THANH TOÁN:</td>
                            <td style='padding: 15px 10px 10px; text-align: right; font-weight: bold; color: #dc3545; border-top: 2px solid #ddd; font-size: 16px;'>{order.TotalAmount.ToString("#,##0")} đ</td>
                        </tr>
                    </tfoot>
                </table>

                <div style='text-align: center; margin-top: 30px; color: #6c757d; font-size: 12px;'>
                    <p>Cảm ơn bạn đã tin tưởng và mua sắm tại Vua Giày Hiệu!</p>
                    <p>Mọi thắc mắc vui lòng liên hệ Hotline: 0987 XXX XXX</p>
                </div>
            </div>";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignShipper(int orderId, int shipperId)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.ShipperId = shipperId;
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã giao đơn hàng #{order.OrderCode} cho nhân viên giao hàng.";
            }
            return RedirectToAction("Orders");
        }

        // ==========================================
        // 3. QUẢN LÝ TÀI KHOẢN & PHÂN QUYỀN
        // ==========================================
        // BƯỚC 3.1: Ghi đè quyền riêng biệt cho Action này (CHỈ ADMIN)
        [HttpGet]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> Users()
        {
            var users = await _db.Users.ToListAsync();
            return View(users);
        }

        // BƯỚC 3.2: Ghi đè quyền riêng biệt cho Action này (CHỈ ADMIN)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole(int id, string role)
        {
            var user = await _db.Users.FindAsync(id);
            if (user != null)
            {
                user.Role = role;
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cấp quyền [{role}] thành công cho tài khoản: {user.FullName ?? user.Email}!";
            }
            return RedirectToAction("Users");
        }
    }
}