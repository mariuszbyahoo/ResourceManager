using ResourceManager.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Models
{
    public class Resource : IResource
    {
        public Guid Id { get; set; }
        public string Variant { get; set; }
        public ResourceStatus Availability { get; set; } = ResourceStatus.Available;
        public Guid LeasedTo { get; set; } = Guid.Empty;

        public DateTime OccupiedTill { get; set; } = DateTime.MinValue;
    }
}
