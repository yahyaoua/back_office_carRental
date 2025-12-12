using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CarRental2.Core.Interfaces
{
    public interface IStatsService
    {
        Task<int> GetTotalVehiclesAsync();
        Task<int> GetAvailableVehiclesCountAsync();
        Task<int> GetActiveReservationsCountAsync();
    }
}
