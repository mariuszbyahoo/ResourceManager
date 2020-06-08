using ResourceManager.Data.Services;
using ResourceManager.Domain.Enums;
using ResourceManager.Domain.Factories;
using ResourceManager.Domain.Models;
using ResourceManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManager.Data.Repos
{
    public class ResourceRepo : IResourceRepo
    {
        public ManagerDbContext Ctx { get; set; }
        private IRemoveService removeService;
        private ILeasingDataRepo _leasingData;
        private IResourceDataFactory _resourceDataFactory;
        private ILoggerService _logger;

        public ResourceRepo(ManagerDbContext ctx, IRemoveService service, ILoggerService logger, 
            ILeasingDataRepo leasingData, IResourceDataFactory resourceDataFactory)
        {
            Ctx = ctx;
            removeService = service;
            _logger = logger;
            _leasingData = leasingData;
            _resourceDataFactory = resourceDataFactory;
        }

        public void AddResource(IResource resource, DateTime availableFromDate)
        {
            var res = resource as Resource;
            Ctx.Resources.Add(res);
            Ctx.SaveChanges();
        }

        public bool FreeResource(IResource resource, ITenant tenant, DateTime date)
        {
            Ctx.Update(resource);
            Ctx.SaveChanges();
            return true;
        }
        //************************* SPRAWDŹ CZY DZIAŁA POPRAWNIE!!!!
        public Resource[] FilterUnavailableResources(Resource[] resources)
        {
            var resultLength = 0;
            var processedResourceDatas = new ResourceData[resources.Length];
            for(int i = 0; i < resources.Length; i ++)
            {
                var resource = resources[i];
                var processedData = _leasingData.GetDataAboutResource(resource.Id);
                //ResourceData processedResourceData = _leasingData.GetDataAboutResource(resource.Id);
                // jeśli kontrakt na dany zasób zakończył się w przeszłości, to...
                if (processedData.OccupiedTill < DateTime.Now)
                {
                    processedResourceDatas[resultLength] = _resourceDataFactory.CreateInstance(processedData.Id,
                        processedData.OccupiedTill, null, "ResourceData") as ResourceData;
                    resultLength++;
                }
            }
            var availableResources = new Resource[resultLength];
            for(int i = 0; i < availableResources.Length; i++)
            {
                availableResources[i] = Ctx.Resources.Where(r => r.Id.Equals(processedResourceDatas[i].Id)).FirstOrDefault();
            }
            return availableResources;
        }

        public Resource GetResource(Guid id)
        {
            return Ctx.Resources.Where(r => r.Id.Equals(id)).FirstOrDefault();
        }

        public Resource[] GetResources(string variant)
        {
            return Ctx.Resources.Where(r => r.Variant.ToLower().Equals(variant.ToLower())).ToArray();
        }

        public bool LeaseResource(IResource resource, ITenant tenant, DateTime date)
        {
            Ctx.Update(resource);
            Ctx.SaveChanges();
            return true;
        }

        public IResource LeaseResource(string variant, ITenant tenant, DateTime date)
        {
            var resources = GetResources(variant);
            if (resources.Length == 0)
                return null;

            var resource = FilterUnavailableResources(resources).FirstOrDefault();

            Ctx.Update(resource);
            Ctx.SaveChanges();
            return resource;
        }

        /// <summary>
        /// Metoda uruchamiana na innym wątku, w momencie, gdy nadejdzie wskazana data usunięcia zasobu, wtedy go usunie
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="withdrawedOnDate"></param>
        public async void WithdrawResource(IResource resource, DateTime withdrawedOnDate)
        {
            // TODO Metoda poprawnie w przypadku wyrzucenia wyjątku jak najbardziej loguje go do pliku, ale API zwraca 204 że niby ok, więc jak to zrobić żeby było ok skoro tutaj musi być void?
            try
            {
                var res = await removeService.CheckDate(withdrawedOnDate);
                Ctx.Resources.Remove(resource as Resource);
                Ctx.SaveChanges();
            }
            catch(Exception ex)
            {
                await _logger.LogToFile(ex, "errors.txt");
            }
        }
    }
}
