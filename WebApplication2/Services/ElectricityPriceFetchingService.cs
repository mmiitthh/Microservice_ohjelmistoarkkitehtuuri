﻿using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace WebApplication2.Services
{
    public class ElectricityPriceFetchingService : BackgroundService
    {
        private readonly ILogger<ElectricityPriceFetchingService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ElectricityPriceFetchingService(ILogger<ElectricityPriceFetchingService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await FetchElectricityPricesAsync(stoppingToken);
                //Wait 1 hour to fetch again
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task FetchElectricityPricesAsync(CancellationToken stoppingToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            try
            {
                var response = await httpClient.GetAsync(Constants.Constants.PorssisahkoUrl, stoppingToken);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Hinnat haettu: {content}");


                // Tässä kohtaa voitaisiin välittää data toiselle palvelulle tai tallentaa se
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Virhe sähkön hintatietojen haussa");
            }
        }

        private async Task SendDataToDatabaseService(string data, CancellationToken stoppingToken)
        {
            var httpClient = _httpClientFactory.CreateClient("DatabaseServiceClient");
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            try
            {
                var response = await httpClient.PostAsync("api/ElectricityData", content, stoppingToken);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Data successfully sent to the database service.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send data to database service");
            }
        }


    }
}
