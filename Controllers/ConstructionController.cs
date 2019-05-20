using System;
using System.Collections.Generic;
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
    /// Class to handle data operations for construction sites.
    /// </summary>
    [Route("api/[controller]")]
    public class ConstructionController : Controller
    {
        private readonly string connectionString;

        public ConstructionController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("CounterHayFeverConnection");
        }

        // GET: api/controller
        /// <summary>
        /// Get a list of construction sites across Victoria.
        /// </summary>
        /// <returns>A list of construction sites along with their locations.</returns>
        [HttpGet]
        public JsonResult Get()
        {
            var results = new List<ConstructionModel>();
            string sqlQuery = @"SELECT C.ConstructionLatitude
                                , C.ConstructionLongitude
                                , S.SuburbName
                            FROM Construction C
                                INNER JOIN Suburb S ON C.SuburbID = S.SuburbID";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        results.Add(new ConstructionModel
                        {
                            ConstructionLatitude = Convert.ToDouble(reader["ConstructionLatitude"]),
                            ConstructionLongitude = Convert.ToDouble(reader["ConstructionLongitude"]),
                            ConstructionSuburb = reader["SuburbName"].ToString()
                        });
                    }
                }
            }
            return new JsonResult(results);
        }
    }
}
