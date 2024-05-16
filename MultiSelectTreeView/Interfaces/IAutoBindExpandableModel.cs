using System.Collections.Generic;

namespace System.Windows.Interfaces
{
    public interface IAutoBindExpandableModel
    {
        IEnumerable<IAutoBindExpandableModel> Children { get; set; }
        bool IsExpanded { get; set; }
    }
}