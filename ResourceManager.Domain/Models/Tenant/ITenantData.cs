using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Models
{
    public interface ITenantData
    {
        string EmailAddress { get; set; }
    }
}
