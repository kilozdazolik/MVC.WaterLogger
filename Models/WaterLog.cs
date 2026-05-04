namespace kilozdazolik.WaterLogger.Models;

public class WaterLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public int? VesselTypeId { get; set; }
    public VesselType? VesselType { get; set; }
    public int Volume { get; set; }
}