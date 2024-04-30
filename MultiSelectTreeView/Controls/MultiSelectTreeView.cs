﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Helpers;
using System.Windows.Input;
using System.Windows.Media;

namespace System.Windows.Controls
{
    public class MultiSelectTreeView : ItemsControl
    {
        #region Constants and Fields

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
            add => AddHandler(SelectionChangedEvent, value);
            remove => RemoveHandler(SelectionChangedEvent, value);
        }

        public event PreviewSelectionChangedEventHandler PreviewSelectionChanged
        {
            add => AddHandler(PreviewSelectionChangedEvent, value);
            remove => RemoveHandler(PreviewSelectionChangedEvent, value);
        }

        public static readonly DependencyProperty LastSelectedItemProperty;

        public static DependencyProperty BackgroundSelectionRectangleProperty = DependencyProperty.Register(
            "BackgroundSelectionRectangle",
            typeof(Brush),
            typeof(MultiSelectTreeView),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0x60, 0x33, 0x99, 0xFF)), null));

        public static DependencyProperty BorderBrushSelectionRectangleProperty = DependencyProperty.Register(
            "BorderBrushSelectionRectangle",
            typeof(Brush),
            typeof(MultiSelectTreeView),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0x33, 0x99, 0xFF)), null));

        public static DependencyProperty HoverHighlightingProperty = DependencyProperty.Register(
            "HoverHighlighting",
            typeof(bool),
            typeof(MultiSelectTreeView),
            new FrameworkPropertyMetadata(true, null));

        public static DependencyProperty VerticalRulersProperty = DependencyProperty.Register(
            "VerticalRulers",
            typeof(bool),
            typeof(MultiSelectTreeView),
            new FrameworkPropertyMetadata(false, null));

        public static DependencyProperty ItemIndentProperty = DependencyProperty.Register(
            "ItemIndent",
            typeof(int),
            typeof(MultiSelectTreeView),
            new FrameworkPropertyMetadata(13, null));

        public static DependencyProperty IsKeyboardModeProperty = DependencyProperty.Register(
            "IsKeyboardMode",
            typeof(bool),
            typeof(MultiSelectTreeView),
            new FrameworkPropertyMetadata(false, null));

        public static DependencyPropertyKey LastSelectedItemPropertyKey = DependencyProperty.RegisterReadOnly(
            "LastSelectedItem",
            typeof(object),
            typeof(MultiSelectTreeView),
            new FrameworkPropertyMetadata(null));

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

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            var treeViewItems = RecursiveTreeViewItemEnumerable(this, true);
            foreach (var treeViewItem in treeViewItems)
            {
                var item = treeViewItem.DataContext;
                if (item == null)
                {
                    continue;
                }

                if (InternalSelectedItems.Contains(item))
                {
                    treeViewItem.IsSelected = true;
                }
            }
        }

        public static DependencyProperty AllowEditItemsProperty = DependencyProperty.Register(
            "AllowEditItems",
            typeof(bool),
            typeof(MultiSelectTreeView),
            new FrameworkPropertyMetadata(false, null));

        public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register(
            "SelectionMode", typeof(TreeViewSelectionMode), typeof(MultiSelectTreeView),
            new FrameworkPropertyMetadata(default(TreeViewSelectionMode), FrameworkPropertyMetadataOptions.None,
                OnSelectionModeChanged));

        #region selection

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

        #endregion

        #endregion

        #region Constructors and Destructors

        static MultiSelectTreeView()
        {
            LastSelectedItemProperty = LastSelectedItemPropertyKey.DependencyProperty;
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiSelectTreeView),
                new FrameworkPropertyMetadata(typeof(MultiSelectTreeView)));
        }

        public MultiSelectTreeView()
        {
            var internalSelectedItems = new ObservableCollection<object>();
            internalSelectedItems.CollectionChanged += OnInternalSelectedItemsChanged;
            internalSelectedItems.CollectionChanged += OnInternalSelectedItemsChangedForExternalSelectedItems;
            SetValue(InternalSelectedItemsPropertyKey, internalSelectedItems);
            Selection = new SelectionMultiple(this);
            Selection.PreviewSelectionChanged += PreviewSelectionChangedHandler;
        }

        #endregion

        #region Public Properties

        public Brush BackgroundSelectionRectangle
        {
            get => (Brush)GetValue(BackgroundSelectionRectangleProperty);
            set => SetValue(BackgroundSelectionRectangleProperty, value);
        }

        public Brush BorderBrushSelectionRectangle
        {
            get => (Brush)GetValue(BorderBrushSelectionRectangleProperty);
            set => SetValue(BorderBrushSelectionRectangleProperty, value);
        }

        public bool HoverHighlighting
        {
            get => (bool)GetValue(HoverHighlightingProperty);
            set => SetValue(HoverHighlightingProperty, value);
        }

        public bool VerticalRulers
        {
            get => (bool)GetValue(VerticalRulersProperty);
            set => SetValue(VerticalRulersProperty, value);
        }

        public int ItemIndent
        {
            get => (int)GetValue(ItemIndentProperty);
            set => SetValue(ItemIndentProperty, value);
        }

        [Browsable(false)]
        public bool IsKeyboardMode
        {
            get => (bool)GetValue(IsKeyboardModeProperty);
            set => SetValue(IsKeyboardModeProperty, value);
        }

        public bool AllowEditItems
        {
            get => (bool)GetValue(AllowEditItemsProperty);
            set => SetValue(AllowEditItemsProperty, value);
        }

        /// <summary>
        ///    Gets the last selected item.
        /// </summary>
        public object LastSelectedItem
        {
            get => GetValue(LastSelectedItemProperty);
            private set => SetValue(LastSelectedItemPropertyKey, value);
        }

        /// <summary>
        ///    Determines whether multi-selection is enabled or not 
        /// </summary>
        public TreeViewSelectionMode SelectionMode
        {
            get => (TreeViewSelectionMode)GetValue(SelectionModeProperty);
            set => SetValue(SelectionModeProperty, value);
        }

        private MultiSelectTreeViewItem lastFocusedItem;

        /// <summary>
        /// Gets the last focused item.
        /// </summary>
        internal MultiSelectTreeViewItem LastFocusedItem
        {
            get => lastFocusedItem;
            set
            {
                // Only the last focused MultiSelectTreeViewItem may have IsTabStop = true
                // so that the keyboard focus only stops a single time for the MultiSelectTreeView control.
                if (lastFocusedItem != null)
                {
                    lastFocusedItem.IsTabStop = false;
                }

                lastFocusedItem = value;
                if (lastFocusedItem != null)
                {
                    lastFocusedItem.IsTabStop = true;
                }

                // The MultiSelectTreeView control only has the tab stop if none of its items has it.
                IsTabStop = lastFocusedItem == null;
            }
        }

        internal ISelectionStrategy Selection { get; private set; }

        #endregion

        #region Public Methods and Operators

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Selection.ApplyTemplate();
        }

        public bool ClearSelection()
        {
            if (InternalSelectedItems.Count > 0)
            {
                // Make a copy of the list and ignore changes to the selection while raising events
                foreach (var selItem in new ArrayList(InternalSelectedItems))
                {
                    var e = new PreviewSelectionChangedEventArgs(false, selItem);
                    OnPreviewSelectionChanged(e);
                    if (e.CancelAny)
                    {
                        return false;
                    }
                }

                InternalSelectedItems.Clear();
            }

            return true;
        }

        public void FocusItem(object item, bool bringIntoView = false)
        {
            MultiSelectTreeViewItem node = GetTreeViewItemsFor(new List<object> { item }).FirstOrDefault();
            if (node != null)
            {
                FocusHelper.Focus(node, bringIntoView);
            }
        }

        public void BringItemIntoView(object item)
        {
            MultiSelectTreeViewItem node = GetTreeViewItemsFor(new List<object> { item }).First();
            FrameworkElement itemContent = (FrameworkElement)node.Template.FindName("headerBorder", node);
            itemContent.BringIntoView();
        }

        public bool SelectNextItem()
        {
            return Selection.SelectNextFromKey();
        }

        public bool SelectPreviousItem()
        {
            return Selection.SelectPreviousFromKey();
        }

        public bool SelectFirstItem()
        {
            return Selection.SelectFirstFromKey();
        }

        public bool SelectLastItem()
        {
            return Selection.SelectLastFromKey();
        }

        public bool SelectAllItems()
        {
            return Selection.SelectAllFromKey();
        }

        public bool SelectParentItem()
        {
            return Selection.SelectParentFromKey();
        }

        #endregion

        #region Methods

        internal bool DeselectRecursive(MultiSelectTreeViewItem item, bool includeSelf)
        {
            List<MultiSelectTreeViewItem> selectedChildren = new List<MultiSelectTreeViewItem>();
            if (includeSelf)
            {
                if (item.IsSelected)
                {
                    var e = new PreviewSelectionChangedEventArgs(false, item.DataContext);
                    OnPreviewSelectionChanged(e);
                    if (e.CancelAny)
                    {
                        return false;
                    }

                    selectedChildren.Add(item);
                }
            }

            if (!CollectDeselectRecursive(item, selectedChildren))
            {
                return false;
            }

            foreach (var child in selectedChildren)
            {
                child.IsSelected = false;
            }

            return true;
        }

        private bool CollectDeselectRecursive(MultiSelectTreeViewItem item,
            List<MultiSelectTreeViewItem> selectedChildren)
        {
            foreach (var child in item.Items)
            {
                MultiSelectTreeViewItem tvi =
                    item.ItemContainerGenerator.ContainerFromItem(child) as MultiSelectTreeViewItem;
                if (tvi != null)
                {
                    if (tvi.IsSelected)
                    {
                        var e = new PreviewSelectionChangedEventArgs(false, child);
                        OnPreviewSelectionChanged(e);
                        if (e.CancelAny)
                        {
                            return false;
                        }

                        selectedChildren.Add(tvi);
                    }

                    if (!CollectDeselectRecursive(tvi, selectedChildren))
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        public void RecursiveExpand(MultiSelectTreeViewItem item)
        {
            if (item.Items.Count > 0)
            {
                item.UpdateLayout();
                foreach (var child in item.Items)
                {
                    MultiSelectTreeViewItem tvi =
                        item.ItemContainerGenerator.ContainerFromItem(child) as MultiSelectTreeViewItem;
                    if (tvi != null)
                    {
                        tvi.IsExpanded = true;
                        RecursiveExpand(tvi);
                    }
                }
            }
        }

        internal bool ClearSelectionByRectangle()
        {
            foreach (var item in new ArrayList(InternalSelectedItems))
            {
                var e = new PreviewSelectionChangedEventArgs(false, item);
                OnPreviewSelectionChanged(e);
                if (e.CancelAny) return false;
            }

            InternalSelectedItems.Clear();
            return true;
        }

        internal MultiSelectTreeViewItem GetNextItem(MultiSelectTreeViewItem item, List<MultiSelectTreeViewItem> items)
        {
            int indexOfCurrent = item != null ? items.IndexOf(item) : -1;
            for (int i = indexOfCurrent + 1; i < items.Count; i++)
            {
                if (items[i].IsVisible)
                {
                    return items[i];
                }
            }

            return null;
        }

        internal MultiSelectTreeViewItem GetPreviousItem(MultiSelectTreeViewItem item,
            List<MultiSelectTreeViewItem> items)
        {
            int indexOfCurrent = item != null ? items.IndexOf(item) : -1;
            for (int i = indexOfCurrent - 1; i >= 0; i--)
            {
                if (items[i].IsVisible)
                {
                    return items[i];
                }
            }

            return null;
        }

        internal MultiSelectTreeViewItem GetFirstItem(List<MultiSelectTreeViewItem> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].IsVisible)
                {
                    return items[i];
                }
            }

            return null;
        }

        internal MultiSelectTreeViewItem GetLastItem(List<MultiSelectTreeViewItem> items)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i].IsVisible)
                {
                    return items[i];
                }
            }

            return null;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MultiSelectTreeViewItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            ((ItemsControl)element).DisplayMemberPath = DisplayMemberPath;
            AutoBindHelper.TryToAutoBindObject(element, item);
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MultiSelectTreeViewItem;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new MultiSelectTreeViewAutomationPeer(this);
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

        private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectTreeView treeView = (MultiSelectTreeView)d;

            if (treeView.Selection != null)
            {
                treeView.Selection.PreviewSelectionChanged -= treeView.PreviewSelectionChangedHandler;
            }

            switch ((TreeViewSelectionMode)e.NewValue)
            {
                case TreeViewSelectionMode.MultiSelectEnabled:
                    treeView.Selection = new SelectionMultiple(treeView);
                    break;
                case TreeViewSelectionMode.SingleSelectOnly:
                    treeView.Selection = new SelectionSingle(treeView);
                    break;
            }

            if (treeView.Selection != null)
            {
                treeView.Selection.PreviewSelectionChanged += treeView.PreviewSelectionChangedHandler;
            }
        }

        private void PreviewSelectionChangedHandler(object sender, PreviewSelectionChangedEventArgs e)
        {
            OnPreviewSelectionChanged(e);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            InternalSelectedItems.Remove(item);
                            // Don't preview and ask, it is already gone so it must be removed from
                            // the SelectedItems list
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    // If the items list has considerably changed, the selection is probably
                    // useless anyway, clear it entirely.
                    InternalSelectedItems.Clear();
                    break;
            }

            base.OnItemsChanged(e);
        }

        internal static IEnumerable<MultiSelectTreeViewItem> RecursiveTreeViewItemEnumerable(ItemsControl parent,
            bool includeInvisible)
        {
            return RecursiveTreeViewItemEnumerable(parent, includeInvisible, true);
        }

        internal static IEnumerable<MultiSelectTreeViewItem> RecursiveTreeViewItemEnumerable(ItemsControl parent,
            bool includeInvisible, bool includeDisabled)
        {
            foreach (var item in parent.Items)
            {
                MultiSelectTreeViewItem tve =
                    (MultiSelectTreeViewItem)parent.ItemContainerGenerator.ContainerFromItem(item);
                if (tve == null)
                {
                    // Container was not generated, therefore it is probably not visible, so we can ignore it.
                    continue;
                }

                if (!includeInvisible && !tve.IsVisible)
                {
                    continue;
                }

                if (!includeDisabled && !tve.IsEnabled)
                {
                    continue;
                }

                yield return tve;
                if (includeInvisible || tve.IsExpanded)
                {
                    foreach (var childItem in RecursiveTreeViewItemEnumerable(tve, includeInvisible, includeDisabled))
                    {
                        yield return childItem;
                    }
                }
            }
        }

        internal IEnumerable<MultiSelectTreeViewItem> GetNodesToSelectBetween(MultiSelectTreeViewItem firstNode,
            MultiSelectTreeViewItem lastNode)
        {
            var allNodes = RecursiveTreeViewItemEnumerable(this, false, false).ToList();
            var firstIndex = allNodes.IndexOf(firstNode);
            var lastIndex = allNodes.IndexOf(lastNode);

            if (firstIndex >= allNodes.Count)
            {
                throw new InvalidOperationException(
                    "First node index " + firstIndex + "greater or equal than count " + allNodes.Count + ".");
            }

            if (lastIndex >= allNodes.Count)
            {
                throw new InvalidOperationException(
                    "Last node index " + lastIndex + " greater or equal than count " + allNodes.Count + ".");
            }

            var nodesToSelect = new List<MultiSelectTreeViewItem>();

            if (lastIndex == firstIndex)
            {
                return new List<MultiSelectTreeViewItem> { firstNode };
            }

            if (lastIndex > firstIndex)
            {
                for (int i = firstIndex; i <= lastIndex; i++)
                {
                    if (allNodes[i].IsVisible)
                    {
                        nodesToSelect.Add(allNodes[i]);
                    }
                }
            }
            else
            {
                for (int i = firstIndex; i >= lastIndex; i--)
                {
                    if (allNodes[i].IsVisible)
                    {
                        nodesToSelect.Add(allNodes[i]);
                    }
                }
            }

            return nodesToSelect;
        }

        /// <summary>
        /// Finds the treeview item for each of the specified data items.
        /// </summary>
        /// <param name="dataItems">List of data items to search for.</param>
        /// <returns></returns>
        internal IEnumerable<MultiSelectTreeViewItem> GetTreeViewItemsFor(IEnumerable dataItems)
        {
            if (dataItems == null)
            {
                yield break;
            }

            foreach (var dataItem in dataItems)
            {
                foreach (var treeViewItem in RecursiveTreeViewItemEnumerable(this, true))
                {
                    if (treeViewItem.DataContext == dataItem)
                    {
                        yield return treeViewItem;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all data items referenced in all treeview items of the entire control.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable GetAllDataItems()
        {
            foreach (var treeViewItem in RecursiveTreeViewItemEnumerable(this, true))
            {
                yield return treeViewItem.DataContext;
            }
        }

        internal IEnumerable<object> GetAllItems()
        {
            return RecursiveTreeViewItemEnumerable(this, true).Select(x => x.DataContext);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled)
            {
                // Basically, this should not be needed anymore. It allows selecting an item with
                // the keyboard when the MultiSelectTreeView control has the focus. If there were already
                // items when the control was focused, an item has already been focused (and
                // subsequent key presses won't land here but at the item).
                Key key = e.Key;
                switch (key)
                {
                    case Key.Up:
                        // Select last item
                        var lastNode = RecursiveTreeViewItemEnumerable(this, false).LastOrDefault();
                        if (lastNode != null)
                        {
                            Selection.Select(lastNode);
                            e.Handled = true;
                        }

                        break;
                    case Key.Down:
                        // Select first item
                        var firstNode = RecursiveTreeViewItemEnumerable(this, false).FirstOrDefault();
                        if (firstNode != null)
                        {
                            Selection.Select(firstNode);
                            e.Handled = true;
                        }

                        break;
                }
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (!IsKeyboardMode)
            {
                IsKeyboardMode = true;
                //System.Diagnostics.Debug.WriteLine("Changing to keyboard mode from PreviewKeyDown");
            }
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (!IsKeyboardMode)
            {
                IsKeyboardMode = true;
                //System.Diagnostics.Debug.WriteLine("Changing to keyboard mode from PreviewKeyUp");
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            if (IsKeyboardMode)
            {
                IsKeyboardMode = false;
                //System.Diagnostics.Debug.WriteLine("Changing to mouse mode");
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("MultiSelectTreeView.OnGotFocus()");
            //System.Diagnostics.Debug.WriteLine(Environment.StackTrace);

            base.OnGotFocus(e);

            // If the MultiSelectTreeView control has gotten the focus, it needs to pass it to an
            // item instead. If there was an item focused before, return to that. Otherwise just
            // focus this first item in the list if any. If there are no items at all, the
            // MultiSelectTreeView control just keeps the focus.
            // In any case, the focussing must occur when the current event processing is finished,
            // i.e. be queued in the dispatcher. Otherwise the TreeView could keep its focus
            // because other focus things are still going on and interfering this final request.

            var lastFocusedItem = LastFocusedItem;
            if (lastFocusedItem != null)
            {
                Dispatcher.BeginInvoke((Action)(() => FocusHelper.Focus(lastFocusedItem)));
            }
            else
            {
                var firstNode = RecursiveTreeViewItemEnumerable(this, false).FirstOrDefault();
                if (firstNode != null)
                {
                    Dispatcher.BeginInvoke((Action)(() => FocusHelper.Focus(firstNode)));
                }
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            // This happens when a mouse button was pressed in an area which is not covered by an
            // item. Then, it should be focused which in turn passes on the focus to an item.
            Focus();
        }

        protected virtual void OnPreviewSelectionChanged(PreviewSelectionChangedEventArgs e)
        {
            e.RoutedEvent = PreviewSelectionChangedEvent;
            RaiseEvent(e);
        }

        protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            e.RoutedEvent = SelectionChangedEvent;
            RaiseEvent(e);
        }

        #endregion
    }
}