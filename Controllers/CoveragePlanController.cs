using maple_web_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace maple_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoveragePlanController : ControllerBase
    {
        private readonly IInsuranceInfoRepository _repository;

        private readonly ILogger<ContractItemsController> _logger;

        public CoveragePlanController(IInsuranceInfoRepository context, ILogger<ContractItemsController> logger)
        {
            _repository = context;
            _logger = logger;
        }

        // GET: api/CoveragePlan
        [HttpGet]
        public IActionResult GetCoveragePlanItem()
        {
            return Ok(_repository.GetCoveragePlans());
        }

        // GET: api/CoveragePlan/5
        [HttpGet("{id}", Name = "GetCoveragePlanItem")]
        public IActionResult GetCoveragePlanItem(int id)
        {
            var coveragePlanItem = _repository.GetCoveragePlan(id);

            if (coveragePlanItem == null)
            {
                _logger.LogInformation($"Unable to find Coverage Plan with Id {id}.");
                return NotFound();
            }

            return Ok(coveragePlanItem);
        }

        // PUT: api/CoveragePlan/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public IActionResult PutCoveragePlanItem(int id, [FromBody] CoveragePlanItem coveragePlanItem)
        {
            if (id != coveragePlanItem.PlanId)
            {
                _logger.LogInformation($"{id} does not match coveragePlanItem.id = {coveragePlanItem.PlanId}.");
                return BadRequest();
            }
            try
            {
                _repository.EditCoveragePlan(id, coveragePlanItem);
            }

            catch (DbUpdateConcurrencyException)
            {
                if (!_repository.CoveragePlanItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // POST: api/CoveragePlan
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public IActionResult PostCoveragePlanItem([FromBody] CoveragePlanItem coveragePlanItem)
        {
            _repository.SaveCoveragePlan(coveragePlanItem);


            return CreatedAtAction("GetCoveragePlanItem", new { id = coveragePlanItem.PlanId }, coveragePlanItem);
        }

        // DELETE: api/CoveragePlan/5
        [HttpDelete("{id}")]
        public IActionResult DeleteCoveragePlanItem(int id)
        {
            var coveragePlanItem = _repository.GetCoveragePlan(id);
            if (coveragePlanItem == null)
            {
                return NotFound();
            }

            _repository.DeleteCoveragePlan(coveragePlanItem);

            return NoContent();
        }


    }
}
