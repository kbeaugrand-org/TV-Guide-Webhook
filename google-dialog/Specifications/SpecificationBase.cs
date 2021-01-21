using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace google_dialog.Specifications
{
    public abstract class SpecificationBase<TElement>: ISpecification<TElement>
        where TElement : ITableEntity, new()
    {
        protected abstract string Execute();

        public TableQuery<TElement> ToTableQuery()
        {
            return new TableQuery<TElement>()
                        .Where(this.Execute());
        }
        public AndSpecification<TElement> And(SpecificationBase<TElement> specification)
        {
            return new AndSpecification<TElement>(this.Execute(), specification.Execute());
        }
    }
}
