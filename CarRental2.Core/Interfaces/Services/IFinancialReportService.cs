using System;
using System.Collections.Generic;
using System.Text;
using CarRental2.Core.DTOs; 
using System;
using System.Threading.Tasks;

namespace CarRental2.Core.Interfaces.Services
{
    public interface IFinancialReportService
    {
        Task<FinancialSummaryDto> GetMonthlySummaryAsync(DateTime startDate, DateTime endDate);
    }
}
