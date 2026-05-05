using kilozdazolik.WaterLogger.Data;
using kilozdazolik.WaterLogger.Models;
using Microsoft.EntityFrameworkCore;

namespace kilozdazolik.WaterLogger.Services
{
    public interface IWaterLogService
    {
        Task AddAsync(int? vesselId, int? manualAmount);
        Task DeleteAsync(int id);
        Task EditAsync(int id, int newVolume);
        Task SetGoalAsync(int goalAmount);
        Task<(List<WaterLog> logs, List<VesselType> vesselTypes, DailyGoal todayGoal)> GetDashboardDataAsync();
    }

    public class WaterLogService : IWaterLogService
    {
        private readonly WaterLoggerContext _context;

        public WaterLogService(WaterLoggerContext context)
        {
            _context = context;
        }

        public async Task<(List<WaterLog> logs, List<VesselType> vesselTypes, DailyGoal todayGoal)> GetDashboardDataAsync()
        {
            var today = DateTime.Today;

            var logs = await _context.WaterLogs
                .Include(w => w.VesselType)
                .Where(w => w.Timestamp.Date == today)
                .OrderByDescending(w => w.Timestamp)
                .ToListAsync();

            var vesselTypes = await _context.VesselTypes
                .Where(v => v.Name != "Custom")
                .OrderBy(v => v.Name)
                .ToListAsync();

            var todayGoal = await _context.DailyGoals
                .FirstOrDefaultAsync(g => g.Date == today)
                ?? new DailyGoal { Date = today, GoalMl = 2000 };

            return (logs, vesselTypes, todayGoal);
        }

        public async Task AddAsync(int? vesselId, int? manualAmount)
        {
            int finalVolume = 0;

            if (vesselId.HasValue)
            {
                var vessel = await _context.VesselTypes.FindAsync(vesselId.Value);
                if (vessel != null)
                {
                    finalVolume = vessel.CapacityMl;
                }
            }
            else if (manualAmount.HasValue && manualAmount.Value > 0)
            {
                finalVolume = manualAmount.Value;
            }

            if (finalVolume > 0)
            {
                _context.WaterLogs.Add(new WaterLog
                {
                    Timestamp = DateTime.Now,
                    VesselTypeId = vesselId,
                    Volume = finalVolume
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var log = await _context.WaterLogs.FindAsync(id);
            if (log != null)
            {
                _context.WaterLogs.Remove(log);
                await _context.SaveChangesAsync();
            }
        }

        public async Task EditAsync(int id, int newVolume)
        {
            var log = await _context.WaterLogs.FindAsync(id);
            if (log != null)
            {
                log.Volume = newVolume;
                log.VesselTypeId = null; 
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetGoalAsync(int goalAmount)
        {
            var today = DateTime.Today;
            var currentGoal = await _context.DailyGoals.FirstOrDefaultAsync(g => g.Date == today);

            if (currentGoal != null)
            {
                currentGoal.GoalMl = goalAmount;
            }
            else
            {
                _context.DailyGoals.Add(new DailyGoal
                {
                    Date = today,
                    GoalMl = goalAmount
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}