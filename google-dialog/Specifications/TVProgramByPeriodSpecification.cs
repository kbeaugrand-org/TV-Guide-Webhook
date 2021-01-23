using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace google_dialog.Specifications
{
    public class TVProgramByPeriodSpecification : SpecificationBase<TVProgram>
    {
        private DateTimeOffset dateTime;

        private TVProgramByPeriodSpecification(DateTimeOffset dateTime)
        {
            this.dateTime = dateTime;
        }

        public static TVProgramByPeriodSpecification For(DateTimeOffset dateTime)
        {
            return new TVProgramByPeriodSpecification(dateTime);
        }

        protected override string Execute()
        {
            DateTimeOffset startDate = dateTime;
            DateTimeOffset endDate = dateTime.AddHours(2);

            return TableQuery.CombineFilters(
                                    TableQuery.GenerateFilterConditionForDate(nameof(TVProgram.Start), QueryComparisons.GreaterThanOrEqual, startDate),
                                    TableOperators.And,
                                    TableQuery.GenerateFilterConditionForDate(nameof(TVProgram.Start), QueryComparisons.LessThanOrEqual, endDate));
        }
    }
}
