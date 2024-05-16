using System.Collections.Generic;
using System.Windows.Interfaces;

namespace Demo.ViewModel
{
    public class FamilyProfiles : IAutoBindExpandableModel, IAutoBindImageSourceModel
    {
        public string Name { get; set; }
        public IEnumerable<IAutoBindExpandableModel> Children { get; set; }
        public bool IsExpanded { get; set; }
        public object ImageSource { get; set; } = "C:\\Users\\lw\\Desktop\\20120628172839.jpg";
    }
}