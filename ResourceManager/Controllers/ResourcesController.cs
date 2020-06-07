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
            _leasingDatas.AddDataAboutResource(_resourceDataFactory.CreateInstance(resource.Id, fromDate, null, "ResourceData"));
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
                return NotFound("Resource with such an ID does not exist.");

            DateTime dateOfWithdrawal;
            DateTime.TryParseExact(withdrawalDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateOfWithdrawal);

            // Jeśli zasób był zajęty w przeszłości, czyli teraz jest wolny, ORAZ nie ma nikogo do niego przyporządkowanego,
            // to...
            if ((res.OccupiedTill < DateTime.Now) && (!res.LeasedTo.Equals(Guid.Empty)))
            {
                WithdrawResource(res, dateOfWithdrawal);
            }
            else // jeśli nie, to jest zajęty, i wtedy....
            {
                // jeśli będzie wycofany w momencie w którym jest dzierżawiony, to wyślij maila
                if (res.OccupiedTill > dateOfWithdrawal)
                {
                    if(!res.LeasedTo.Equals(Guid.Empty))
                    // Niniejsza pozagnieżdżana ifologia gwarantuje, że aplikacja nie wyrzuci NullReferenceException w linii 115
                        await _email.NotifyByEmail(res.LeasedTo,
                            _leasingDatas.GetDataAboutTenant(res.LeasedTo).EmailAddress, 
                            $"Zerwano umowę dzierżawy ze skutkiem na dzień {dateOfWithdrawal.Date}.", 
                            $"Dotyczy zasobu o ID: {res.Id}", _config);
                }
                // Jeśli nie będzie aktualnie zajęty, to nie ma co wysyłać maila.
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
            _leasingDatas.WithdrawResourceData(resource.Id, fromDate);
            _resources.WithdrawResource(resource, fromDate);
        }

        /// <summary>
        /// Metoda wywołująca właściwą komunikację z bazą danych, posiada zabezpieczenie przed NullReferenceException.
        /// !!!!!
        /// Dla uproszczenia przyjąłem, że mając resource, który jest w drodze do dostarczenia dostawcy 
        /// (nie jest wynajmowany - GUID.Empty) program to zignoruje, i przypisze do tego 
        /// (jeszcze nie dostępnego do dzierżawy) zasobu właśnie do tenanta wskazanego przy wywołaniu metody, 
        /// to też nie ma dużego znaczenia ponieważ jeśli dany zasób był w przeszłości wynajmowany przez kogoś 
        /// (occupiedTill z datą przeszłą i leasedTo które nie jest nullem) to po prostu wynajmie go nowemu dzierżawcy, 
        /// i nie będzie wysyłał maila temu, który w przeszłości wynajmował ten zasób.
        /// </summary>
        /// <param name="res">Id zasobu, którego akcja dotyczy</param>
        /// <param name="ten">Id dzierżawcy, który żąda dzierżawy zasobu. Jeśli zasób nie ma przypisanego dzierżawcy,
        /// W takim przypadku można wskazać tutaj ID dowolnego istniejącego dzierżawcy.</param>
        /// <param name="availableFrom">Data, z którą zasób zostanie zwolniony: yyyyMMdd nie zawiera godzin (domyślnie do 00:00 wskazanego dnia - czyli w podanym dniu zasób będzie wolny)</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("free")]
        public ActionResult FreeResourceAction(Guid res, Guid ten, string availableFrom)
        {
            DateTime date;
            DateTime.TryParseExact(availableFrom, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
            if (_resources.GetResource(res) == null)
                return NotFound($"Resource with an ID of: {res} is missing");

            var newResourceData = _resourceDataFactory.CreateInstance(res, date, _tenants.GetTenant(ten), "ResourceData");
            var acquiredTerminationDate = _leasingDatas.GetDataAboutResource(res).OccupiedTill;
            if (acquiredTerminationDate < date)
                return BadRequest("Resource will be available at the specified date, maybe wrong date provided?");
            var tenant = _tenants.GetTenant(ten);
            if (tenant == null)
                return NotFound($"Tenant with an ID of: {ten} is missing");
            /* IF o którym mowa w Docsach do metody */
            if ((!newResourceData.LeasedTo.Equals(tenant.Id)) && (!newResourceData.LeasedTo.Equals(Guid.Empty)))
                return BadRequest($"Resource with ID of:{newResourceData.Id} not belongs to tenant with ID of: {tenant.Id}");
            if (acquiredTerminationDate < DateTime.Now)
                return BadRequest("This resource is already available, wrong reource ID provided");

            if (FreeResource(_resources.GetResource(res), tenant, date))
            {
                var msg = $"Resource with an ID of:{res} released";
                // Jeśli był do kogoś przyporządkowany ten zasób na dzień dzisiejszy, to wyślij maila
                if (!_leasingDatas.GetDataAboutResource(res).LeasedTo.Equals(Guid.Empty) && acquiredTerminationDate > DateTime.Now)
                {
                    _email.NotifyByEmail(tenant.Id, _leasingDatas.GetDataAboutTenant(tenant.Id).EmailAddress, 
                        "Zerwano umowę dzierżawy", $"Dot. zasobu o Id {res}, wchodzi w życie w dn. {date}", _config);
                    msg += $", and message has been sent succesfully to tenant with an ID of:{ten}";
                }

                return Ok(msg);
            }
            else
            {
                /* wejście programu do tego bloku kodu oznacza, że podczas zwalniania zasobu wystąpił wyjątek:
                ręcznie zwracam kod odpowiedzi, by nie terminować działania programu i jednocześnie poinformować użytkownika API o błędzie.
                Który z kolei jest logowany w metodzie poniżej */
                _logger.LogToFile("Something went bad, inspect ResourceController at lines 154-178", "errors.txt");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured when releasing the resource, check the 'errors.txt' file");
            }
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


                if (_resources.FreeResource(resource, tenant, date))
                { 
                    if (_leasingDatas.GetDataAboutResource(resource.Id).LeasedTo.Equals(Guid.Empty))
                        tenant.Id = Guid.Empty;

                    _leasingDatas.SetDataAboutResource(resource.Id, _resourceDataFactory.CreateInstance(resource.Id, date, tenant, "ResourceData"));
                 /* Jeśli resource nie jest nikomu udostępniony - leasedTo = Guid.Empty, 
                 * to wtedy po prostu w pamięci tylko i wyłącznie tej metody 
                 * przypisuję pusty GUID do ID przekazanego tenanta i go zwalniam, w innym przypadku jego ID
                 * pozostaje takie jakie było. Podyktowane jest to koniecznością implementacji niezmienionego interfejsu
                 */
                }
                return true;
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
            var resourceData = _leasingDatas.GetDataAboutResource(res);
            if (resourceData == null)
                return NotFound("Resource with such an ID is missing");
            var tenant = _tenants.GetTenant(ten); // TODO change it to TenantData object
            if (tenant == null)
                return NotFound("Tenant with such an ID is missing");

            DateTime date;
            DateTime.TryParseExact(leasedTill, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

            if (!resourceData.LeasedTo.Equals(Guid.Empty) && resourceData.OccupiedTill > DateTime.Now) 
                return ResolveTenantsConflict(resource, tenant, date);
            if (resourceData.OccupiedTill > DateTime.Now)
                return NotFound($"Requested Resource is not available to anyone at the time... come back at {resourceData.OccupiedTill}");

            if (LeaseResource(resource, tenant, date))
            {

                    // TODO implement email notification
                return Ok($"Resource with an ID of:{res} leased to the tenant with an ID of {ten}, and a message has been sent");
            }
            else
            {
                _logger.LogToFile("An error occured when leasing the resource, check the 'errors.txt' file", "errors.txt");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured when leasing the resource, check the 'errors.txt' file");
            }
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
                if (_leasingDatas.SetDataAboutResource(resource.Id, _resourceDataFactory.CreateInstance(resource.Id, date, tenant, "ResourceData"))
                    .GetType().Equals(typeof(OkObjectResult))) 
                {
                    return _resources.LeaseResource(resource, tenant, date);
                }
                else
                {
                    throw new Exception("Something went bad when setting ResourceDatas table, inspect ResourcesController, line 274");
                }
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
        public ActionResult LeaseResourceAction(string variant, Guid ten, string leasedTill)
        {
            IResource resource;
            var resources = _resources.GetResources(variant);
            if (resources.Length == 0)
                return NotFound("Not found any resources with such a variant.");
            var tenant = _tenants.GetTenant(ten);
            if (tenant == null)
                return NotFound("Not found any tenant with such an ID");

            resource = _resources.FilterUnavailableResources(resources).FirstOrDefault();
            if (resource == null)
                return NotFound("Has not found any available resource with such a variant");

            DateTime date;
            DateTime.TryParseExact(leasedTill, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

            if (LeaseResource(variant, tenant, date, out resource))
                return Ok($"Resource with an ID of:{resource.Id} leased to the tenant with an ID of {ten}, and a message has been sent");
            else
                _logger.LogToFile("Something went bad when setting ResourceDatas table, inspect ResourcesController, line 302", "errors.txt");
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
                    {
                        return Ok($"Resource with an ID of:{resource.Id} leased to the tenant with an ID of {tenant.Id}, and a message has been sent, the resource has been expropriated from a user with an ID of {concurrent.Id}");
                    }
                    else
                    {
                        _logger.LogToFile("Something went bad when resolving the tenants conflict, inspect ResourcesController, line 368", "errors.txt");
                        return StatusCode(StatusCodes.Status500InternalServerError, "Somehow cannot lease the resource, check the 'errors.txt' file");
                    }
                }
                return NotFound("Resource with such an ID is already leased by a tenant with higher priority");
        }
    }
}