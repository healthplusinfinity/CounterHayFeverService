using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using HealthPlusInfinity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HealthPlusInfinity.Controllers
{
    /// <summary>
    /// Class to handle user ratings.
    /// </summary>
    [Route("api/[controller]")]
    public class UserRatingsController : Controller
    {
        private readonly string connectionString;

        public UserRatingsController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("CounterHayFeverConnection");
        }

        // GET api/userratings/caulfield
        /// <summary>
        /// Get the ratings for a specified suburb.
        /// </summary>
        /// <returns>The rating object for the suburb.</returns>
        /// <param name="suburb">Suburb name.</param>
        [HttpGet("{suburb}")]
        public JsonResult Get(string suburb)
        {
            var results = new UserRatingsModel();
            string sqlQuery = @"SELECT SuburbName
                                  , TotalRating
                                  , RatingCount
                                FROM Suburb
                                WHERE SuburbName = @suburb";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.Add("@suburb", SqlDbType.VarChar).SqlValue = suburb;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        results.Suburb = reader["SuburbName"].ToString();
                        results.TotalRating = (int)reader["TotalRating"];
                        results.RatingCount = (int)reader["RatingCount"];
                        if (results.RatingCount > 0)
                        {
                            results.AverageRating = (double)results.TotalRating / results.RatingCount;
                        }
                    }
                }
            }
            return new JsonResult(results);
        }

        // POST api/userratings
        /// <summary>
        /// Accept and record user rating for a particular suburb.
        /// </summary>
        /// <param name="model">Indivual rating model that has fields for suburb and rating.</param>
        [HttpPost]
        public void Post([FromBody]IndividualRatingModel model)
        {
            string sqlQuery = @"UPDATE Suburb
                                SET TotalRating = TotalRating + @rating, RatingCount = Suburb.RatingCount + 1
                                WHERE SuburbName = @suburb";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Transaction = transaction;
                        command.Parameters.Add("@suburb", SqlDbType.VarChar).SqlValue = model.Suburb;
                        command.Parameters.Add("@rating", SqlDbType.Int).SqlValue = model.Rating;
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }

            }
        }
    }
}
