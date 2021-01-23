using google_dialog.Extensions;
using google_dialog.Specifications;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace google_dialog.Repositories
{
    public class TVProgramRepository : CloudTableRepository<TVProgram>
    {
        public TVProgramRepository()
            : base(Constants.ProgramsTableName)
        {

        }

        public async Task<IEnumerable<TVProgram>> SearchPrograms(DateTimeOffset dateTime, string channelName)
        {
            var searchSpecification = TVProgramByPeriodSpecification.For(dateTime)
                                .And(TVPRogramByDurationSpecification.For(30));

            if (!String.IsNullOrWhiteSpace(channelName))
            {
                searchSpecification = searchSpecification
                                        .And(TVProgramByChannelSpecification.For(channelName));
            }

            var tableQuery = searchSpecification
                                .ToTableQuery();

            var querySegment = await base.cloudTable
                                         .ExecuteQuerySegmentedAsync<TVProgram>(tableQuery, new TableContinuationToken());

            return querySegment.ToList();
        }
    }
}
