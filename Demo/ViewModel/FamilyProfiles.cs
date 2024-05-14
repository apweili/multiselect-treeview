using System.Collections.Generic;
using System.Windows.Interfaces;

namespace Demo.ViewModel
{
    public class FamilyProfiles : IAutoBindableModel
    {
        public string Name { get; set; }
        public IEnumerable<IAutoBindableModel> Children { get; set; }
        public bool IsExpanded { get; set; }
    }
}