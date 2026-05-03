using kilozdazolik.WaterLogger.Models;
using Microsoft.EntityFrameworkCore;

namespace kilozdazolik.WaterLogger.Data
{
    public class WaterLoggerContext : DbContext
    {
        public WaterLoggerContext(DbContextOptions<WaterLoggerContext> options)
            : base(options) { }


        public DbSet<WaterLog> WaterLogs { get; set; }
        public DbSet<VesselType> VesselTypes { get; set; }
        public DbSet<DailyGoal> DailyGoals { get; set; }

        public static void SeedData(WaterLoggerContext context)
        {

            if (!context.VesselTypes.Any())
            {
                context.VesselTypes.AddRange(
                    new VesselType { Name = "Glass", CapacityMl = 250 },
                    new VesselType { Name = "Mug", CapacityMl = 350 },
                    new VesselType { Name = "Bottle", CapacityMl = 500 }
                );
                context.SaveChanges();
            }
        }
    }

}
