using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Models
{
    public class Tenant : ITenant
    {
        public Guid Id { get;} = Guid.NewGuid();

        public byte Priority { get; set; } = 0;
    }
}
