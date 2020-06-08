using Microsoft.AspNetCore.Mvc;
using ResourceManager.Data.Repos;
using ResourceManager.Data.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManager.Services
{
    public class RemoveService : IRemoveService
    {
        private ILoggerService _logger;

        public RemoveService(ILoggerService logger)
        {
            _logger = logger;
        }

        public async Task<bool> CheckDate(DateTime withdrawalDate)
        {
            var timesToCheck = withdrawalDate.Subtract(DateTime.Now).TotalHours / 4;
            timesToCheck = Math.Round(timesToCheck, MidpointRounding.AwayFromZero);
            for (int i = 0; i < timesToCheck; i++)
            {
                if (DateTime.Now.Date.Equals(withdrawalDate.Date))
                {
                    await _logger.LogToFile($"Log INFO entry: {DateTime.Now}:: Is the moment to withdraw a resource", "info.txt");
                    return true;
                }
                else
                {
                    await _logger.LogToFile($"Log INFO entry: {DateTime.Now}:: Checked the date, awaiting further", "info.txt");
                    Thread.Sleep(new TimeSpan(6, 0, 0)); // sprawdza co sześć godzin
                }
            }
            // Jeśli wyznaczono usunięcie na dziś dociera tutaj.
            return true;
        }
    }
}
