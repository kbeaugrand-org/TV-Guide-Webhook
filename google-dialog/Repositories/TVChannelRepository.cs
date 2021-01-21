using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace google_dialog.Repositories
{
    public class TVChannelRepository: CloudTableRepository<TVProgram>
    {
        public TVChannelRepository()
            : base(Constants.ChannelsTableName)
        {

        }

        public async Task<IEnumerable<TVChannel>> SearchChannels()
        {
            var continuationToken = new TableContinuationToken();

            var querySegment = await cloudTable.ExecuteQuerySegmentedAsync(new TableQuery<TVChannel>(), continuationToken);

            return querySegment.ToList();
        }
    }
}
