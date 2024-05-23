using System.Windows.Controls;
using System.Windows.Extensions;
using System.Windows.Interfaces;
using System.Windows.Models;

namespace System.Windows.Helpers
{
    internal static class AutoBindHelper
    {
        public static void TryToAutoBindObject(DependencyObject container, object viewModel)
        {
            var itemContainerControl = container as MultiSelectTreeViewItem;
            if (itemContainerControl == null)
            {
                return;
            }

            var autoBindableModel = viewModel as IAutoBindExpandableModel;
            if (autoBindableModel != null)
            {
                autoBindableModel.BindExpandableToContainer(itemContainerControl);
            }

            var modelWithImageSource = viewModel as IAutoBindImageSourceModel;
            if (modelWithImageSource != null)
            {
                modelWithImageSource.BindImageSourceToContainer(itemContainerControl);
            }

            var bindingProvider = viewModel as IAutoBindingsProvider;
            if (bindingProvider != null)
            {
                bindingProvider.AddBindingsToContainer(itemContainerControl);
            }
        }
    }
}