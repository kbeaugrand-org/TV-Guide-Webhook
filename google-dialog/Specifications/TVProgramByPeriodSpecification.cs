using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace google_dialog.Specifications
{
    public class TVProgramByPeriodSpecification : SpecificationBase<TVProgram>
    {
        private string period;

        private TVProgramByPeriodSpecification(string period)
        {
            this.period = period;
        }

        public static TVProgramByPeriodSpecification For(string period)
        {
            return new TVProgramByPeriodSpecification(period);
        }

        protected override string Execute()
        {
            DateTimeOffset startDate = DateTimeOffset.UtcNow;
            DateTimeOffset endDate = DateTimeOffset.UtcNow.AddHours(2);

            switch (period)
            {
                case "tonight":
                    startDate = new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, DateTimeOffset.UtcNow.Day, 19, 40, 0, DateTimeOffset.UtcNow.Offset);
                    endDate = new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, DateTimeOffset.UtcNow.Day, 20, 39, 59, DateTimeOffset.UtcNow.Offset);
                    break;

            }

            return TableQuery.CombineFilters(
                                    TableQuery.GenerateFilterConditionForDate(nameof(TVProgram.Start), QueryComparisons.GreaterThanOrEqual, startDate),
                                    TableOperators.And,
                                    TableQuery.GenerateFilterConditionForDate(nameof(TVProgram.Start), QueryComparisons.LessThanOrEqual, endDate));
        }
    }
}
