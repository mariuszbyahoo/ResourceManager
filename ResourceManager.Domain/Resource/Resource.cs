using ResourceManager.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Implementations
{
    public class Resource : IResource
    {
        public Guid Id { get;} = Guid.NewGuid();
        public string Variant { get; set; } = "N/A";
        public ResourceStatus Availability { get; set; } = ResourceStatus.Available;
        public Guid LeasedTo { get; set; } = Guid.Empty;
    }
}
