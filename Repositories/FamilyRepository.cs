using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using FamilyAPI.Models;
using Microsoft.Azure.Cosmos;

namespace FamilyAPI.Repositories
{
    public class LearnerRepository : ILearnerRepository
    {
        // The Azure Cosmos DB endpoint for running this sample.
        public static readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUri"];

        // The primary key for the Azure Cosmos account.
        public static readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];

        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        // The name of the database and container we will create
        private string databaseId = "LearnersDB";
        private string containerId = "Learner";

        public LearnerRepository()
        {
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey,
                new CosmosClientOptions() {ApplicationName = "CosmosDBDotnetQuickstart"});
            GetStartedDemoAsync().Wait();
        }

        public async Task GetStartedDemoAsync()
        {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey,
                new CosmosClientOptions() {ApplicationName = "CosmosDBDotnetQuickstart"});
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
            await this.ScaleContainerAsync();
            await this.AddItemsToContainerAsync();
        }

        // Create the database if it does not exist
        private async Task CreateDatabaseAsync()
        {
            // Create a new database
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            //Console.WriteLine("Created Database: {0}\n", this.database.Id);
        }

        // Create the container if it does not exist. 
        // Specifiy "/partitionKey" as the partition key path since we're storing family information, to ensure good distribution of requests and storage.
        private async Task CreateContainerAsync()
        {
            // Create a new container
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/partitionKey");
            //Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }

        // Scale the throughput provisioned on an existing Container.
        // You can scale the throughput (RU/s) of your container up and down to meet the needs of the workload. Learn more: https://aka.ms/cosmos-request-units
        private async Task ScaleContainerAsync()
        {
            // Read the current throughput
            try
            {
                int? throughput = await this.container.ReadThroughputAsync();
                if (throughput.HasValue)
                {
                    //Console.WriteLine("Current provisioned throughput : {0}\n", throughput.Value);
                    int newThroughput = throughput.Value + 100;
                    // Update throughput
                    await this.container.ReplaceThroughputAsync(newThroughput);
                    //Console.WriteLine("New provisioned throughput : {0}\n", newThroughput);
                }
            }
            catch (CosmosException cosmosException) when (cosmosException.StatusCode == HttpStatusCode.BadRequest)
            {
                //Console.WriteLine("Cannot read container throuthput.");
                //Console.WriteLine(cosmosException.ResponseBody);
            }
        }

        // Add Family items to the container
        private async Task AddItemsToContainerAsync()
        {
            // Create a family object for the Andersen family
            Family andersenFamily = new Family
            {
                Id = "Andersen.1",
                PartitionKey = "Andersen",
                LastName = "Andersen",
                Parents = new Parent[]
                {
                    new Parent {FirstName = "Thomas"},
                    new Parent {FirstName = "Mary Kay"}
                },
                Children = new Child[]
                {
                    new Child
                    {
                        FirstName = "Henriette Thaulow",
                        Gender = "female",
                        Grade = 5,
                        Pets = new Pet[]
                        {
                            new Pet {GivenName = "Fluffy"}
                        }
                    }
                },
                Address = new Address {State = "WA", County = "King", City = "Seattle"},
                IsRegistered = false
            };

            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Family> andersenFamilyResponse =
                    await this.container.ReadItemAsync<Family>(andersenFamily.Id,
                        new PartitionKey(andersenFamily.PartitionKey));
                //Console.WriteLine("Item in database with id: {0} already exists\n", andersenFamilyResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                ItemResponse<Family> andersenFamilyResponse =
                    await this.container.CreateItemAsync<Family>(andersenFamily,
                        new PartitionKey(andersenFamily.PartitionKey));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                //Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", andersenFamilyResponse.Resource.Id, andersenFamilyResponse.RequestCharge);
            }

            // Create a family object for the Wakefield family
            Family wakefieldFamily = new Family
            {
                Id = "Wakefield.7",
                PartitionKey = "Wakefield",
                LastName = "Wakefield",
                Parents = new Parent[]
                {
                    new Parent {FamilyName = "Wakefield", FirstName = "Robin"},
                    new Parent {FamilyName = "Miller", FirstName = "Ben"}
                },
                Children = new Child[]
                {
                    new Child
                    {
                        FamilyName = "Merriam",
                        FirstName = "Jesse",
                        Gender = "female",
                        Grade = 8,
                        Pets = new Pet[]
                        {
                            new Pet {GivenName = "Goofy"},
                            new Pet {GivenName = "Shadow"}
                        }
                    },
                    new Child
                    {
                        FamilyName = "Miller",
                        FirstName = "Lisa",
                        Gender = "female",
                        Grade = 1
                    }
                },
                Address = new Address {State = "NY", County = "Manhattan", City = "NY"},
                IsRegistered = true
            };

            try
            {
                // Read the item to see if it exists
                ItemResponse<Family> wakefieldFamilyResponse =
                    await this.container.ReadItemAsync<Family>(wakefieldFamily.Id,
                        new PartitionKey(wakefieldFamily.PartitionKey));
                //Console.WriteLine("Item in database with id: {0} already exists\n", wakefieldFamilyResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Wakefield family. Note we provide the value of the partition key for this item, which is "Wakefield"
                ItemResponse<Family> wakefieldFamilyResponse =
                    await this.container.CreateItemAsync<Family>(wakefieldFamily,
                        new PartitionKey(wakefieldFamily.PartitionKey));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                //Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", wakefieldFamilyResponse.Resource.Id, wakefieldFamilyResponse.RequestCharge);
            }
        }

        // Run a query (using Azure Cosmos DB SQL syntax) against the container
        // Including the partition key value of lastName in the WHERE filter results in a more efficient query
        private async Task QueryItemsAsync()
        {
            var sqlQueryText = "SELECT * FROM c WHERE c.partitionKey = 'Andersen'";

            //Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Family> queryResultSetIterator = this.container.GetItemQueryIterator<Family>(queryDefinition);

            List<Family> families = new List<Family>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Family> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Family family in currentResultSet)
                {
                    families.Add(family);
                    //Console.WriteLine("\tRead {0}\n", family);
                }
            }
        }

        private async Task ReadItemsToContainerAsync(Family andersenFamily)
        {
            ItemResponse<Family> andersenFamilyResponse =
                await this.container.ReadItemAsync<Family>(andersenFamily.Id,
                    new PartitionKey(andersenFamily.PartitionKey));
        }

        public async Task<IEnumerable<Family>> GetAllRecords()
        {
            var sqlQueryText = "SELECT * FROM c ";


            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Family> queryResultSetIterator = this.container.GetItemQueryIterator<Family>(queryDefinition);

            List<Family> families = new List<Family>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Family> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Family family in currentResultSet)
                {
                    families.Add(family);
                }
            }

            return families;
        }

        public async Task<IEnumerable<Family>> GetRecords(string partitionKey)
        {
            var sqlQueryText = $"SELECT * FROM c where c.partitionKey='{partitionKey}'";


            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Family> queryResultSetIterator = this.container.GetItemQueryIterator<Family>(queryDefinition);

            List<Family> families = new List<Family>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Family> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Family family in currentResultSet)
                {
                    families.Add(family);
                }
            }

            return families;
        }

        // Add Family items to the container
        public async Task AddItemsToContainerAsync(Family andersenFamily)
        {
            ItemResponse<Family> andersenFamilyResponse =
                await this.container.CreateItemAsync<Family>(andersenFamily,
                    new PartitionKey(andersenFamily.PartitionKey));
        }

        // Replace an item in the container
        public async Task ReplaceFamilyItemAsync(Family newFamily, string partitionKey)
        {
            ItemResponse<Family> wakefieldFamilyResponse =
                await this.container.ReadItemAsync<Family>(newFamily.Id, new PartitionKey(partitionKey));
            var itemBody = wakefieldFamilyResponse.Resource;

            // update registration status from false to true
            itemBody.IsRegistered = newFamily.IsRegistered;
            // update grade of child
            itemBody.Children[0].Grade = newFamily.Children[0].Grade;

            // replace the item with the updated content
            wakefieldFamilyResponse =
                await this.container.ReplaceItemAsync<Family>(itemBody, itemBody.Id,
                    new PartitionKey(itemBody.PartitionKey));
        }

        // Delete an item in the container
        public async Task DeleteFamilyItemAsync(string partitionKeyValue, string familyId)
        {
            // Delete an item. Note we must provide the partition key value and id of the item to delete
            ItemResponse<Family> wakefieldFamilyResponse =
                await this.container.DeleteItemAsync<Family>(familyId, new PartitionKey(partitionKeyValue));
        }
    }
}