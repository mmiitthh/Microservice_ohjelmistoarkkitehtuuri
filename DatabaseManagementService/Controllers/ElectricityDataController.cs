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
                return StatusCode(StatusCodes.Status500InternalServerError, "error clearing table");
            }
            return Ok("table cleared");
        }

        [Route("getprices")]
        [HttpGet]
        public async Task<IActionResult> get_prices([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            if (start == null || end == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "start and end parameters must be in DateTime format");
            }
            string log = "entries between " + start + " and " + end + "\n";
            try
            {
                var result = _context.ElectricityPrices
                    .Where(e => (e.StartDate > start && e.StartDate < end))
                    .OrderBy(e => e.StartDate).ToList();
                foreach (var a in result)
                {
                    log += a.StartDate + ":\t" + a.Price + "\n";
                }
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "error");
            }
        }

        [Route("getprices_page")]
        [HttpGet]
        public async Task<IActionResult> get_prices_page([FromQuery] DateTime? start, [FromQuery] DateTime? end,
    [FromQuery] int page, [FromQuery] int pageSize)
        {
            if (page < 1) { page = 1; }
            if (pageSize < 1) { pageSize = 1; }
            if (pageSize > 20) { pageSize = 20; }
            if (start == null || end == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "wrong format");
            }
            string log = "entries between " + start + " and " + end + "\n";
            try
            {
                var result = _context.ElectricityPrices
                    .Where(e => (e.StartDate > start && e.StartDate < end))
                    .OrderBy(e => e.StartDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize).ToList();

                foreach (var a in result)
                {
                    log += a.StartDate + ":\t" + a.Price + "\n";
                }
                log += "page: " + page + " pageSize: " + pageSize;
                _logger.LogInformation(log);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "error");
                return StatusCode(StatusCodes.Status500InternalServerError, "error");
            }
        }
        [Route("GetPriceDifference")]
        [HttpGet]
        public async Task<IActionResult> get_prices_dif([FromQuery] DateTime start, [FromQuery] DateTime end,
            [FromQuery] double fixedPrice)
        {
            if (start == null || end == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "wrong format");
            }
            string log = "price difference between " + start + " and " + end + "\tfixedPrice:" + fixedPrice + "\n";
            try
            {
                var result = _context.ElectricityPrices
                    .Where(e => (e.StartDate > start && e.StartDate < end))
                    .OrderBy(e => e.StartDate).ToList();
                double totalprice = 0;
                foreach (var a in result)
                {
                    totalprice += a.Price;
                }
                totalprice /= result.Count;
                log += "totalprice: " + totalprice + "\n";
                if (totalprice > fixedPrice)
                {
                    totalprice -= fixedPrice;
                }
                else
                {
                    fixedPrice -= totalprice;
                }
                _logger.LogInformation(log);
                return Ok(new PriceDifference(start, end, totalprice));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "error!");
                return StatusCode(StatusCodes.Status500InternalServerError, "error");
            }
        }
        [Route("GetPriceDifferenceList")]
        [HttpGet]
        public async Task<IActionResult> get_prices_dif_list([FromQuery] DateTime? start, [FromQuery] DateTime? end,
            [FromQuery] double fixedPrice)
        {
            if (start == null || end == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "wrong format");
            }
            string log = "price difference between " + start + " and " + end + "\tfixedPrice:" + fixedPrice + "\n";
            try
            {
                var result = _context.ElectricityPrices
                    .Where(e => (e.StartDate > start && e.StartDate < end))
                    .OrderBy(e => e.StartDate).ToList();
                List<PriceDifference> pd_list = new List<PriceDifference>();
                foreach (var a in result)
                {
                    double priceDifference;
                    if (a.Price > fixedPrice)
                    {
                        priceDifference = a.Price - fixedPrice;
                    }
                    else
                    {
                        priceDifference = fixedPrice - a.Price;
                    }

                    pd_list.Add(new PriceDifference(a.StartDate, a.EndDate, priceDifference));
                    log += a.StartDate + ":\t" + pd_list[pd_list.Count - 1].PriceDifferenceValue + "\n";
                }
                _logger.LogInformation(log);
                return Ok(pd_list);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "error!");
                return StatusCode(StatusCodes.Status500InternalServerError, "error");
            }
        }


    }
}
