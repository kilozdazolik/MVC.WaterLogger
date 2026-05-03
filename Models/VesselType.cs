namespace kilozdazolik.WaterLogger.Models
{
    public class VesselType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CapacityMl { get; set; }
        public List<WaterLog> WaterLogs { get; set; } = new();
    }
}
