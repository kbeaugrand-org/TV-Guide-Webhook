using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace google_dialog.Repositories
{
    public abstract class CloudTableRepository<TElement>
        where TElement : ITableEntity, new()
    {
        protected readonly CloudTable cloudTable;

        protected CloudTableRepository(string tableName)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(Constants.StorageAccountConnectionString);
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();

            cloudTable = tableClient.GetTableReference(tableName);
        }
    }
}
