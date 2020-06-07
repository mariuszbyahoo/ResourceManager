using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ResourceManager.Data.Services
{
    public interface IEmailService
    {
        Task<IActionResult> NotifyByEmail(Guid receiverID, string receiverAddres, string subject, string message, IConfiguration _configuration);
    }
}
