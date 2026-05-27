using kilozdazolik.WaterLogger.Data;
using kilozdazolik.WaterLogger.Models;
using kilozdazolik.WaterLogger.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace kilozdazolik.WaterLogger.Tests
{
    public class WaterLogServiceTests
    {
        private WaterLoggerContext GetInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<WaterLoggerContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new WaterLoggerContext(options);
        }

        [Fact]
        public async Task AddAsync_WithVesselId_AddsLogCorrectly()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var service = new WaterLogService(context, NullLogger<WaterLogService>.Instance);

            var vessel = new VesselType { Id = 1, Name = "Glass", CapacityMl = 250 };
            context.VesselTypes.Add(vessel);
            await context.SaveChangesAsync();

            // Act
            await service.AddAsync(vessel.Id, null);

            // Assert
            var log = await context.WaterLogs.FirstOrDefaultAsync();
            Assert.NotNull(log);
            Assert.Equal(vessel.Id, log.VesselTypeId);
            Assert.Equal(250, log.Volume);
        }

        [Fact]
        public async Task AddAsync_WithManualAmount_AddsLogCorrectly()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var service = new WaterLogService(context, NullLogger<WaterLogService>.Instance);

            // Act
            await service.AddAsync(null, 500);

            // Assert
            var log = await context.WaterLogs.FirstOrDefaultAsync();
            Assert.NotNull(log);
            Assert.Null(log.VesselTypeId);
            Assert.Equal(500, log.Volume);
        }

        [Fact]
        public async Task DeleteAsync_ExistingLog_RemovesLog()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var service = new WaterLogService(context, NullLogger<WaterLogService>.Instance);

            var log = new WaterLog { Id = 1, Volume = 300 };
            context.WaterLogs.Add(log);
            await context.SaveChangesAsync();

            // Act
            await service.DeleteAsync(1);

            // Assert
            var exists = await context.WaterLogs.AnyAsync();
            Assert.False(exists);
        }

        [Fact]
        public async Task EditAsync_UpdatesVolumeAndClearsVesselRef()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var service = new WaterLogService(context, NullLogger<WaterLogService>.Instance);

            var log = new WaterLog { Id = 1, VesselTypeId = 5, Volume = 300 };
            context.WaterLogs.Add(log);
            await context.SaveChangesAsync();

            // Act
            await service.EditAsync(1, 400);

            // Assert
            var updatedLog = await context.WaterLogs.FindAsync(1);
            Assert.NotNull(updatedLog);
            Assert.Equal(400, updatedLog.Volume);
            Assert.Null(updatedLog.VesselTypeId);
        }

        [Fact]
        public async Task SetGoalAsync_NewGoal_CreatesGoal()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var service = new WaterLogService(context, NullLogger<WaterLogService>.Instance);

            // Act
            await service.SetGoalAsync(2500);

            // Assert
            var goal = await context.DailyGoals.FirstOrDefaultAsync(g => g.Date == DateTime.Today);
            Assert.NotNull(goal);
            Assert.Equal(2500, goal.GoalMl);
        }

        [Fact]
        public async Task SetGoalAsync_ExistingGoal_UpdatesGoal()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var service = new WaterLogService(context, NullLogger<WaterLogService>.Instance);

            context.DailyGoals.Add(new DailyGoal { Date = DateTime.Today, GoalMl = 2000 });
            await context.SaveChangesAsync();

            // Act
            await service.SetGoalAsync(3000);

            // Assert
            var goal = await context.DailyGoals.FirstOrDefaultAsync(g => g.Date == DateTime.Today);
            Assert.NotNull(goal);
            Assert.Equal(3000, goal.GoalMl);
        }
    }
}
