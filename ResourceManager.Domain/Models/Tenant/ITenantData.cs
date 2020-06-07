using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Models
{
    public interface ITenantData
    {
        Guid Id { get; set; }
        string EmailAddress { get; set; }
    }
}
