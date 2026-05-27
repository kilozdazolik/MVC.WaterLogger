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
        private readonly ILogger<WaterLogService> _logger;

        public WaterLogService(WaterLoggerContext context, ILogger<WaterLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(List<WaterLog> logs, List<VesselType> vesselTypes, DailyGoal todayGoal)> GetDashboardDataAsync()
        {
            try
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
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "A database error occurred while retrieving dashboard data for date {Date}.", DateTime.Today);
                return (new List<WaterLog>(), new List<VesselType>(), new DailyGoal { Date = DateTime.Today, GoalMl = 2000 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving dashboard data.");
                return (new List<WaterLog>(), new List<VesselType>(), new DailyGoal { Date = DateTime.Today, GoalMl = 2000 });
            }
        }

        public async Task AddAsync(int? vesselId, int? manualAmount)
        {
            try
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
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "A database error occurred while adding a water log for vessel ID {VesselId} and manual amount {ManualAmount}.", vesselId, manualAmount);
                throw; // rethrow after logging so caller knows it failed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding a water log.");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var log = await _context.WaterLogs.FindAsync(id);
                if (log != null)
                {
                    _context.WaterLogs.Remove(log);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "A database error occurred while deleting water log with ID {Id}.", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting water log with ID {Id}.", id);
                throw;
            }
        }

        public async Task EditAsync(int id, int newVolume)
        {
            try
            {
                var log = await _context.WaterLogs.FindAsync(id);
                if (log != null)
                {
                    log.Volume = newVolume;
                    log.VesselTypeId = null; 
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "A database error occurred while editing water log with ID {Id} to new volume {NewVolume}.", id, newVolume);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while editing water log with ID {Id}.", id);
                throw;
            }
        }

        public async Task SetGoalAsync(int goalAmount)
        {
            try
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
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "A database error occurred while setting the daily goal to {GoalAmount} ml.", goalAmount);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while setting the daily goal.");
                throw;
            }
        }
    }
}