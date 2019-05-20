using System;
namespace HealthPlusInfinity.Models
{
    /// <summary>
    /// Model class to hold analyzed data as returned by the 
    /// web-service per suburb.
    /// </summary>
    public class SuburbLocation
    {
        public string Suburb { get; set; }
        public int TreeCount { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
