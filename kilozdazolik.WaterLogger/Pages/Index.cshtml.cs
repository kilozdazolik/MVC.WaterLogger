using kilozdazolik.WaterLogger.Models;
using kilozdazolik.WaterLogger.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace kilozdazolik.WaterLogger.Pages;

public class IndexModel : PageModel
{
    private readonly IWaterLogService _waterLogService;
    public List<VesselType> VesselTypes { get; set; } = new();
    public List<WaterLog> WaterLogs { get; set; } = new();
    public DailyGoal TodayGoal { get; set; } = new();

    public IndexModel(IWaterLogService waterLogService)
    {
        _waterLogService = waterLogService;
    }

    public async Task OnGetAsync()
    {
        var data = await _waterLogService.GetDashboardDataAsync();
        WaterLogs = data.logs;
        VesselTypes = data.vesselTypes;
        TodayGoal = data.todayGoal;
    }

    public async Task<IActionResult> OnPostAddAsync(int? vesselId, int? manualAmount)
    {
        if (!vesselId.HasValue && (!manualAmount.HasValue || manualAmount <= 0))
        {
            ModelState.AddModelError("", "Please enter a valid amount.");
            await OnGetAsync();
            return Page();
        }

        await _waterLogService.AddAsync(vesselId, manualAmount);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _waterLogService.DeleteAsync(id);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, int newVolume)
    {
        await _waterLogService.EditAsync(id, newVolume);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSetGoalAsync(int goalAmount)
    {
        await _waterLogService.SetGoalAsync(goalAmount);
        return RedirectToPage();
    }
}