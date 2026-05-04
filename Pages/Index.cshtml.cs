using kilozdazolik.WaterLogger.Data;
using kilozdazolik.WaterLogger.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace kilozdazolik.WaterLogger.Pages;

public class IndexModel : PageModel
{
    private readonly WaterLoggerContext _context;

    public List<VesselType> VesselTypes { get; set; } = new();
    public List<WaterLog> WaterLogs { get; set; } = new();
    public DailyGoal TodayGoal { get; set; } = new();

    public IndexModel(WaterLoggerContext context)
    {
        _context = context;
    }

    public async Task OnGetAsync()
    {
        VesselTypes = await _context.VesselTypes.ToListAsync();

        // Fetch logs for today with related vessel details
        WaterLogs = await _context.WaterLogs
            .Include(w => w.VesselType)
            .Where(t => t.Timestamp.Date == DateTime.Today)
            .OrderByDescending(w => w.Timestamp)
            .ToListAsync();

        // Fetch today's goal or use default
        var goal = await _context.DailyGoals
            .FirstOrDefaultAsync(g => g.Date.Date == DateTime.Today);

        TodayGoal = goal ?? new DailyGoal { GoalMl = 2000 };
    }


    public async Task<IActionResult> OnPostAddAsync(int? vesselId, int? manualAmount)
    {
        if (!vesselId.HasValue && (!manualAmount.HasValue || manualAmount.Value <= 0))
        {
            return RedirectToPage();
        }

        int finalVolume = 0;
        if (vesselId.HasValue)
        {
            var vessel = await _context.VesselTypes.FindAsync(vesselId.Value);
            if (vessel != null) finalVolume = vessel.CapacityMl;
        }
        else
        {
            finalVolume = manualAmount.Value;
        }

        _context.WaterLogs.Add(new WaterLog
        {
            VesselTypeId = vesselId,
            Volume = finalVolume,
            Timestamp = DateTime.Now
        });

        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var log = await _context.WaterLogs.FindAsync(id);
        if (log != null)
        {
            _context.WaterLogs.Remove(log);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, int newVolume)
    {
        if (newVolume <= 0) return RedirectToPage();

        var log = await _context.WaterLogs.FindAsync(id);
        if (log != null)
        {
            log.Volume = newVolume;
            log.VesselTypeId = null;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}