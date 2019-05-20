using System;
namespace HealthPlusInfinity.Models
{
    public class WeatherModel
    {
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
        public double WindSpeed { get; set; }
        public double Score { get; set; }
        public int TreeCount { get; set; }
        public int ConstructionCount { get; set; }
    }
}
