using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ResourceManager.Data.Repos;
using ResourceManager.Data.Services;
using ResourceManager.Domain.Models;

namespace ResourceManager.Controllers
{
    /// <summary>
    /// Implementacja kontrolera zasobów
    /// </summary>
    [Route("resources")]
    public class ResourcesController : Controller, IController
    {
        private IResourceRepo _resources;
        private ITenantRepo _tenants;
        private IConfiguration _config;
        private ILoggerService _logger;
        private string dateFormat;

        public ResourcesController(IResourceRepo resources, ITenantRepo tenants, IConfiguration config, ILoggerService logger)
        {
            _resources = resources;
            _tenants = tenants;
            _config = config;
            _logger = logger;
            dateFormat = _config.GetSection("DateFormats").GetSection("Default").Value;
        }

        /// <summary>
        /// Metoda wywoływana poprzez żądanie HTTP Post
        /// </summary>
        /// <param name="availableFrom">Data, z którą zasób staje się dostępny: yyyyMMdd nie zawiera godzin (domyślnie do 00:00 wskazanego dnia - czyli w podanym dniu zasób będzie wolny)</param>
        /// <param name="resource">Dodawany zasób</param>
        /// <returns></returns>
        [HttpPost]
        [Route("add")]
        public ActionResult<IResource> AddResourceAction([FromQuery] string availableFrom, [FromBody]Resource resource)
        {
            if (resource == null)
                return BadRequest("Incorrect data provided");

            if (_resources.GetResource(resource.Id) != null)
                return BadRequest("Resource with such an ID already exists");

            DateTime date;
            DateTime.TryParseExact(availableFrom, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

            AddResource(resource, date);

            return Created("add", resource);
        }

        /// <summary>
        /// Właściwa implementacja metody kontrolera
        /// </summary>
        /// <param name="resource">Data, z którą zasób staje się dostępny: yyyyMMdd nie zawiera godzin (domyślnie do 00:00 wskazanego dnia - czyli w podanym dniu zasób będzie wolny)</param>
        /// <param name="fromDate">Dodawany zasób</param>
        public void AddResource(IResource resource, DateTime fromDate)
        {
            _resources.AddResource(resource, fromDate);
        }

        /// <summary>
        /// Metoda wywołująca właściwą komunikację z bazą danych, posiada zabezpieczenie przed NullReferenceException.
        /// </summary>
        /// <param name="withdrawalDate">Data, z którą zasób zostanie usunięty: yyyyMMdd nie zawiera godzin (domyślnie do 00:00 wskazanego dnia - czyli w podanym dniu zasób zostanie usunięty)</param>
        /// <param name="Id">Id zasobu</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("delete")]
        // TODO zaimplementuj usunięcie z bazy w danym dniu. Sprawdź raz dziennie gdy godzina wynosi 00:00 czy to czas na usunięcie.
        public IActionResult WithdrawResourceAction(string withdrawalDate, Guid Id)
        {
            var res = _resources.GetResource(Id);
            if (res == null)
                return BadRequest("Resource with such an ID does not exist.");

            DateTime date;
            DateTime.TryParseExact(withdrawalDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

            // Jeśli zasób był zajęty w przeszłości
            if (res.OccupiedTill < DateTime.Now && res.LeasedTo != Guid.Empty)
                WithdrawResource(res, date);
            else
                /* TODO powiadom o zerwaniu umowy przez dostawcę i konieczności zwrotu zasobu do dostawcy ze skutkiem na dzień {date} */
                // TODO Dorób obsługę sytuacji, gdy np. zasób jest wolny, wyznaczony do usunięcia na dzień 20.06.2020r. a ktoś go chce wydzierżawić w dn. 19-21.06.2020
                WithdrawResource(res, date);

            return NoContent();
        }

        /// <summary>
        /// Właściwa implementacja metody interfejsu IController
        /// </summary>
        /// <param name="resource">Przedmiot metody.</param>
        /// <param name="fromDate">Data, z którą zasób zostanie usunięty: yyyyMMdd nie zawiera godzin (domyślnie do 00:00 wskazanego dnia - czyli w podanym dniu zasób zostanie usunięty)</param>
        public void WithdrawResource(IResource resource, DateTime fromDate)
        {
            _resources.WithdrawResource(resource, fromDate);
        }

        /// <summary>
        /// Metoda wywołująca właściwą komunikację z bazą danych, posiada zabezpieczenie przed NullReferenceException.
        /// </summary>
        /// <param name="res">Id zasobu, którego akcja dotyczy</param>
        /// <param name="ten">Id dzierżawcy, który żąda dzierżawy zasobu</param>
        /// <param name="availableFrom">Data, z którą zasób zostanie zwolniony: yyyyMMdd nie zawiera godzin (domyślnie do 00:00 wskazanego dnia - czyli w podanym dniu zasób będzie wolny)</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("free")]
        public ActionResult FreeResourceAction(Guid res, Guid ten, string availableFrom)
        {
            var resource = _resources.GetResource(res);
            if (resource == null)
                return BadRequest($"Resource with an ID of: {res} is missing");
            var tenant = _tenants.GetTenant(ten);
            if (tenant == null)
                return BadRequest($"Tenant with an ID of: {ten} is missing");
            if (!resource.LeasedTo.Equals(tenant.Id))
                return BadRequest($"Resource with ID of:{resource.Id} not belongs to tenant with ID of: {tenant.Id}");

            DateTime date;
            DateTime.TryParseExact(availableFrom, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

            if (FreeResource(resource, tenant, date))
                return Ok($"Resource with an ID of:{res} released, and message has been sent succesfully to tenant with an ID of:{ten}");
            else
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured when releasing the resource, check the 'errors.txt' file");

            // TODO notify the tenant !!
        }

        /// <summary>
        /// Właściwa metoda implementująca metodę interfejsu IController
        /// </summary>
        /// <param name="resource">Przedmiot akcji metody</param>
        /// <param name="tenant">Dzierżawca zwalniający zasób</param>
        /// <param name="date">Data, z którą zasób zostanie zwolniony: yyyyMMdd nie zawiera godzin (domyślnie do 00:00 wskazanego dnia - czyli w podanym dniu zasób będzie wolny)</param>
        /// <returns></returns>
        public bool FreeResource(IResource resource, ITenant tenant, DateTime date)
        {
            try
            {
                // TODO send a message to Tenant
                return _resources.FreeResource(resource, tenant, date);
            }
            catch (Exception ex)
            {
                _logger.LogToFile(ex,"errors.txt");
                return false;
            }
        }

        /// <summary>
        /// Metoda wywołująca właściwą komunikację z bazą danych, posiada zabezpieczenie przed NullReferenceException. Dotyczy konkretnego zasobu.
        /// </summary>
        /// <param name="res">Przedmiot działania metody</param>
        /// <param name="ten">Dzierżawca wnoszący o dzierżawę</param>
        /// <param name="availableFrom">Data, do której będzie trwać dzierżawa: yyyyMMdd nie zawiera godzin (domyślnie do 00:00 wskazanego dnia - czyli w podanym dniu zasób będzie wydzierżawiony przez wnoszącego)</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("lease")]
        public IActionResult LeaseResourceAction(Guid res, Guid ten, string availableFrom) 
        {
            var resource = _resources.GetResource(res);
            if (resource == null)
                return BadRequest("Resource with such an ID is missing");
            var tenant = _tenants.GetTenant(ten);
            if (tenant == null)
                return BadRequest("Resource with such an ID is missing");
            if (!resource.LeasedTo.Equals(Guid.Empty))
                return ResolveTenantsConflict(resource, tenant);

            DateTime date;
            DateTime.TryParseExact(availableFrom, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

            if (LeaseResource(resource, tenant, date))
                return Ok($"Resource with an ID of:{res} leased to the tenant with an ID of {ten}, and a message has been sent");
            else
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured when leasing the resource, check the 'errors.txt' file");
        }

        /// <summary>
        /// Właściwa metoda implementująca metodę interfejsu IController; dot. konkretnego zasobu
        /// </summary>
        /// <param name="resource">Przedmiot działania metody</param>
        /// <param name="tenant">Dzierżawca wnoszący o dzierżawę</param>
        /// <param name="term">Data, do której będzie trwać dzierżawa: yyyyMMdd nie zawiera godzin (domyślnie do 00:00 wskazanego dnia - czyli w podanym dniu zasób będzie wydzierżawiony wnoszącemu)</param>
        /// <returns></returns>
        public bool LeaseResource(IResource resource, ITenant tenant, DateTime date)
        {
            try
            {
                return _resources.LeaseResource(resource, tenant, date);
            }
            catch(Exception ex)
            {
                _logger.LogToFile(ex, "errors.txt");
                return false;
            }
        }

        /// <summary>
        /// Metoda wywołująca właściwą komunikację z bazą danych, posiada zabezpieczenie przed NullReferenceException. 
        /// Dotyczy dowolnego zasobu z podanego wariantu.
        /// </summary>
        /// <param name="variant">Żądany wariant dzierżawionego zasobu.</param>
        /// <param name="ten">Id dzierżawcy wnoszącego o dzierżawę</param>
        /// <param name="leasedTill">Data, do której będzie trwać dzierżawa: yyyyMMdd nie zawiera godzin (domyślnie do 00:00 wskazanego dnia - czyli w podanym dniu zasób będzie wydzierżawiony wynoszącemu)</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("lease/any")]
        // TODO dokończ kwestię data handlingu
        public ActionResult LeaseResourceAction(string variant, Guid ten, string leasedTill)
        {
            IResource resource;
            var resources = _resources.GetResources(variant);
            if (resources.Length == 0)
                return NotFound("Not found any resources with such a variant.");
            var tenant = _tenants.GetTenant(ten);
            // TODO opisz w mailu czemu tu NotFound a innym razem BadRequest
            if (tenant == null)
                return BadRequest("Not found any tenant with such an ID");

            resource = _resources.FilterUnavailableResources(resources).FirstOrDefault();
            if (resource == null)
                // TODO zaimplementuj wywłaszczenie zasobu od dzierżawcy z najniższym priorytetem
                return NotFound("Has not found any available resource with such a variant");

            DateTime date;
            DateTime.TryParseExact(leasedTill, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

            if (LeaseResource(variant, tenant, date, out resource))
                return Ok($"Resource with an ID of:{resource.Id} leased to the tenant with an ID of {ten}, and a message has been sent");
            else
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured when leasing the resource, check the 'errors.txt' file");
        }

        /// <summary>
        /// Właściwa metoda implementująca interfejs IController, a jako że ten został wysłany w treści zadania przyjęto, 
        /// że nie można w nim zmienić nawet nazwy, bez uprzedniej konsultacji.
        /// </summary>
        /// <param name="variant">Żądany wariant dzierżawionego zasobu.</param>
        /// <param name="tenant">Id dzierżawcy wnoszącego o dzierżawę</param>
        /// <param name="date">Dla uproszczenia przyjęto, że 'date' oznacza datę, do której zasób będzie wynajmowany (domyślnie do 00:00 wskazanego dnia - czyli w podanym dniu zasób będzie wydzierżawiony wnoszącemu), jako datę startową przyjęto datę złożenia zlecenia wynajmu.</param>
        /// <param name="resource">Przedmiot dzierżawy.</param>
        /// <returns></returns>
        public bool LeaseResource(string variant, ITenant tenant, DateTime date, out IResource resource)
        {
            // LEPSZY SPOSÓB NA TRY CATCHE W ASP.NET CORE API : https://stackoverflow.com/questions/37793418/how-to-return-http-500-from-asp-net-core-rc2-web-api
            // ZRÓB TAK JAK POWYŻEJ!!!!!!!!!!!!!!!!!!!!!!*******************************************************************************************
            try
            {
                resource = null;
                var resources = _resources.GetResources(variant);
                if (resources.Length == 0)
                    return false;

                resource = _resources.FilterUnavailableResources(resources).FirstOrDefault();
                if (resource == null)
                    return false;

                return _resources.LeaseResource(resource, tenant, date);
            }
            catch(Exception ex)
            {
                _logger.LogToFile(ex, "errors.txt");
                resource = null;
                return false;
            }
        }

        /// <summary>
        /// Metoda rozwiązująca konflikt pomiędzy dwoma dzierżawcami, gdy przedmiot sporu jest obecnie dzierżawiony
        /// </summary>
        /// <param name="resource">Przedmiot konfliktu</param>
        /// <param name="tenant">Dzierżawca wnoszący o dzierżawę</param>
        /// <returns></returns>
        private IActionResult ResolveTenantsConflict(IResource resource, ITenant tenant)
        {
                var concurrent = _tenants.GetTenant(resource.LeasedTo);
                if (concurrent == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, "Resource is leased to tenant which is not existing anymore, an error occured.");
                if (concurrent.Priority < tenant.Priority)
                {
                // TODO notify the concurrent about the leasing contract termination due to higher priority tenant requested a resource
                    if (LeaseResource(resource, tenant, DateTime.Now))
                        return Ok($"Resource with an ID of:{resource.Id} leased to the tenant with an ID of {tenant.Id}, and a message has been sent");
                    else
                        return StatusCode(StatusCodes.Status500InternalServerError, "An error occured when leasing the resource, check the 'errors.txt' file");
                }
                return BadRequest("Resource with such an ID is already leased by a tenant with higher priority");
        }
    }
}