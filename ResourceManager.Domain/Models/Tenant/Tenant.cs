using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Models
{
    public class Tenant : ITenant
    {
        public Guid Id { get; set; }

        public byte Priority { get; set; }
    }
}
