﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ResourceManager.Domain;

namespace ResourceManager.Controllers
{
    /// <summary>
    /// Implementacja kontrolera zasobów
    /// </summary>
    public class ResourcesController : Controller, IController
    {
        public void AddResource(IResource resource, DateTime fromDate)
        {
            throw new NotImplementedException();
        }
        public void WithdrawResource(IResource resource, DateTime fromDate)
        {
            throw new NotImplementedException();
        }
        public bool FreeResource(IResource resource, ITenant tenant, DateTime date)
        {
            throw new NotImplementedException();
        }

        public bool LeaseResource(IResource resource, ITenant tenant, DateTime date)
        {
            throw new NotImplementedException();
        }

        public bool LeaseResource(string variant, ITenant tenant, DateTime date, out IResource resource)
        {
            throw new NotImplementedException();
        }
    }
}