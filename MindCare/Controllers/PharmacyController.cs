using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCare.Data;

namespace MindCare.Controllers
{
    public class PharmacyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PharmacyController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> PrescriptionList()
        {
            var prescriptions = await _context.Prescriptions.ToListAsync();
            return View(prescriptions);
        }

        public async Task<IActionResult> MedicationList(int prescriptionId)
        {
            var medications = await _context.Medications
                .Where(m => m.Prescriptions.Any(p => p.PrescriptionId == prescriptionId))
                .ToListAsync();
            return View(medications);
        }
        public IActionResult PrescribeMedication()
        {
            return View();
        }
        public IActionResult Prescriptions()
        {
            return View();
        }
    }
}
