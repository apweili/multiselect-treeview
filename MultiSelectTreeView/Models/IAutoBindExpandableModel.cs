using System.Collections.Generic;

namespace System.Windows.Models
{
    public interface IAutoBindExpandableModel
    {
        IAutoBindExpandableModel Parent { get; }
        IEnumerable<IAutoBindExpandableModel> Children { get; }
        bool IsExpanded { get; set; }
    }
}