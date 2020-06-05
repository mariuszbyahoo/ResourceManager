﻿using ResourceManager.Domain.Enums;
using ResourceManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Factories
{
    public class ResourceFactory : IResourceFactory
    {
        /// <summary>
        /// Tworzy nowy zasób, o podanym wariancie, domyślnie jest dostępny i nie wydzierżawiony nikomu.
        /// </summary>
        /// <param name="variant"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IResource CreateInstance(string variant, ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Resource:
                    return new Resource
                    {
                        Variant = variant
                    };
                default:
                    return null;
            }
        }
    }
}
