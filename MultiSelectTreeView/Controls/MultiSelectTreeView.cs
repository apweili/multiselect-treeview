using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Helpers;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Models;

namespace System.Windows.Controls
{
    public partial class MultiSelectTreeView : ItemsControl
    {
        static MultiSelectTreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiSelectTreeView),
                new FrameworkPropertyMetadata(typeof(MultiSelectTreeView)));
            ToolTipService.IsEnabledProperty.OverrideMetadata(typeof(MultiSelectTreeView),
                new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceToolTipIsEnabled)));
            EventManager.RegisterClassHandler(typeof(MultiSelectTreeView), Mouse.LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture));
            EventManager.RegisterClassHandler(typeof(MultiSelectTreeView), Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseButtonDown), true);
            EventManager.RegisterClassHandler(typeof(MultiSelectTreeView), Mouse.MouseWheelEvent, new MouseWheelEventHandler(OnMouseWheel), true);
            EventManager.RegisterClassHandler(typeof(MultiSelectTreeView), MultiSelectTreeViewItem.MultiTreeViewItemMouseMoveEvent, new RoutedEventHandler(OnItemMouseMove));
        }

        private static void OnItemMouseMove(object sender, RoutedEventArgs e)
        {
            var multiSelectTreeView = (MultiSelectTreeView)sender;
            if (multiSelectTreeView.IsAbleToExpand())
            {
                return;
            }

            var multiSelectTreeViewItem = e.OriginalSource as MultiSelectTreeViewItem;
            if (multiSelectTreeViewItem != null && !multiSelectTreeViewItem.IsKeyboardFocusWithin)
            {
                multiSelectTreeViewItem.Focus();
            }
        }

        private static void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var multiSelectTreeView = (MultiSelectTreeView)sender;
            if (!multiSelectTreeView.IsKeyboardFocusWithin)
            {
                if (multiSelectTreeView.IsDropDownOpen)
                {
                    multiSelectTreeView.Close();
                }

                e.Handled = true;
                return;
            }

            if(!FindDescendant(multiSelectTreeView, e.OriginalSource as DependencyObject))
            {
                if (multiSelectTreeView.IsDropDownOpen)
                {
                    multiSelectTreeView.Close();
                }

                e.Handled = true;
                return;
            }
        }

        private static void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            var multiSelectTreeView = (MultiSelectTreeView)sender;
            if (!multiSelectTreeView.IsKeyboardFocusWithin)
            {
                multiSelectTreeView.Focus();
            }

            e.Handled = true;
            if (Mouse.Captured == multiSelectTreeView && e.OriginalSource == multiSelectTreeView)
            {
                multiSelectTreeView.Close();
            }
        }

        private static void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            var multiSelectTreeView = (MultiSelectTreeView)sender;
            if (Mouse.Captured == multiSelectTreeView)
            {
                return;
            }

            if (e.OriginalSource == multiSelectTreeView)
            {
                if (Mouse.Captured == null ||
                    !FindDescendant(multiSelectTreeView, Mouse.Captured as DependencyObject))
                {
                    multiSelectTreeView.Close();
                }
            }
            else
            {
                if (FindDescendant(multiSelectTreeView, e.OriginalSource as DependencyObject))
                {
                    if (multiSelectTreeView.IsDropDownOpen && Mouse.Captured == null)
                    {
                        Mouse.Capture(multiSelectTreeView, CaptureMode.SubTree);
                        e.Handled = true;
                    }
                }
                else
                {
                    multiSelectTreeView.Close();
                }
            }
        }

        public static bool FindDescendant(DependencyObject reference, DependencyObject node)
        {
            if (node == null)
            {
                return false;
            }

            var childrenCount = VisualTreeHelper.GetChildrenCount(reference);
            if (childrenCount == 0) return false;

            for (int i = 0; i < childrenCount; i++)
            {
                var current = VisualTreeHelper.GetChild(reference, i);
                if (current == node)
                    return true;

                if (FindDescendant(current, node))
                {
                    return true;
                }
            }

            return false;
        }

        private static object CoerceToolTipIsEnabled(DependencyObject d, object value)
        {
            MultiSelectTreeView cb = (MultiSelectTreeView)d;
            return cb.IsDropDownOpen ? false : value;
        }

        #region Constants and Fields

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

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        public static DependencyProperty AllowEditItemsProperty = DependencyProperty.Register(
            "AllowEditItems",
            typeof(bool),
            typeof(MultiSelectTreeView),
            new FrameworkPropertyMetadata(false, null));

        public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register(
            "SelectionMode", typeof(TreeViewSelectionMode), typeof(MultiSelectTreeView),
            new FrameworkPropertyMetadata(TreeViewSelectionMode.SingleSelectOnly, FrameworkPropertyMetadataOptions.None,
                OnSelectionModeChanged));

        #endregion

        #region Constructors and Destructors

        public MultiSelectTreeView()
        {
            var internalSelectedItems = new ObservableCollection<object>();
            internalSelectedItems.CollectionChanged += OnInternalSelectedItemsChanged;
            SetValue(InternalSelectedItemsPropertyKey, internalSelectedItems);
        }

        #endregion

        #region Public Properties

        public Brush BackgroundSelectionRectangle
        {
            get { return (Brush)GetValue(BackgroundSelectionRectangleProperty); }
            set { SetValue(BackgroundSelectionRectangleProperty, value); }
        }

        public Brush BorderBrushSelectionRectangle
        {
            get { return (Brush)GetValue(BorderBrushSelectionRectangleProperty); }
            set { SetValue(BorderBrushSelectionRectangleProperty, value); }
        }

        public bool HoverHighlighting
        {
            get { return (bool)GetValue(HoverHighlightingProperty); }
            set { SetValue(HoverHighlightingProperty, value); }
        }

        public bool VerticalRulers
        {
            get { return (bool)GetValue(VerticalRulersProperty); }
            set { SetValue(VerticalRulersProperty, value); }
        }

        public int ItemIndent
        {
            get { return (int)GetValue(ItemIndentProperty); }
            set { SetValue(ItemIndentProperty, value); }
        }

        [Browsable(false)]
        public bool IsKeyboardMode
        {
            get { return (bool)GetValue(IsKeyboardModeProperty); }
            set { SetValue(IsKeyboardModeProperty, value); }
        }

        public bool AllowEditItems
        {
            get { return (bool)GetValue(AllowEditItemsProperty); }
            set { SetValue(AllowEditItemsProperty, value); }
        }

        /// <summary>
        ///    Determines whether multi-selection is enabled or not 
        /// </summary>
        public TreeViewSelectionMode SelectionMode
        {
            get { return (TreeViewSelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        private MultiSelectTreeViewItem lastFocusedItem;

        /// <summary>
        /// Gets the last focused item.
        /// </summary>
        internal MultiSelectTreeViewItem LastFocusedItem
        {
            get { return lastFocusedItem; }
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
            Popup = (Popup)GetTemplateChild(ContentPopupName);
            if (Selection == null)
            {
                Selection = GetSelectionStrategyByMode(this, SelectionMode);
            }

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

                DeselectAllItem();
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

            DeselectAllItem();
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
            element.SetValue(MultiSelectTreeView.SelectItemByCheckBoxProperty, SelectItemByCheckBox);
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

        private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectTreeView treeView = (MultiSelectTreeView)d;
            if (treeView.Selection != null)
            {
                treeView.Selection.PreviewSelectionChanged -= treeView.PreviewSelectionChangedHandler;
            }

            treeView.Selection = GetSelectionStrategyByMode(treeView, (TreeViewSelectionMode)e.NewValue);
            var defaultSelectedItem = treeView.InternalSelectedItems.OfType<object>().LastOrDefault();
            treeView.DeselectAllItem();
            if (defaultSelectedItem != null)
            {
                treeView.SelectItem(defaultSelectedItem);
            }
        }

        private static ISelectionStrategy GetSelectionStrategyByMode(MultiSelectTreeView treeView, TreeViewSelectionMode mode)
        {
            ISelectionStrategy selectionStrategy;
            switch (mode)
            {
                case TreeViewSelectionMode.MultiSelectEnabled:
                    selectionStrategy  = new SelectionMultiple(treeView);
                    break;
                case TreeViewSelectionMode.SingleSelectOnly:
                    selectionStrategy  = new SelectionSingle(treeView);
                    break;
                default:
                    throw new ArgumentException(); 
            }

            selectionStrategy.PreviewSelectionChanged += treeView.PreviewSelectionChangedHandler;
            return selectionStrategy;
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
                            DeselectItem(item);
                            // Don't preview and ask, it is already gone so it must be removed from
                            // the SelectedItems list
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    // If the items list has considerably changed, the selection is probably
                    // useless anyway, clear it entirely.
                    DeselectAllItem();
                    break;
            }

            base.OnItemsChanged(e);
        }

        internal bool IsAbleToExpand()
        {
            return (from object item in Items
                select (MultiSelectTreeViewItem)this.ItemContainerGenerator.ContainerFromItem(item)
                into container
                where container != null
                select container).Any(container => container.HasItems);
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
                var container =
                    (MultiSelectTreeViewItem)parent.ItemContainerGenerator.ContainerFromItem(item);
                if (container == null)
                {
                    // Container was not generated, therefore it is probably not visible, so we can ignore it.
                    continue;
                }

                if (!includeInvisible && !container.IsVisible)
                {
                    continue;
                }

                if (!includeDisabled && !container.IsEnabled)
                {
                    continue;
                }

                yield return container;
                if (includeInvisible || container.IsExpanded)
                {
                    foreach (var childItem in RecursiveTreeViewItemEnumerable(container, includeInvisible, includeDisabled))
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

        internal IEnumerable<object> GetAllItemsByAutoBindExpandableModel()
        {
            var itemsSourceQueue = new Queue<IEnumerable>();
            itemsSourceQueue.Enqueue(Items);
            while (itemsSourceQueue.Count > 0)
            {
                var items = itemsSourceQueue.Dequeue();
                foreach (var item in items)
                {
                    yield return item;
                    var autoBindableModel = item as IAutoBindExpandableModel;
                    if (autoBindableModel == null)
                    {
                        continue;
                    }

                    var childrenOfItem = autoBindableModel.Children;
                    if (childrenOfItem != null)
                    {
                        itemsSourceQueue.Enqueue(childrenOfItem);
                    }
                }
            } 
        }
        
        internal IEnumerable<object> GetAllItems()
        {
            var dataItems = Items.Cast<object>().ToList();
            if (dataItems.All(item => item is IAutoBindExpandableModel))
            {
                return GetAllItemsByAutoBindExpandableModel();
            }

            if (dataItems.Any(item => ItemContainerGenerator.ContainerFromItem(item) == null))
            {
                return dataItems;
            }

            return GetAllItemsByTreeViewItem();
        }


        internal IEnumerable<object> GetAllItemsByTreeViewItem()
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

        private Popup Popup { get; set; }
        private const string ContentPopupName = "PART_Popup";
    }
}