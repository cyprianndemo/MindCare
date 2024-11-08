using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;
using MindCare.Models;

namespace MindCare.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cartItems = await _context.CartItems.ToListAsync();
            return View(cartItems);
        }

        public async Task<IActionResult> AddToCart(int medicationId)
        {
            var medication = await _context.Medications.FindAsync(medicationId);
            if (medication != null)
            {
                var cartItem = new CartItem { MedicationId = medicationId, Quantity = 1 };
                _context.CartItems.Add(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
