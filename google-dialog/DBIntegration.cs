using google_dialog.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace google_dialog
{
    public static class DBIntegration
    {
        [FunctionName("DBIntegration")]
        public static async Task Run([TimerTrigger("0 0 0 * * *")]TimerInfo timer, ILogger log)
        {
            var elements = await LoadDatabase();
            await PopulateTables(elements);
        }

        public static async Task PopulateTables(XElement elements)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(Constants.StorageAccountConnectionString);
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();

            await PopulateChannels(tableClient, elements);
            await PopulatePrograms(tableClient, elements);
        }

        public static async Task PopulateChannels(CloudTableClient tableClient, XElement elements)
        {
            CloudTable cloudTable = tableClient.GetTableReference(Constants.ChannelsTableName);
            await cloudTable.CreateIfNotExistsAsync();

            TableBatchOperation batch;

            int page = 0;
            int pageSize = 20;

            do
            {
                batch = new TableBatchOperation();                   

                foreach (var item in elements.Descendants("channel")
                                            .Skip(page * pageSize)
                                            .Take(pageSize))
                {

                    var entity = item.ToTVChannel();

                    batch.Add(TableOperation.InsertOrReplace(entity));
                }

                var result = await cloudTable.ExecuteBatchAsync(batch);

                page++;
            }
            while (batch.Count == pageSize);
        }

        public static async Task PopulatePrograms(CloudTableClient tableClient, XElement elements)
        {
            CloudTable cloudTable = tableClient.GetTableReference(Constants.ProgramsTableName);

            await cloudTable.CreateIfNotExistsAsync();

            TableBatchOperation batch;

            var groups = elements.Descendants("programme")
                                    .Select(c => c.ToTVProgram())
                                    .GroupBy(c => c.PartitionKey);

            foreach(var group in groups)
            {
                int page = 0;
                int pageSize = 50;

                do
                {
                    batch = new TableBatchOperation();

                    try
                    {
                        foreach (var item in group.Skip(page * pageSize).Take(pageSize))
                        {
                            batch.Add(TableOperation.InsertOrReplace(item));
                        }

                        if (batch.Count > 0)
                        {
                            var result = await cloudTable.ExecuteBatchAsync(batch);
                        }
                    }
                    catch(Exception e)
                    {

                    }
                    finally
                    {
                        page++;
                    }
                }
                while (batch.Count == pageSize);
            }           
        }

        public static async Task<XElement> LoadDatabase()
        {
            using (var handler = new HttpClientHandler())
            {
                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri("https://xmltv.ch");

                    using (var fileStream = await client.GetStreamAsync("/xmltv/xmltv-tnt.xml"))
                    {
                        return await XElement.LoadAsync(fileStream, LoadOptions.None, CancellationToken.None);
                    }                    
                }
            }           
        }
    }
}
