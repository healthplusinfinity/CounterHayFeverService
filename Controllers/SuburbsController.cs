using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using HealthPlusInfinity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace HealthPlusInfinity.Controllers
{
    /// <summary>
    /// Class to handle data operations for suburbs.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SuburbsController : Controller
    {
        private readonly string connectionString;

        public SuburbsController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("CounterHayFeverConnection");
        }

        // GET api/suburbs
        /// <summary>
        /// Get a list of all suburbs across Victoria.
        /// </summary>
        /// <returns>A list of suburbs across Victoria.</returns>
        [HttpGet]
        public JsonResult Get()
        {
            var suburbs = new List<string>();
            string sqlQuery = @"SELECT SuburbName FROM Suburb";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        suburbs.Add(reader["SuburbName"].ToString());
                    }
                }
            }
            return new JsonResult(suburbs);
        }

        // GET /api/suburbs/melbourne
        /// <summary>
        /// Get data for the specified suburb.
        /// </summary>
        /// <returns>The tree count and coordinated of the suburb.</returns>
        /// <param name="suburb">Suburb name.</param>
        [HttpGet("{suburb}")]
        public JsonResult Get(string suburb)
        {
            SuburbLocation location = new SuburbLocation();
            string sqlString = @"SELECT SUM(TreeCount) AS Count
                                      , Suburb
                                      , CentroidLatitude
                                      , CentroidLongitude
                                FROM Analysis
                                WHERE Suburb = @suburb
                                GROUP BY Suburb, CentroidLatitude, CentroidLongitude";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlString, connection))
                {
                    command.Parameters.Add("@suburb", SqlDbType.VarChar).SqlValue = suburb;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        location.Suburb = suburb;
                        location.TreeCount = Convert.ToInt32(reader["Count"]);
                        location.Latitude = Convert.ToDouble(reader["CentroidLatitude"]);
                        location.Longitude = Convert.ToDouble(reader["CentroidLongitude"]);
                    }
                }
                return new JsonResult(location);
            }
        }
    }
}
