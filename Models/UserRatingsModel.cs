using System;
namespace HealthPlusInfinity.Models
{
    /// <summary>
    /// Model class to send requests for rating.
    /// </summary>
    public class UserRatingsModel
    {
        public string Suburb { get; set; }
        public int TotalRating { get; set; }
        public int RatingCount { get; set; }
        public double AverageRating { get; set; }
    }
}
