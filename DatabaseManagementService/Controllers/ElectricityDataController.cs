using DatabaseManagementService.Context;
using DatabaseManagementService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using DatabaseManagementService.Extensions;
using DatabaseManagementService.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DatabaseManagementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ElectricityDataController : ControllerBase
    {
        private readonly ElectricityDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private ILogger<ElectricityDataController> _logger;

        public ElectricityDataController(ElectricityDbContext context, ILogger<ElectricityDataController> logger,IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public async Task<IActionResult> PostElectricityData([FromBody] List<ElectricityPrice> prices)
        {
            if (prices == null || !prices.Any())
            {
                return BadRequest("No prices provided.");
            }

            _context.ElectricityPrices.AddRange(prices);
            await _context.SaveChangesAsync();
            return Ok(prices.Count);

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ElectricityPrice>>> GetElectricityPrices()
        {
            return await _context.ElectricityPrices.ToListAsync();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutElectricityPrice(Guid id, ElectricityPrice electricityPrice)
        {
            if (id != electricityPrice.Id)
            {
                return BadRequest();
            }

            _context.Entry(electricityPrice).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteElectricityPrice(Guid id)
        {
            var electricityPrice = await _context.ElectricityPrices.FindAsync(id);
            if (electricityPrice == null)
            {
                return NotFound();
            }

            _context.ElectricityPrices.Remove(electricityPrice);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Route("getDataBase")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ElectricityPrice>>> FetchElectricityPrices()
        {
            var httpClient = _httpClientFactory.CreateClient();
            string content = null;

            try
            {
                var response = await httpClient.GetAsync(Constants.Constants.ElectrictyDataUrl);
                response.EnsureSuccessStatusCode();

                content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("höhö");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PASKA");
            }

            try
            {
                _logger.LogInformation(content);
                string ret = await save_to_db(content);
                return Ok(ret);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving");
                return StatusCode(StatusCodes.Status500InternalServerError, "Virhe");
            }
        }

            private async Task<string> save_to_db(string data)
            {
            ElectricityPriceDataDtoIn data2 = JsonConvert.DeserializeObject<ElectricityPriceDataDtoIn>(data);
                _logger.LogInformation(data);

            int counter = 0;
                foreach (var i in data2.Prices)
                {
                bool exists = _context.ElectricityPrices.Any(e => e.StartDate == i.StartDate);
                if (!exists)
                {
                    _context.ElectricityPrices.Add(i.ToEntity());
                    counter++;
                }
                    
                }

                _logger.LogInformation($"Electricity price data: {counter}");

                await _context.SaveChangesAsync();

                return $"{data2.Prices.Count} entries fetched, not adding duplicates: {counter} new entries added";
            }

        [Route("cleartable")]
        [HttpGet]
        public async Task<IActionResult> clear_table()
        {
            try
            {
                var linq = _context.ElectricityPrices.ToList();
                _context.ElectricityPrices.RemoveRange(linq);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "error clearing table");
                return StatusCode(StatusCodes.Status500InternalServerError, "error clearing table");
            }
            _logger.LogInformation("table cleared");
            return Ok("table cleared");
        }
        //return await _context.ElectricityPrices.ToListAsync

    }
}
