using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace google_dialog.Specifications
{
    public interface ISpecification<TElement>
        where TElement : ITableEntity, new()
    {
        public TableQuery<TElement> ToTableQuery();
    }
}
