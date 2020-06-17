using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using maple_web_api.Models;
using Microsoft.Extensions.Logging;

namespace maple_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractItemsController : ControllerBase
    {
        public ContractItemsController(Logger<ContractItemsController> logger)
        {
            _logger = logger;
        }
        private readonly InsuranceInfoContext _context;
        private readonly Logger<ContractItemsController> _logger;

        public ContractItemsController(InsuranceInfoContext context)
        {
            _context = context;
        }

        // GET: api/ContractItems
        [HttpGet]
        public IActionResult GetContractItems()
        {
            return Ok(_context.ContractItems.ToList());
        }

        // GET: api/ContractItems/5
        [HttpGet("{id}", Name = "GetContract")]
        public IActionResult GetContractItem(int id)
        {
            var contractItem = _context.ContractItems.Find(id);

            if (contractItem == null)
            {
                _logger.LogInformation($"Contract with id {id} not found.");
                return NotFound();
            }
            contractItem.Customer = _context.Customers.Where(c => c.CustomerId == contractItem.CustomerId).First();
            contractItem.CoveragePlan = _context.CoveragePlans.Where(cp => cp.PlanId == contractItem.CoverageId).First();
            return Ok(contractItem);
        }

        // PUT: api/ContractItems/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut()]
        public IActionResult PutContractItem(string customerName, DateTime dob, string gender)
        {
            Customer customer = _context.Customers.Where(c => c.Name == customerName).FirstOrDefault();
            if (customer == null)
            {
                _logger.LogInformation($"Customer with name {customerName} not found.");
                ModelState.AddModelError("Customer Name", "No Customer with this name!");
                return BadRequest(ModelState);
            }
            var planType = _context.CoveragePlans.Where(cp =>
               cp.EligibilityCountry == customer.Country &&
               cp.EligibilityDateFrom < customer.DateOfBirth &&
               cp.EligibilityDateTo > customer.DateOfBirth).FirstOrDefault();
            var age = DateTime.Now.Year - dob.Year;

            if (planType == null)
            {
                _logger.LogInformation($"Suitable Coverage Plan for the provided parameters not found.");
                ModelState.AddModelError("Coverage Plan", "Coverage Plan not found!");
                return BadRequest(ModelState);
            }
            object g = null;
            Gender cgender = (Gender)
            (Enum.TryParse(typeof(Gender), gender, true, out g) ? g : Gender.Other);
            var rate = _context.RateCharts.Where(ch =>
             ch.Gender == cgender
             && ch.CuttoffAge > age &&
            ch.CoveragePlan.PlanId == planType.PlanId).FirstOrDefault();
            if (rate == null)
            {
                _logger.LogInformation($"Rate not found for the Plan Id {planType.PlanId}.");
                ModelState.AddModelError("Rate", "Rate not found!");
                return BadRequest(ModelState);
            }
            var contractItem = new ContractItem
            {
                CustomerId = customer.CustomerId,
                SaleDate = DateTime.Now.Date,
                CoverageId = planType.PlanId,
                NetPrice = rate.NetPrice
            };

            _context.Entry(contractItem).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogCritical("Database update concurrency exception thrown", ex);
                return StatusCode(500, "Internal Server Error has occurred.");
            }

            return NoContent();
        }

        // POST: api/ContractItems
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost()]
        public IActionResult PostContractItem([FromBody] ContractDetailsForPostDto contractDetails)
        {
            var customer = _context.Customers.Where(c => c.Name == contractDetails.CustomerName).FirstOrDefault();
            if (customer == null)
            {
                _logger.LogInformation($"Customer with name {contractDetails.CustomerName} not found.");
                ModelState.AddModelError("Customer Name", "Customer does not exist!");
                return BadRequest(ModelState);
            }
            var planType = _context.CoveragePlans.Where(cp =>
                cp.EligibilityCountry == contractDetails.CustomerCountry &&
                cp.EligibilityDateFrom < customer.DateOfBirth &&
                cp.EligibilityDateTo > customer.DateOfBirth).FirstOrDefault();
            var age = DateTime.Now.Year - contractDetails.DOB.Year;
            if (planType == null)
            {
                _logger.LogInformation("Suitable Coverage Plan not found for provided paramters.");
                ModelState.AddModelError("Coverage Plan", "Coverage Plan not found!");
                return BadRequest(ModelState);
            }
            object g = null;
            Gender gender = (Gender)
            (Enum.TryParse(typeof(Gender), contractDetails.CustomerGender, true, out g) ? g : Gender.Other);
            var rate = _context.RateCharts.Where(ch =>
             ch.Gender == gender
             && ch.CuttoffAge > age &&
            ch.CoveragePlan.PlanId == planType.PlanId).FirstOrDefault();
            if (rate == null)
            {
                _logger.LogInformation($"Rate not found for Plan Id {planType.PlanId}.");
                ModelState.AddModelError("Rate", "Rate not found!");
                return BadRequest(ModelState);
            }
            var contractItem = new ContractItem
            {
                CustomerId = customer.CustomerId,
                SaleDate = contractDetails.SaleDate,
                CoverageId = planType.PlanId,
                NetPrice = rate.NetPrice
            };
            _context.ContractItems.Add(contractItem);
            _context.SaveChanges();

            return CreatedAtAction("GetContract", new { id = contractItem.ContractId }, contractItem);
        }

        // DELETE: api/ContractItems/5
        [HttpDelete("{id}")]
        public IActionResult DeleteContractItem(int id)
        {
            var contractItem = _context.ContractItems.Find(id);
            if (contractItem == null)
            {
                _logger.LogInformation($"No contract found with id {id}.");
                ModelState.AddModelError("Id", "No Contract Found!");
                return BadRequest(ModelState);
            }

            _context.ContractItems.Remove(contractItem);
            _context.SaveChanges();

            return NoContent();
        }

        private bool ContractItemExists(int id)
        {
            return _context.ContractItems.Any(e => e.ContractId == id);
        }
    }
}
