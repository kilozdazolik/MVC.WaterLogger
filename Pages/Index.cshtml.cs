using kilozdazolik.WaterLogger.Data;
using kilozdazolik.WaterLogger.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace kilozdazolik.WaterLogger.Pages
{
    public class IndexModel : PageModel
    {
        private readonly WaterLoggerContext _context;
        public List<VesselType> VesselTypes { get; set; } = new();
        public List<WaterLog> WaterLogs { get; set; }

        public IndexModel(WaterLoggerContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            VesselTypes = await _context.VesselTypes.Include(v => v.WaterLogs).ToListAsync();
            WaterLogs = await _context.WaterLogs.Include(w => w.VesselType).OrderByDescending(w => w.Timestamp).Where(t => t.Timestamp.Date == DateTime.Today).ToListAsync();
        }

        public async Task<IActionResult> OnPostAddAsync(int vesselId)
        {
            var log = new WaterLog
            {
                VesselTypeId = vesselId,
                Timestamp = DateTime.Now
            };
            _context.WaterLogs.Add(log);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}