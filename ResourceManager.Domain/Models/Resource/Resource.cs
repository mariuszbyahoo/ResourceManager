﻿using ResourceManager.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Models
{
    public class Resource : IResource
    {
        public Guid Id { get; set; }
        public string Variant { get; set; }
    }
}
