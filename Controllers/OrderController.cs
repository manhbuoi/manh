using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using cuahanggiay.Data;

namespace cuahanggiay.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Xem lịch sử đơn hàng của khách hàng
        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            // Kiểm tra đăng nhập
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem lịch sử đơn hàng!";
                return RedirectToAction("Login", "Account");
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdString);

            // Lấy danh sách đơn hàng của user này (Lấy luôn cả chi tiết OrderItems và thông tin Shoe)
            var orders = await _context.Orders
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Shoe) // Kéo theo thông tin bảng Giày để lấy Tên, Hình, Giá
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate) // Đơn mới nhất xếp lên đầu
                .ToListAsync();

            return View(orders);
        }
    }
}