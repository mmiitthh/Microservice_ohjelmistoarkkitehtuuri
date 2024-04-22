using DatabaseManagementService.Models;

namespace DatabaseManagementService.DTO
{
        public class ElectricityPriceDataDtoIn
        {
            public List<PriceInfo> Prices { get; set; }

        }

        public class PriceInfo
        {
            public double Price { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
}