using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ResourceManager.Data.Services
{
    public interface ILoggerService
    {
        Task LogToFile(Exception? ex, string fileName);
        Task LogToFile(string message, string fileName);
    }
}
