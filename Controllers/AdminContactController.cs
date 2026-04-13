using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cuahanggiay.Data;

namespace cuahanggiay.Controllers
{
    public class AdminContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var contacts = await _context.Contacts.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(contacts);
        }
        
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa tin nhắn liên hệ.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
