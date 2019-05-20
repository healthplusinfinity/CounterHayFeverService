using System;
namespace HealthPlusInfinity.Models
{
    /// <summary>
    /// Model class to accept individual user ratings.
    /// </summary>
    public class IndividualRatingModel
    {
        public string Suburb { get; set; }
        public int Rating { get; set; }
    }
}
