using System.Collections.Generic;
using System.Windows.Interfaces;
using System.Windows.Models;

namespace Demo.ViewModel
{
    public class FamilyProfiles : IAutoBindExpandableModel, IAutoBindImageSourceModel
    {
        public string Name { get; set; }
        public IAutoBindExpandableModel Parent { get; } = null;
        public IEnumerable<IAutoBindExpandableModel> Children { get; set; }
        public bool IsExpanded { get; set; }
        public object ImageSource { get; set; } = null; //= "C:\\Users\\lw\\Desktop\\20120628172839.jpg";
    }
}