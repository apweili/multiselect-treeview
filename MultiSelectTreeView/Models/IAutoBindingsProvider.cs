using System.Collections.Generic;
using System.Windows.Data;

namespace System.Windows.Models
{
    public interface IAutoBindingsProvider
    {
        IEnumerable<BindingInfo> GetBindingInfos { get; }
    }

    public class BindingInfo
    {
        public Binding Binding { get; set; }
        public DependencyProperty DependencyProperty { get; set; }
    }
}