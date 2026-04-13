using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using cuahanggiay.Data;

namespace cuahanggiay.Controllers
{
    [Authorize(Roles = "Shipper", AuthenticationSchemes = "CustomerSecurityScheme")]
    public class ShipperController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ShipperController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            
            int shipperId = int.Parse(userIdStr);
            
            var orders = await _db.Orders
                                  .Include(o => o.User)
                                  .Where(o => o.ShipperId == shipperId)
                                  .OrderByDescending(o => o.OrderDate)
                                  .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            int shipperId = int.Parse(userIdStr);

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.ShipperId == shipperId);
            if (order != null)
            {
                order.Status = status;
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Cập nhật trạng thái đơn hàng #{order.OrderCode} thành [{status}] thành công.";
            }

            return RedirectToAction("Index");
        }
    }
}
