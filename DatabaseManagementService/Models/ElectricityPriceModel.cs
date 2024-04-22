using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using DatabaseManagementService.DTO;

namespace DatabaseManagementService.Models
{
    public class ElectricityPrice
    {
        public Guid Id { get; set; }

        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }


        public ElectricityPrice()
        {
            Id = Guid.NewGuid();
        }
    }

}

