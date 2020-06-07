using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Models
{
    public class TenantData : ITenantData
    {
        public string EmailAddress { get; set; }
    }
}
