using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Diagnostics;
using System.Text.RegularExpressions;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration) 
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetData(bool InjectionEnabled, String Oib)
        {
            if(!InjectionEnabled && (string.IsNullOrEmpty(Oib) || !Regex.IsMatch(Oib, @"^\d{11}$")))
            {
                TempData["Error"] = "Wrong input";
                return RedirectToAction("Index");
            }

            try
            {
                var connectionString = _configuration.GetConnectionString("connectionString");
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string sql;
                    if(InjectionEnabled)
                    {
                        sql = "SELECT * FROM \"user\" WHERE oib = \'" + Oib + "\'";
                    }
                    else
                    {
                        sql = "SELECT * FROM \"user\" WHERE oib = @oib";
                    }

                    using var command = new NpgsqlCommand(sql, connection);
                    if(!InjectionEnabled)
                    {
                        command.Parameters.AddWithValue("@oib", Oib);
                    }

                    using var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        TempData["FirstName"] += reader.GetString(reader.GetOrdinal("firstname"));
                        TempData["LastName"] += reader.GetString(reader.GetOrdinal("lastname"));
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction("Index");
            }
        }
    }
}