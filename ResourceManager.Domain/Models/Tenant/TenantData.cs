using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ResourceManager.Domain.Models
{
    public class TenantData : ITenantData
    {
        [Key]
        public Guid Id { get; set; }
        public string EmailAddress { get; set; }
    }
}
