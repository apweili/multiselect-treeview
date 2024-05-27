using System.Collections.Generic;
using System.Windows.Enums;

namespace System.Windows.Models
{
    public interface IAutoBindExpandableModel
    {
        IAutoBindExpandableModel Parent { get; }
        IEnumerable<IAutoBindExpandableModel> Children { get; }
        bool IsExpanded { get; set; }
        
        //todo remove
        SelectionCheckState SelectionCheckState { get; set; }
    }
}