using System.Collections.Generic;
using System.Threading.Tasks;
using FamilyAPI.Models;

namespace FamilyAPI.Repositories
{
    public interface ILearnerRepository
    {
        Task<IEnumerable<Family>> GetAllRecords();
        Task<IEnumerable<Family>> GetRecords(string partitionKey);
        Task AddItemsToContainerAsync(Family newFamily);
        Task ReplaceFamilyItemAsync(Family newFamily, string partitionKeyValue);
        Task DeleteFamilyItemAsync(string partitionKeyValue, string familyId);
    }
}