using Microsoft.WindowsAzure.Storage.Table;

namespace google_dialog.Specifications
{
    public class TVProgramByChannelSpecification: SpecificationBase<TVProgram>
    {
        private readonly string channel;

        private TVProgramByChannelSpecification(string channel)
        {
            this.channel = channel;
        }

        public static TVProgramByChannelSpecification For(string channel)
        {
            return new TVProgramByChannelSpecification(channel);
        }

        protected override string Execute()
        {
            return TableQuery.GenerateFilterCondition(nameof(TVProgram.Channel), QueryComparisons.Equal, this.channel);
        }
    }
}
