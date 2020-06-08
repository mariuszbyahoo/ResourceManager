using ResourceManager.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ResourceManager.Domain.Models
{
    public class ResourceData : IResourceData
    {
        [Key]
        public Guid Id { get; set; }
        public ResourceStatus Availability { get; set; }
        public Guid LeasedTo { get; set; }
        public DateTime OccupiedTill { get; set; }
    }
}
