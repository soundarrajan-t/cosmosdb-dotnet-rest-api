using System.Collections.Generic;
using System.Threading.Tasks;
using FamilyAPI.Models;
using FamilyAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FamilyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FamilyController : ControllerBase
    {
        private readonly ILearnerRepository _learnerRepository;

        public FamilyController(ILearnerRepository learnerRepository)
        {
            this._learnerRepository = learnerRepository;
        }

        // GET: api/<FamilyController>
        [HttpGet]
        public async Task<IEnumerable<Family>> Get()
        {
            return await _learnerRepository.GetAllRecords();
        }

        // GET api/<FamilyController>/5
        [HttpGet("{partitionKey}")]
        public async Task<IEnumerable< Family>> Get(string partitionKey)
        {
           return await _learnerRepository.GetRecords(partitionKey);
        }

        // POST api/<FamilyController>
        [HttpPost]
        public async Task Post([FromBody] Family newFamily)
        {
            await _learnerRepository.AddItemsToContainerAsync(newFamily);
        }

        // PUT api/<FamilyController>/5
        [HttpPut("{partitionKeyValue}")]
        public async Task Put([FromBody] Family newFamily,  string partitionKeyValue)
        {
            await _learnerRepository.ReplaceFamilyItemAsync(newFamily, partitionKeyValue);
        }

        // DELETE api/<FamilyController>/5
        [HttpDelete("{partitionKeyValue}/{familyId}")]
        public async Task Delete(string partitionKeyValue, string familyId)
        {
            await _learnerRepository.DeleteFamilyItemAsync(partitionKeyValue, familyId);
        }
        
    }
}
