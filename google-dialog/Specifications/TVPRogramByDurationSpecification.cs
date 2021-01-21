using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace google_dialog.Specifications
{
    public class TVPRogramByDurationSpecification : SpecificationBase<TVProgram>
    {
        private int minDuration;

        private TVPRogramByDurationSpecification(int minDuration)
        {
            this.minDuration = minDuration;
        }

        public static TVPRogramByDurationSpecification For(int minDuration)
        {
            return new TVPRogramByDurationSpecification(minDuration);
        }

        protected override string Execute()
        {
            return TableQuery.GenerateFilterConditionForInt(nameof(TVProgram.LengthInMinutes), QueryComparisons.GreaterThanOrEqual, this.minDuration);
        }
    }
}
