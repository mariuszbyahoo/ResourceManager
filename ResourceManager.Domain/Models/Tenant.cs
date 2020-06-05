using ResourceManager.Domain.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Models.Implementations
{
    public class Tenant : ITenant
    {
        public Guid Id { get;} = Guid.NewGuid();

        public byte Priority { get; set; } = 0;
    }
}
