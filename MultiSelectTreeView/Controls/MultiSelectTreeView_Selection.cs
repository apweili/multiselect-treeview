using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Helpers;

namespace System.Windows.Controls
{
    public partial class MultiSelectTreeView
    {
        /// <summary>
        ///     The key needed set a read-only property.
        /// </summary>
        private static readonly DependencyPropertyKey InternalSelectedItemsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "InternalSelectedItems",
                typeof(IList),
                typeof(MultiSelectTreeView),
                new FrameworkPropertyMetadata(
                    (IList)null));

        /// <summary>
        /// A read-only IList containing the currently selected items
        /// </summary>
        internal static readonly DependencyProperty InternalSelectedItemsImplProperty =
            InternalSelectedItemsPropertyKey.DependencyProperty;

        public static DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            "SelectedItems",
            typeof(IList),
            typeof(MultiSelectTreeView),
            new FrameworkPropertyMetadata(
                null, OnSelectedItemsPropertyChanged));

        /// <summary>
        /// Gets or sets a list of selected items and can be bound to another list. If the source list
        /// implements <see cref="INotifyPropertyChanged"/> the changes are automatically taken over.
        /// </summary>
        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public IList InternalSelectedItems
        {
            get { return (IList)GetValue(InternalSelectedItemsImplProperty); }
        }

        /// <summary>
        ///     SelectedValuePath DependencyProperty
        /// </summary>
        public static readonly DependencyProperty SelectedValuePathProperty =
            Selector.SelectedValuePathProperty.AddOwner
            (
                typeof(MultiSelectTreeView),
                new FrameworkPropertyMetadata(
                    String.Empty,
                    new PropertyChangedCallback(OnSelectedValuePathChanged)));

        /// <summary>
        ///  The path used to retrieve the SelectedValue from the SelectedItem
        /// </summary>
        [Bindable(true), Category("Appearance")]
        [Localizability(LocalizationCategory.NeverLocalize)] // not localizable
        public string SelectedValuePath
        {
            get => (string)GetValue(SelectedValuePathProperty);
            set => SetValue(SelectedValuePathProperty, value);
        }

        private void UpdateSelectedValueByPath(string newValuePath)
        {
            if (SelectionMode == TreeViewSelectionMode.MultiSelectEnabled || InternalSelectedItems.Count != 1)
            {
                return;
            }

            var selectedItem = InternalSelectedItems.First();
            SelectedValue = PropertyPathHelper.GetObjectByPropertyPath(selectedItem, newValuePath);
        }

        private static void OnSelectedValuePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeView = d as MultiSelectTreeView;
            if (treeView == null)
            {
                return;
            }

            treeView.UpdateSelectedValueByPath(e.NewValue as string);
        }

        /// <summary>
        ///     SelectedValue DependencyProperty
        /// </summary>
        public static readonly DependencyProperty SelectedValueProperty = Selector.SelectedValueProperty.AddOwner(
            typeof(MultiSelectTreeView),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                new PropertyChangedCallback(OnSelectedValueChanged),
                new CoerceValueCallback(CoerceSelectedValue)));

        /// <summary>
        ///  The value of the SelectedItem, obtained using the SelectedValuePath.
        /// </summary>
        /// <remarks>
        /// <p>Setting SelectedValue to some value x attempts to select an item whose
        /// "value" evaluates to x, using the current setting of <seealso cref="SelectedValuePath"/>.
        /// If no such item can be found, the selection is cleared.</p>
        ///
        /// <p>Getting the value of SelectedValue returns the "value" of the <seealso cref="SelectedItem"/>,
        /// using the current setting of <seealso cref="SelectedValuePath"/>, or null
        /// if there is no selection.</p>
        ///
        /// <p>Note that these rules imply that getting SelectedValue immediately after
        /// setting it to x will not necessarily return x.  It might return null,
        /// if no item with value x can be found.</p>
        /// </remarks>
        [Bindable(true), Category("Appearance"),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedValue
        {
            get => GetValue(SelectedValueProperty);
            set => SetValue(SelectedValueProperty, value);
        }

        /// <summary>
        /// This could happen when SelectedValuePath has changed,
        /// SelectedItem has changed, or someone is setting SelectedValue.
        /// </summary>
        private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private static object CoerceSelectedValue(DependencyObject d, object basevalue)
        {
            var treeView = d as MultiSelectTreeView;
            if (treeView == null)
            {
                return null;
            }

            if (treeView.SelectionMode == TreeViewSelectionMode.MultiSelectEnabled)
            {
                return null;
            }

            if (treeView.TryToFindSelectedItem(basevalue))
            {
                return basevalue;
            }

            return null;
        }

        private bool TryToFindSelectedItem(object selectedValue)
        {
            if (selectedValue == null)
            {
                return false;
            }

            var valuePath = SelectedValuePath;
            var items = GetAllItems();
            if (string.IsNullOrEmpty(valuePath))
            {
                return items.FirstOrDefault(selectedValue.Equals) != null;
            }

            var selectedItem = items.FirstOrDefault(item => item != null &&
                                                            selectedValue.Equals(
                                                                PropertyPathHelper.GetObjectByPropertyPath(item,
                                                                    valuePath)));
            if (selectedItem == null)
            {
                return false;
            }

            InternalSelectedItems.Clear();
            InternalSelectedItems.Add(selectedItem);
            return true;
        }

        private void SyncInternalAndExternalSelectedItems()
        {
            if (SelectedItems == null)
            {
                InternalSelectedItems.Clear();
                return;
            }

            var externalSelectedItems = SelectedItems.Cast<object>().ToList();
            InternalSelectedItems.Clear();
            SelectedItems.Clear();
            foreach (var item in externalSelectedItems)
            {
                InternalSelectedItems.Add(item);
            }
        }

        private static void OnSelectedItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeView = (MultiSelectTreeView)d;
            if (e.OldValue != null)
            {
                var collection = e.OldValue as INotifyCollectionChanged;
                if (collection != null)
                {
                    collection.CollectionChanged -= treeView.OnExternalSelectedItemsCollectionChanged;
                }
            }

            if (e.NewValue != null)
            {
                var collection = e.NewValue as INotifyCollectionChanged;
                if (collection != null)
                {
                    collection.CollectionChanged += treeView.OnExternalSelectedItemsCollectionChanged;
                }
            }

            treeView.SyncInternalAndExternalSelectedItems();
        }

        private bool IsSyncInternalAndExternalSelectedItems { get; set; }

        private void OnInternalSelectedItemsChangedForExternalSelectedItems(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            var selectedItems = SelectedItems as ObservableCollection<object>;
            if (selectedItems == null)
            {
                return;
            }

            UpdateSelectedItems(selectedItems, e);
        }

        private void OnExternalSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var selectedItems = (ObservableCollection<object>)InternalSelectedItems;
            UpdateSelectedItems(selectedItems, e);
        }

        private void UpdateSelectedItems(ObservableCollection<object> selectedItems, NotifyCollectionChangedEventArgs e)
        {
            if (IsSyncInternalAndExternalSelectedItems)
            {
                return;
            }

            try
            {
                IsSyncInternalAndExternalSelectedItems = true;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems)
                        {
                            selectedItems.Add(item);
                        }

                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems)
                        {
                            selectedItems.Remove(item);
                        }

                        break;
                    case NotifyCollectionChangedAction.Reset:
                        selectedItems.Clear();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            finally
            {
                IsSyncInternalAndExternalSelectedItems = false;
            }
        }

        // this eventhandler reacts on the firing control to, in order to update the own status
        private void OnInternalSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var addedItems = new ArrayList();
            var removedItems = new ArrayList();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
#if DEBUG
                    // Make sure we don't confuse MultiSelectTreeViewItems and their DataContexts while development
                    if (e.NewItems.OfType<MultiSelectTreeViewItem>().Any())
                        throw new ArgumentException(
                            "A MultiSelectTreeViewItem instance was added to the SelectedItems collection. Only their DataContext instances must be added to this list!");
#endif
                    object last = null;
                    foreach (var item in GetTreeViewItemsFor(e.NewItems))
                    {
                        if (!item.IsSelected)
                        {
                            item.IsSelected = true;
                        }

                        last = item.DataContext;
                    }

                    addedItems.AddRange(e.NewItems);
                    LastSelectedItem = last;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in GetTreeViewItemsFor(e.OldItems))
                    {
                        item.IsSelected = false;
                        if (item.DataContext == LastSelectedItem)
                        {
                            if (InternalSelectedItems.Count > 0)
                            {
                                LastSelectedItem = InternalSelectedItems[InternalSelectedItems.Count - 1];
                            }
                            else
                            {
                                LastSelectedItem = null;
                            }
                        }
                    }

                    removedItems.AddRange(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in RecursiveTreeViewItemEnumerable(this, true))
                    {
                        if (item.IsSelected)
                        {
                            removedItems.Add(item.DataContext);
                            item.IsSelected = false;
                        }
                    }

                    LastSelectedItem = null;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var selectionChangedEventArgs =
                new SelectionChangedEventArgs(SelectionChangedEvent, addedItems, removedItems);

            OnSelectionChanged(selectionChangedEventArgs);
        }
    }
}