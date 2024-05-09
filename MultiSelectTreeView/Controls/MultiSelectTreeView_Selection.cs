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
        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectionChanged",
            RoutingStrategy.Bubble,
            typeof(SelectionChangedEventHandler),
            typeof(MultiSelectTreeView));

        public static readonly RoutedEvent PreviewSelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "PreviewSelectionChanged",
            RoutingStrategy.Bubble,
            typeof(PreviewSelectionChangedEventHandler),
            typeof(MultiSelectTreeView));

        public event SelectionChangedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        public event PreviewSelectionChangedEventHandler PreviewSelectionChanged
        {
            add { AddHandler(PreviewSelectionChangedEvent, value); }
            remove { RemoveHandler(PreviewSelectionChangedEvent, value); }
        }

        /// <summary>
        ///     SelectedItem DependencyProperty
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty =
            Selector.SelectedItemProperty.AddOwner(typeof(MultiSelectTreeView), new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                new PropertyChangedCallback(OnSelectedItemChanged),
                new CoerceValueCallback(CoerceSelectedItem)));

        private static object CoerceSelectedItem(DependencyObject d, object baseValue)
        {
            var treeView = (ItemsControl)d;
            if (treeView == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return treeView.Items.Contains(baseValue) ? baseValue : DependencyProperty.UnsetValue;
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeView = (MultiSelectTreeView)d;
            if (treeView == null)
            {
                return;
            }

            treeView.SelectItem(e.NewValue);
        }

        /// <summary>
        ///  The first item in the current selection, or null if the selection is empty.
        /// </summary>
        [Bindable(true), Category("Appearance"),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static DependencyPropertyKey LastSelectedItemPropertyKey = DependencyProperty.RegisterReadOnly(
            "LastSelectedItem",
            typeof(object),
            typeof(MultiSelectTreeView),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty LastSelectedItemProperty =
            LastSelectedItemPropertyKey.DependencyProperty;

        /// <summary>
        ///    Gets the last selected item.
        /// </summary>
        public object LastSelectedItem
        {
            get { return GetValue(LastSelectedItemProperty); }
            private set { SetValue(LastSelectedItemPropertyKey, value); }
        }

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
        
        private bool IsItemsInitialized { get; set; }

        internal IList InternalSelectedItems
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
            get { return (string)GetValue(SelectedValuePathProperty); }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        private void UpdateSelectedValueByPath(string newValuePath)
        {
            if (InternalSelectedItems.Count == 0)
            {
                return;
            }

            var selectedItem = InternalSelectedItems.Last();
            SelectedValue = PropertyPathHelper.GetObjectByPropertyPath(selectedItem, newValuePath);
        }

        private static void OnSelectedValuePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeView = d as MultiSelectTreeView;
            if (treeView == null)
            {
                return;
            }

            if (treeView.SelectionMode == TreeViewSelectionMode.MultiSelectEnabled)
            {
                return;
                // throw new InvalidOperationException("SingleSelectOnly support SelectedValueProperty");
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
            get { return GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        /// <summary>
        /// This could happen when SelectedValuePath has changed,
        /// SelectedItem has changed, or someone is setting SelectedValue.
        /// </summary>
        private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private static object CoerceSelectedValue(DependencyObject d, object baseValue)
        {
            var treeView = d as MultiSelectTreeView;
            if (treeView == null)
            {
                return null;
            }

            if (treeView.SelectionMode == TreeViewSelectionMode.MultiSelectEnabled)
            {
                return null;
                // throw new InvalidOperationException("SingleSelectOnly support SelectedValueProperty");
            }

            if (!treeView.TryToUpdateSelectedItemBySelectedValue(baseValue))
            {
                return null;
            }

            return baseValue;
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            IsItemsInitialized = true;
        }

        /// <summary>
        ///     SelectedIndex DependencyProperty
        /// </summary>
        public static readonly DependencyProperty SelectedIndexProperty = Selector.SelectedIndexProperty.AddOwner(
            typeof(MultiSelectTreeView), new FrameworkPropertyMetadata(
                -1,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                new PropertyChangedCallback(OnSelectedIndexChanged),
                new CoerceValueCallback(CoerceSelectedIndex)));

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeView = (MultiSelectTreeView)d;
            if (treeView == null)
            {
                return;
            }
            
            var items = treeView.Items;
            var newValue = (int)e.NewValue;
            treeView.UnSelectAllItem();
            if (newValue > -1)
            {
                treeView.SelectItem(items[newValue]); 
            }
        }

        private static object CoerceSelectedIndex(DependencyObject d, object baseValue)
        {
            var treeView = (ItemsControl)d;
            if ((baseValue is int) && (int)baseValue >= treeView.Items.Count)
            {
                return DependencyProperty.UnsetValue;
            }

            return baseValue;
        }

        private bool TryToUpdateSelectedItemBySelectedValue(object selectedValue)
        {
            object selectedItem;
            if (selectedValue == null)
            {
                selectedItem = null;
            }
            else
            {
                var valuePath = SelectedValuePath;
                var items = GetAllItems();
                if (string.IsNullOrEmpty(valuePath))
                {
                    selectedItem = items.FirstOrDefault(selectedValue.Equals);
                }
                else
                {
                    selectedItem = items.FirstOrDefault(item => item != null &&
                                                                selectedValue.Equals(
                                                                    PropertyPathHelper.GetObjectByPropertyPath(item,
                                                                        valuePath)));
                }
            }

            if (selectedItem != null && IsItemSelected(selectedItem))
            {
                return true;
            }

            var lastSelectedItem = InternalSelectedItems.Count > 0 ? InternalSelectedItems.Last() : null;
            UnSelectAllItem();
            if (selectedItem != null)
            {
                SelectItem(selectedItem);
            }

            LastSelectedItem = lastSelectedItem;
            return true;
        }

        private bool _isUpdatingSelectedItems;
        private bool IsUpdatingSelectedItems => _isUpdatingSelectedItems;

        private void SyncSelectedInfoWhileUpdatingSelectedItem()
        {
            object selectedItem;
            if (InternalSelectedItems.Count == 0)
            {
                selectedItem = null;
            }
            else
            {
                selectedItem = InternalSelectedItems.Last(); 
            }

            SelectedItem = selectedItem;
            if (selectedItem != null)
            {
                SelectedIndex = Items.IndexOf(selectedItem);
                SelectedValue = PropertyPathHelper.GetObjectByPropertyPath(selectedItem,
                    SelectedValuePath); 
            }
            else
            {
                SelectedIndex = -1;
                SelectedValue = null;
            }
        }

        private void SyncInternalByExternalSelectedItems()
        {
            if (SelectedItems == null)
            {
                return;
            }

            var externalSelectedItems = SelectedItems.Cast<object>().ToList();
            SelectedItems.Clear();
            foreach (var item in externalSelectedItems)
            {
                SelectItem(item);
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

            treeView.SyncInternalByExternalSelectedItems();
        }

        private bool IsSyncInternalAndExternalSelectedItems { get; set; }

        private void SyncExternalSelectedItems(object sender,
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
        
         private void OnInternalSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsUpdatingSelectedItems)
            {
                return;
            }

            try
            {
                _isUpdatingSelectedItems = true;
                SyncExternalSelectedItems(sender, e);
                OnInternalSelectedItemsChangedCore(sender, e);
            }
            finally
            {
                _isUpdatingSelectedItems = false;
            }
            
        }

        // this eventhandler reacts on the firing control to, in order to update the own status
        private void OnInternalSelectedItemsChangedCore(object sender, NotifyCollectionChangedEventArgs e)
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

            SyncSelectedInfoWhileUpdatingSelectedItem();
            var selectionChangedEventArgs =
                new SelectionChangedEventArgs(SelectionChangedEvent, addedItems, removedItems);
            OnSelectionChanged(selectionChangedEventArgs);
        }

        public void SelectItem(object item)
        {
            if (SelectionMode == TreeViewSelectionMode.SingleSelectOnly)
            {
                SelectItemInSingleSelectionMode(item);
                return;
            }
            
            SelectItemInMultiSelectionMode(item); 
        }
        
        private void SelectItemInMultiSelectionMode(object item)
        {
            if (IsUpdatingSelectedItems)
            {
                return;
            }

            if (IsItemSelected(item))
            {
                return;
            }
            
            InternalSelectedItems.Add(item);
        }
        
        private void SelectItemInSingleSelectionMode(object item)
        {
            if (IsUpdatingSelectedItems)
            {
                return;
            }
 
            InternalSelectedItems.Clear();
            InternalSelectedItems.Add(item);
        }

        public void UnSelectItem(object item)
        {
            if (IsUpdatingSelectedItems)
            {
                return;
            }

            var index = InternalSelectedItems.IndexOf(item);
            if (index < 0)
            {
                return;
            }
            
            InternalSelectedItems.RemoveAt(index);
        }

        public void UnSelectAllItem()
        {
            if (IsUpdatingSelectedItems)
            {
                return;
            }

            InternalSelectedItems.Clear();
        }

        public bool IsItemSelected(object item)
        {
            return InternalSelectedItems.Contains(item);
        }
    }
}