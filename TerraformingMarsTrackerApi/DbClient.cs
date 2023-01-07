using Microsoft.Azure.Cosmos;
using Azure.Identity;

namespace TerraformingMarsTrackerApi
{
    public class DbClient
    {
        public async Task<Root> Connect()
        {

            using CosmosClient client = new(
                accountEndpoint: "https://pharylon-1.documents.azure.com",
                "sGS8Pnnir4IQQSMk9ftIXGiJNTSHnb6TvdLZ39XTvGaQpsPSWNc0krs994G4brmSopzlchnIBx3qhWkELHHTog=="
            );
            Database database = client.GetDatabase("fantasy-calendar");
            Container container = database.GetContainer("tm-tracker");
            //var pk = new PartitionKey("id");
            //Product item = new(
            //    id: "68719518388",
            //    category: "gear-surf-surfboards",
            //    name: "Sunnox Surfboard",
            //    quantity: 8,
            //    sale: true
            //);


            //Product replacedItem = await container.CreateItemAsync<Product>(
            //    item: item
            //);


            var item = await container.ReadItemAsync<Root>("ddd11c8c-208c-492a-8709-2f0046b55bcc", new PartitionKey("ddd11c8c-208c-492a-8709-2f0046b55bcc"));
            //var query = new QueryDefinition(
            //    query: "SELECT * FROM c  where c.id = @id"
            //).WithParameter("@id", "ddd11c8c-208c-492a-8709-2f0046b55bcc");

            //Root item2 = new Root();

            //using (FeedIterator<Root> feed = container.GetItemQueryIterator<Root>(queryDefinition: query))
            //{
            //    while (feed.HasMoreResults)
            //    {
            //        FeedResponse<Root> response = await feed.ReadNextAsync();
            //        foreach (Root item in response)
            //        {
            //            Console.WriteLine($"Found item:\t{item.name}");
            //            item2 = item;
            //        }
            //    }

            //}

            return new Root();


        }
    }

    public record Product(
    string id,
    string category,
    string name,
    int quantity,
    bool sale
);

    public class FantasyDate
    {
        public int year { get; set; }
        public int dayOfMonth { get; set; }
        public int month { get; set; }
    }

    public class Root
    {
        public string id { get; set; }
        public string calendarId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public DateTime realDate { get; set; }
        public FantasyDate fantasyDate { get; set; }
        public string type { get; set; }
        public string _rid { get; set; }
        public string _self { get; set; }
        public string _etag { get; set; }
        public string _attachments { get; set; }
        public bool hidden { get; set; }
        public string createUser { get; set; }
        public int _ts { get; set; }
    }


}
