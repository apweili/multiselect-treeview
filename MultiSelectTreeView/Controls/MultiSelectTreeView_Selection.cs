using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Converters;
using System.Windows.Data;
using System.Windows.Helpers;
using System.Windows.Interfaces;
using System.Windows.Threading;

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
                OnSelectedItemChanged,
                CoerceSelectedItem));

        private static object CoerceSelectedItem(DependencyObject d, object baseValue)
        {
            var treeView = (ItemsControl)d;
            if (treeView == null)
            {
                return DependencyProperty.UnsetValue;
            }

            if (!treeView.IsInitialized)
            {
                return baseValue;
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

            if (treeView.IsInitialized && e.NewValue != DependencyProperty.UnsetValue)
            {
                treeView.SelectItem(e.NewValue);
            }
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

        private IList InternalSelectedItems
        {
            get { return (IList)GetValue(InternalSelectedItemsImplProperty); }
        }

        /// <summary>
        ///     SelectedValuePath DependencyProperty
        /// </summary>
        public static readonly DependencyProperty SelectItemByCheckBoxProperty =
            DependencyProperty.RegisterAttached("SelectItemByCheckBox", typeof(bool), typeof(MultiSelectTreeView),
                new FrameworkPropertyMetadata(false, propertyChangedCallback:OnSelectItemByCheckBoxChanged));

        private static void OnSelectItemByCheckBoxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
        }

        public bool SelectItemByCheckBox
        {
            get { return (bool)GetValue(SelectItemByCheckBoxProperty); }
            set { SetValue(SelectItemByCheckBoxProperty, value); }
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
                    OnSelectedValuePathChanged));

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

            if (!treeView.IsInitialized)
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
                OnSelectedValueChanged,
                CoerceSelectedValue));

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

            if (!treeView.IsInitialized)
            {
                return baseValue;
            }

            if (treeView.SelectionMode == TreeViewSelectionMode.MultiSelectEnabled)
            {
                return null;
                // throw new InvalidOperationException("SingleSelectOnly support SelectedValueProperty");
            }

            if (!treeView.TryToSelectItemByValue(baseValue))
            {
                return null;
            }

            return baseValue;
        }
        
        /// <summary>
        ///     SelectedIndex DependencyProperty
        /// </summary>
        public static readonly DependencyProperty SelectedIndexProperty = Selector.SelectedIndexProperty.AddOwner(
            typeof(MultiSelectTreeView), new FrameworkPropertyMetadata(
                -1,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                OnSelectedIndexChanged,
                CoerceSelectedIndex));

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == DependencyProperty.UnsetValue)
            {
                return;
            }

            var treeView = (MultiSelectTreeView)d;
            if (treeView == null)
            {
                return;
            }

            treeView.SelectItemByIndex((int)e.NewValue);
        }

        private static object CoerceSelectedIndex(DependencyObject d, object baseValue)
        {
            if ((int)baseValue < -1)
            {
                return DependencyProperty.UnsetValue;
            }

            var treeView = (ItemsControl)d;
            if (!treeView.IsInitialized)
            {
                return baseValue;
            }

            return (int)baseValue >= treeView.Items.Count ? DependencyProperty.UnsetValue : baseValue;
        }

        private bool IsUpdatingSelectedItems { get; set; }

        private void SyncInternalByExternalSelectedItems()
        {
            if (SelectedItems == null)
            {
                return;
            }

            var externalSelectedItems = SelectedItems.OfType<object>().ToList();
            InternalSelectedItems.Clear();
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

        private void SyncExternalSelectedItems(NotifyCollectionChangedEventArgs e)
        {
            var selectedItems = SelectedItems;
            if (selectedItems == null)
            {
                return;
            }

            UpdateSelectedItems(selectedItems, e);
        }

        private void OnExternalSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var selectedItems = InternalSelectedItems;
            UpdateSelectedItems(selectedItems, e);
        }

        private void UpdateSelectedItems(IList selectedItems, NotifyCollectionChangedEventArgs e)
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
            try
            {
                BeginSelect();
                SyncExternalSelectedItems(e);
                OnInternalSelectedItemsChangedCore(e);
            }
            finally
            {
                EndSelect();
            }
        }

        private void BeginSelect()
        {
            IsUpdatingSelectedItems = true; 
        }

        private void EndSelect()
        {
            IsUpdatingSelectedItems = false; 
        }

        private bool IsSelecting()
        {
            return IsUpdatingSelectedItems;
        }

        // this eventhandler reacts on the firing control to, in order to update the own status
        private void OnInternalSelectedItemsChangedCore(NotifyCollectionChangedEventArgs e)
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

            SyncSelectedInfoAfterUpdateSelectedItems();
            var selectionChangedEventArgs =
                new SelectionChangedEventArgs(SelectionChangedEvent, addedItems, removedItems);
            OnSelectionChanged(selectionChangedEventArgs);
        }

        internal void SelectItem(object item)
        {
            if (SelectionMode == TreeViewSelectionMode.SingleSelectOnly)
            {
                SelectItemInSingleSelectionMode(item);
                return;
            }

            SelectItemInMultiSelectionMode(item);
        }

        private void SelectItemByIndex(int index)
        {
            Contract.Assert(index < Items.Count, "index < Items.Count");
            UnSelectAllItem();
            if (index < 0)
            {
                return;
            }

            SelectItem(Items[index]);
        }

        private bool TryToSelectItemByValue(object selectedValue)
        {
            var selectedItem = selectedValue == null ? null : GetSelectedItemByValue(selectedValue);
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

        private object GetSelectedItemByValue(object selectedValue)
        {
            var valuePath = SelectedValuePath;
            var items = GetAllItems();
            if (string.IsNullOrEmpty(valuePath))
            {
                return items.FirstOrDefault(selectedValue.Equals);
            }
            
            return items.FirstOrDefault(item => item != null &&
                                                selectedValue.Equals(
                                                    PropertyPathHelper.GetObjectByPropertyPath(item,
                                                        valuePath)));
        }

        private void SelectItemInMultiSelectionMode(object item)
        {
            if (IsSelecting())
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
            if (IsSelecting())
            {
                return;
            }

            InternalSelectedItems.Clear();
            InternalSelectedItems.Add(item);
        }

        internal void UnSelectItem(object item)
        {
            if (IsSelecting())
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

        private void UnSelectAllItem()
        {
            if (IsSelecting())
            {
                return;
            }

            InternalSelectedItems.Clear();
        }

        internal bool IsItemSelected(object item)
        {
            return InternalSelectedItems.Contains(item);
        }

        internal int GetSelectedItemsCount()
        {
            return InternalSelectedItems.Count;
        }

        internal List<object> GetInternalSelectedItemsCopy()
        {
            return InternalSelectedItems.Cast<object>().ToList();
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            IsItemsInitialized = true;
        }

        private bool IsItemsInitialized { get; set; }
        private bool IsSelectionInitialized { get; set; }
        private void SyncSelectedInfoAfterUpdateSelectedItems()
        {
            if (!IsItemsInitialized)
            {
                return;
            }

            var selectedItem = InternalSelectedItems.Count == 0 ? null : InternalSelectedItems.Last();
            if (IsSelectionInitialized)
            {
                SyncSelectedInfoNormal(selectedItem);
                return;
            }

            SyncSelectedInfoTheFirstTime(selectedItem);
        }

        private void SyncSelectedInfoTheFirstTime(object selectedItem)
        {
            if (selectedItem == null)
            {
                if (SelectedValue != null)
                {
                    selectedItem = GetSelectedItemByValue(SelectedValue);
                }

                var selectedIndex = SelectedIndex;
                if (selectedItem == null && selectedIndex > -1 && selectedIndex < Items.Count)
                {
                    selectedItem = Items[selectedIndex];
                }
            }

            Dispatcher.InvokeAsync(() =>
            {
                SyncSelectedInfoNormal(selectedItem);
                IsSelectionInitialized = true;
            }, DispatcherPriority.Input);
        }
        
        private void SyncSelectedInfoNormal(object selectedItem)
        {
            SelectedItem = selectedItem;
            UpdateSelectionBoxItem();
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

        private void UpdateSelectionBoxItem()
        {
            // var internalSelectedItems = InternalSelectedItems;
            // var selectedItem = SelectedItem;
            // var itemTemplate = ItemTemplate;
            // var stringFormat = ItemStringFormat;
            var internalSelectedItems = InternalSelectedItems.OfType<object>().ToList();
            if (SelectionMode == TreeViewSelectionMode.MultiSelectEnabled)
            {
                SelectionBoxItem = internalSelectedItems;
            }
            else
            {
                SelectionBoxItem = internalSelectedItems.FirstOrDefault();
                if (IsSelectionBoxItemIncludeImageSource(SelectionBoxItem))
                {
                    IsSelectionBoxItemIncludingRemark = true;
                }
            }

            if (SelectionBoxItemDataTemplate == null)
            {
                SelectionBoxItemDataTemplate = CreateSelectionBoxItem();
            }

            SelectionBoxItemTemplate = SelectionBoxItemDataTemplate;
        }

        private bool IsSelectionBoxItemIncludeImageSource(object selectionBoxItem)
        {
            if (selectionBoxItem == null)
            {
                return false;
            }

            return selectionBoxItem is IAutoBindImageSourceModel &&
                   ((IAutoBindImageSourceModel)selectionBoxItem).ImageSource != null;
        }

        protected override void OnDisplayMemberPathChanged(string oldDisplayMemberPath, string newDisplayMemberPath)
        {
            SelectionBoxItemDataTemplate = CreateSelectionBoxItem();
            base.OnDisplayMemberPathChanged(oldDisplayMemberPath, newDisplayMemberPath);
        }

        private DataTemplate SelectionBoxItemDataTemplate { get; set; }

        private DataTemplate CreateSelectionBoxItem()
        {
            var template = new DataTemplate();
            var text = new FrameworkElementFactory(typeof(TextBlock));
            var binding = new Binding
            {
                Converter = new SelectionBoxItemValueConverter(),
                ConverterParameter = DisplayMemberPath
            };
            text.SetBinding(TextBlock.TextProperty, binding);
            template.VisualTree = text;
            template.Seal();
            return template;
        }
    }
}