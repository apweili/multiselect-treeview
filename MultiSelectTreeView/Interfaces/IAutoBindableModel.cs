using System.Collections.Generic;

namespace System.Windows.Interfaces
{
    public interface IAutoBindableModel
    {
        IEnumerable<IAutoBindableModel> Children { get; set; }
        bool IsExpanded { get; set; }
        object Header { get; set; }
    }
}