using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HealthPlusInfinity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HealthPlusInfinity.Controllers
{
    /// <summary>
    /// Class to handle data operations for weather related data.
    /// </summary>
    [Route("api/[controller]")]
    public class WeatherController : Controller
    {
        /// <summary>
        /// OpenWeather API key to fetch weather data.
        /// </summary>
        private readonly string weatherAPIKey;

        /// <summary>
        /// Base URL for the Weather API endpoint.
        /// </summary>
        /// <value>The Weather API Base URL.</value>
        public string APIBaseURL { get; }

        /// <summary>
        /// Container for configuration
        /// </summary>
        /// <value>The configuration for the API service.</value>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// The database connection string.
        /// </summary>
        private readonly string connectionString;

        public WeatherController(IConfiguration configuration)
        {
            Configuration = configuration;
            weatherAPIKey = Configuration.GetSection("APIKeys")["WeatherAPIKey"];
            APIBaseURL = $"https://api.openweathermap.org/data/2.5/weather?appid={weatherAPIKey}";
            connectionString = Configuration.GetConnectionString("CounterHayFeverConnection");
        }

        // GET: api/weather?latitude=<lat>&longitude=<long>&suburb=<suburbName>
        /// <summary>
        /// Get weather data for the specified latitude, longitude and suburb.
        /// </summary>
        /// <returns>Weather data</returns>
        /// <param name="latitude">Latitude of the place.</param>
        /// <param name="longitude">Longitude of the place.</param>
        /// <param name="suburb">Suburb name.</param>
        [HttpGet]
        public JsonResult Get([FromQuery] double latitude, [FromQuery] double longitude,
            [FromQuery] string suburb)
        {
            string result = string.Empty;
            using (var httpClient = new HttpClient())
            {
                string url = APIBaseURL + $"&lat={latitude}&lon={longitude}&units=metric";
                Uri uri = new Uri(url);
                var cts = new CancellationToken();
                
                Task.Run(async () => {
                    try
                    {
                        var client = new HttpClient();
                        HttpResponseMessage response = await client.GetAsync(url, cts);
                        if (response.IsSuccessStatusCode)
                        {
                            result = await response.Content.ReadAsStringAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.ToString());
                    }
                }, cts).Wait();
            }

            JObject jsonObject = JObject.Parse(result);
            var weatherData = new WeatherModel
            {
                Temperature = (double) jsonObject.SelectToken("main.temp"),
                Pressure = (double) jsonObject.SelectToken("main.pressure"),
                Humidity = (double) jsonObject.SelectToken("main.humidity"),
                WindSpeed = 3.6 * (double) jsonObject.SelectToken("wind.speed")
            };

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlString = @"SELECT TreeCount
                                    FROM Analysis
                                    WHERE Suburb = @suburb";
                using (SqlCommand command = new SqlCommand(sqlString, connection))
                {
                    command.Parameters.Add("@suburb", SqlDbType.VarChar).SqlValue = suburb;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            weatherData.TreeCount = Convert.ToInt32(reader["TreeCount"]);
                        }
                    }
                }

                sqlString = @"SELECT COUNT(*) AS ConstructionCount
                            FROM Construction
                            WHERE SuburbID = (SELECT SuburbID 
                                                FROM Suburb 
                                                WHERE SuburbName = @suburb)";
                using (SqlCommand command = new SqlCommand(sqlString, connection))
                {
                    command.Parameters.Add("@suburb", SqlDbType.VarChar).SqlValue = suburb;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            weatherData.ConstructionCount = Convert.ToInt32(reader["ConstructionCount"]);
                        }
                    }
                }
            }

            weatherData.Score = GetScore(weatherData.Temperature, weatherData.Humidity
                , weatherData.Pressure, weatherData.WindSpeed
                , weatherData.TreeCount, weatherData.ConstructionCount);

            return new JsonResult(weatherData);
        }

        /// <summary>
        /// Get the weather score depending on the weather parameters.
        /// </summary>
        /// <returns>The weather score.</returns>
        /// <param name="temperature">Temperature in degree C.</param>
        /// <param name="humidity">Humidity in percentage.</param>
        /// <param name="pressure">Pressure in hPa.</param>
        /// <param name="windspeed">Windspeed in kmph.</param>
        /// <param name="trees">Tree population in the location.</param>
        private double GetScore(double temperature, double humidity, double pressure, 
            double windspeed, double trees, double construction)
        {
            IConfigurationSection coefficientSection = Configuration.GetSection("Coefficients");
            double t = Convert.ToDouble(coefficientSection["Temperature"]);
            double h = Convert.ToDouble(coefficientSection["Humidity"]);
            double p = Convert.ToDouble(coefficientSection["Pressure"]);
            double w = Convert.ToDouble(coefficientSection["Windspeed"]);
            double tr = Convert.ToDouble(coefficientSection["Trees"]);
            double cn = Convert.ToDouble(coefficientSection["Construction"]);

            return t * temperature + h * humidity + p * pressure + w * windspeed 
                + tr * trees + cn * construction;
        }
    }
}
