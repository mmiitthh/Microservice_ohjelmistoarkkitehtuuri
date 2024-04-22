using DatabaseManagementService.Models;
using DatabaseManagementService.DTO;

namespace DatabaseManagementService.Extensions
{
    public static class mappingextension
    {
        public static ElectricityPrice ToEntity(this PriceInfo priceInfo)
        {
            return new ElectricityPrice
            {
                StartDate = priceInfo.StartDate,
                EndDate = priceInfo.EndDate,
                Price = priceInfo.Price
            };
        }
    }
}
