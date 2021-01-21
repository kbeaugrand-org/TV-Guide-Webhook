using Microsoft.WindowsAzure.Storage.Table;

namespace google_dialog.Specifications
{
    public class AndSpecification<TElement> : SpecificationBase<TElement>
        where TElement : ITableEntity, new()
    {
        private string specificationA;
        private string specificationB;

        public AndSpecification(string specificationA, string specificationB)
        {
            this.specificationA = specificationA;
            this.specificationB = specificationB;
        }

        protected override string Execute()
        {
            return TableQuery.CombineFilters(specificationA, TableOperators.And, specificationB);
        }
    }
}
