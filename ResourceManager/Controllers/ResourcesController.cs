using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ResourceManager.Data.Repos;
using ResourceManager.Data.Services;
using ResourceManager.Domain.Enums;
using ResourceManager.Domain.Factories;
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
        private ILeasingDataRepo _leasingDatas;
        private ITenantRepo _tenants;
        private IConfiguration _config;
        private ILoggerService _logger;
        private IEmailService _email;
        private IResourceDataFactory _resourceDataFactory;
        private ITenantDataFactory _tenantDataFactory;
        private string dateFormat;

        public ResourcesController(IResourceRepo resources, ITenantRepo tenants, IConfiguration config, 
            ILoggerService logger, IEmailService email, ILeasingDataRepo leasingDatas,
            IResourceDataFactory resourceDataFactory, ITenantDataFactory tenantDataFactory)
        {
            _resources = resources;
            _tenants = tenants;
            _config = config;
            _logger = logger;
            _email = email;
            _resourceDataFactory = resourceDataFactory;
            _tenantDataFactory = tenantDataFactory;
            _leasingDatas = leasingDatas;
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
        /// <param name="resource">Dodawany zasób</param>
        /// <param name="fromDate">Data, z którą zasób staje się dostępny: yyyyMMdd nie zawiera godzin (domyślnie do 00:00 wskazanego dnia - czyli w podanym dniu zasób będzie wolny)</param>
        public void AddResource(IResource resource, DateTime fromDate)
        {
            // TODO zaimplementuj poniższą linię w każdej metodzie poniżej.
            _leasingDatas.AddDataAboutResource(_resourceDataFactory.CreateInstance(resource.Id, fromDate, "ResourceData"));
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
        public async Task<IActionResult> WithdrawResourceAction(string withdrawalDate, Guid Id)
        {
            var res = _resources.GetResource(Id);
            if (res == null)
                return BadRequest("Resource with such an ID does not exist.");

            DateTime dateOfWithdrawal;
            DateTime.TryParseExact(withdrawalDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateOfWithdrawal);

            // Jeśli zasób był zajęty w przeszłości
            if (res.OccupiedTill < DateTime.Now && res.LeasedTo != Guid.Empty)
            {
                WithdrawResource(res, dateOfWithdrawal);
            }
            else
            {
                // jeśli zostanie zwolniony przed usunięciem z puli, to nie ma co wysyłać maila
                if (res.OccupiedTill > dateOfWithdrawal)
                {
                    // TODO Dorób serwis, odpowiadający za wybranie adresu e-mail z tabeli w DB
                    await _email.NotifyByEmail(res.LeasedTo, "mariusz.budzisz@yahoo.com", 
                        $"Zerwano umowę dzierżawy ze skutkiem na dzień {dateOfWithdrawal.Date}.", 
                        $"Dotyczy zasobu o ID: {res.Id}", _config);
                }
                /* TODO Dorób obsługę sytuacji, gdy np. zasób jest wolny, wyznaczony do usunięcia na dzień 20.06.2020r. a ktoś go chce wydzierżawić w dn. 19-21.06.2020 */
                WithdrawResource(res, dateOfWithdrawal);
            }
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
            // TODO zamiast encji będą procesowane encje zaw. dane odn obiektu resource.
            var resource = _resources.GetResource(res);
            if (resource == null)
                return BadRequest($"Resource with an ID of: {res} is missing");
            var tenant = _tenants.GetTenant(ten);
            if (tenant == null)
                return BadRequest($"Tenant with an ID of: {ten} is missing");
            if (!resource.LeasedTo.Equals(tenant.Id))
                return BadRequest($"Resource with ID of:{resource.Id} not belongs to tenant with ID of: {tenant.Id}");
            if (resource.Availability.Equals(ResourceStatus.Available))
                return BadRequest("This resource is already available, wrong reource ID provided");

            DateTime date;
            DateTime.TryParseExact(availableFrom, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
            if (resource.OccupiedTill < date)
                return BadRequest("Resource will be available at the specified date, maybe wrong date provided?");

            if (FreeResource(resource, tenant, date))
            {
                return Ok($"Resource with an ID of:{res} released, and message has been sent succesfully to tenant with an ID of:{ten}");
            }
            else
            {
                // ręcznie zwracam kod odpowiedzi, by nie terminować działania programu i jednocześnie poinformować użytkownika API o błędzie.
                // Który z kolei jest logowany w metodzie poniżej
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured when releasing the resource, check the 'errors.txt' file");
            }

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
        /// <param name="leasedTill">Data, do której będzie trwać dzierżawa: yyyyMMdd nie zawiera godzin (domyślnie do 00:00 wskazanego dnia - czyli w podanym dniu zasób będzie wydzierżawiony przez wnoszącego)</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("lease")]
        public IActionResult LeaseResourceAction(Guid res, Guid ten, string leasedTill) 
        {
            var resource = _resources.GetResource(res);
            if (resource == null)
                return BadRequest("Resource with such an ID is missing");
            var tenant = _tenants.GetTenant(ten);
            if (tenant == null)
                return BadRequest("Resource with such an ID is missing");

            DateTime date;
            DateTime.TryParseExact(leasedTill, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

            if (!resource.LeasedTo.Equals(Guid.Empty) && resource.OccupiedTill > DateTime.Now)
                return ResolveTenantsConflict(resource, tenant, date);

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
                return NotFound("Has not found any available resource with such a variant");

            DateTime date;
            DateTime.TryParseExact(leasedTill, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

            if (LeaseResource(variant, tenant, date, out resource))
                return Ok($"Resource with an ID of:{resource.Id} leased to the tenant with an ID of {ten}, and a message has been sent");
            else
                // Nie wyrzucam wyjątku, zamiast tego sygnalizuję, że coś poszło nie tak, ponieważ kod nie powinien tu dotrzeć w ogóle po sprawdzeniu możliwych scenariuszy negatywnych.
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went bad during lease granting process occured when leasing the resource, check the 'errors.txt' file");
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
        /// <param name="leaseTill">Termin do którego wniesiono o dzierżawę</param>
        /// <returns></returns>
        private IActionResult ResolveTenantsConflict(IResource resource, ITenant tenant, DateTime leaseTill)
        {
                var concurrent = _tenants.GetTenant(resource.LeasedTo);
                if (concurrent == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, "Resource is leased to tenant which is not existing anymore, an error occured.");
                if (concurrent.Priority < tenant.Priority)
                {
                // TODO notify the concurrent about the leasing contract termination due to higher priority tenant requested a resource
                    if (LeaseResource(resource, tenant, leaseTill))
                        return Ok($"Resource with an ID of:{resource.Id} leased to the tenant with an ID of {tenant.Id}, and a message has been sent, the resource has been expropriated from a user with an ID of {concurrent.Id}");
                    else
                        return StatusCode(StatusCodes.Status500InternalServerError, "Somehow cannot lease the resource, check the 'errors.txt' file");
                }
                return NotFound("Resource with such an ID is already leased by a tenant with higher priority");
        }
    }
}