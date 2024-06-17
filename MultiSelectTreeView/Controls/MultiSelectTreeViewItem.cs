using System.Collections;
using System.Collections.Specialized;
using System.Windows.Automation.Peers;
using System.Windows.Enums;
using System.Windows.Helpers;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Models;

namespace System.Windows.Controls
{
    public partial class MultiSelectTreeViewItem : HeaderedItemsControl
    {
        static MultiSelectTreeViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MultiSelectTreeViewItem),
                new FrameworkPropertyMetadata(typeof(MultiSelectTreeViewItem)));
            EventManager.RegisterClassHandler(typeof(MultiSelectTreeViewItem), Mouse.MouseMoveEvent, new MouseEventHandler(OnMouseMove));
        }

        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            var multiSelectTreeViewItem = (MultiSelectTreeViewItem)sender;
            var itemMouseMove = new RoutedEventArgs
            {
                RoutedEvent = MultiTreeViewItemMouseMoveEvent,
                Source = multiSelectTreeViewItem
            };
            multiSelectTreeViewItem.RaiseEvent(itemMouseMove);
        }

        internal static readonly RoutedEvent MultiTreeViewItemMouseMoveEvent = EventManager.RegisterRoutedEvent("MultiTreeViewItemMouseMove", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MultiSelectTreeViewItem));

        #region Dependency properties 

        #region Brushes

        public static DependencyProperty BackgroundFocusedProperty = DependencyProperty.Register(
            "BackgroundFocused",
            typeof(Brush),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(SystemColors.HighlightBrush, null));

        public static DependencyProperty BackgroundSelectedHoveredProperty = DependencyProperty.Register(
            "BackgroundSelectedHovered",
            typeof(Brush),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(Brushes.DarkGray, null));

        public static DependencyProperty BackgroundSelectedProperty = DependencyProperty.Register(
            "BackgroundSelected",
            typeof(Brush),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(SystemColors.HighlightBrush, null));

        public static DependencyProperty ForegroundSelectedProperty = DependencyProperty.Register(
            "ForegroundSelected",
            typeof(Brush),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(SystemColors.HighlightTextBrush, null));

        public static DependencyProperty BackgroundHoveredProperty = DependencyProperty.Register(
            "BackgroundHovered",
            typeof(Brush),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(Brushes.LightGray, null));

        public static DependencyProperty BackgroundInactiveProperty = DependencyProperty.Register(
            "BackgroundInactive",
            typeof(Brush),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(SystemColors.ControlBrush, null));

        public static DependencyProperty ForegroundInactiveProperty = DependencyProperty.Register(
            "ForegroundInactive",
            typeof(Brush),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, null));

        public static DependencyProperty BorderBrushHoveredProperty = DependencyProperty.Register(
            "BorderBrushHovered",
            typeof(Brush),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(Brushes.Transparent, null));

        public static DependencyProperty BorderBrushFocusedProperty = DependencyProperty.Register(
            "BorderBrushFocused",
            typeof(Brush),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(Brushes.Transparent, null));

        public static DependencyProperty BorderBrushInactiveProperty = DependencyProperty.Register(
            "BorderBrushInactive",
            typeof(Brush),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(Brushes.Black, null));

        public static DependencyProperty BorderBrushSelectedProperty = DependencyProperty.Register(
            "BorderBrushSelected",
            typeof(Brush),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(Brushes.Transparent, null));

        #endregion Brushes

        #region Others

        public static DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded",
            typeof(bool),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(false));

        public static DependencyProperty IsEditableProperty = DependencyProperty.Register(
            "IsEditable",
            typeof(bool),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(true));

        public static new DependencyProperty IsVisibleProperty = DependencyProperty.Register(
            "IsVisible",
            typeof(bool),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(true));

        public static DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected",
            typeof(bool),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsSelectedChanged)));

        public static DependencyProperty IsEditingProperty = DependencyProperty.Register(
            "IsEditing",
            typeof(bool),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(false));

        public static DependencyProperty ContentTemplateEditProperty = DependencyProperty.Register(
            "ContentTemplateEdit",
            typeof(DataTemplate),
            typeof(MultiSelectTreeViewItem));

        public static DependencyProperty DisplayNameProperty = DependencyProperty.Register(
            "DisplayName",
            typeof(string),
            typeof(MultiSelectTreeViewItem));

        public static DependencyProperty HoverHighlightingProperty = DependencyProperty.Register(
            "HoverHighlighting",
            typeof(bool),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(false));

        public static DependencyProperty ItemIndentProperty = DependencyProperty.Register(
            "ItemIndent",
            typeof(int),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(13));

        public static DependencyProperty IsKeyboardModeProperty = DependencyProperty.Register(
            "IsKeyboardMode",
            typeof(bool),
            typeof(MultiSelectTreeViewItem),
            new FrameworkPropertyMetadata(false));

        public static DependencyProperty RemarksProperty = DependencyProperty.Register(
            "Remarks",
            typeof(object),
            typeof(MultiSelectTreeViewItem));

        public static DependencyProperty RemarksTemplateProperty = DependencyProperty.Register(
            "RemarksTemplate",
            typeof(DataTemplate),
            typeof(MultiSelectTreeViewItem));

        public static DependencyProperty SelectionCheckStateProperty = DependencyProperty.Register(
            "SelectionCheckState",
            typeof(SelectionCheckState),
            typeof(MultiSelectTreeViewItem));
        
        #endregion Others

        #endregion Dependency properties

        #region Constructors and Destructors

        public MultiSelectTreeViewItem()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            MultiSelectTreeView parentTV = ParentTreeView;
            IsSelected = parentTV.IsItemSelected(this.DataContext);
            var autoBindModel = DataContext as IAutoBindExpandableModel;
            if (autoBindModel == null)
            {
                SelectionCheckState = IsSelected ? SelectionCheckState.FullSelected : SelectionCheckState.Deselected;
            }
        }

        #endregion

        #region Public Properties

        #region Brushes

        public Brush BackgroundFocused
        {
            get { return (Brush)GetValue(BackgroundFocusedProperty); }
            set { SetValue(BackgroundFocusedProperty, value); }
        }

        public Brush BackgroundSelected
        {
            get { return (Brush)GetValue(BackgroundSelectedProperty); }
            set { SetValue(BackgroundSelectedProperty, value); }
        }

        public Brush ForegroundSelected
        {
            get { return (Brush)GetValue(ForegroundSelectedProperty); }
            set { SetValue(ForegroundSelectedProperty, value); }
        }

        public Brush BackgroundSelectedHovered
        {
            get { return (Brush)GetValue(BackgroundSelectedHoveredProperty); }
            set { SetValue(BackgroundSelectedHoveredProperty, value); }
        }

        public Brush BackgroundHovered
        {
            get { return (Brush)GetValue(BackgroundHoveredProperty); }
            set { SetValue(BackgroundHoveredProperty, value); }
        }

        public Brush BackgroundInactive
        {
            get { return (Brush)GetValue(BackgroundInactiveProperty); }
            set { SetValue(BackgroundInactiveProperty, value); }
        }

        public Brush ForegroundInactive
        {
            get { return (Brush)GetValue(ForegroundInactiveProperty); }
            set { SetValue(ForegroundInactiveProperty, value); }
        }

        public Brush BorderBrushInactive
        {
            get { return (Brush)GetValue(BorderBrushInactiveProperty); }
            set { SetValue(BorderBrushInactiveProperty, value); }
        }

        public Brush BorderBrushHovered
        {
            get { return (Brush)GetValue(BorderBrushHoveredProperty); }
            set { SetValue(BorderBrushHoveredProperty, value); }
        }

        public Brush BorderBrushFocused
        {
            get { return (Brush)GetValue(BorderBrushFocusedProperty); }
            set { SetValue(BorderBrushFocusedProperty, value); }
        }

        public Brush BorderBrushSelected
        {
            get { return (Brush)GetValue(BorderBrushSelectedProperty); }
            set { SetValue(BorderBrushSelectedProperty, value); }
        }

        #endregion Brushes

        #region Others

        public DataTemplate ContentTemplateEdit
        {
            get { return (DataTemplate)GetValue(ContentTemplateEditProperty); }
            set { SetValue(ContentTemplateEditProperty, value); }
        }

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        public new bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        public bool HoverHighlighting
        {
            get { return (bool)GetValue(HoverHighlightingProperty); }
            set { SetValue(HoverHighlightingProperty, value); }
        }

        public int ItemIndent
        {
            get { return (int)GetValue(ItemIndentProperty); }
            set { SetValue(ItemIndentProperty, value); }
        }

        public bool IsKeyboardMode
        {
            get { return (bool)GetValue(IsKeyboardModeProperty); }
            set { SetValue(IsKeyboardModeProperty, value); }
        }

        public object Remarks
        {
            get { return GetValue(RemarksProperty); }
            set { SetValue(RemarksProperty, value); }
        }

        public DataTemplate RemarksTemplate
        {
            get { return (DataTemplate)GetValue(RemarksTemplateProperty); }
            set { SetValue(RemarksTemplateProperty, value); }
        }

        internal SelectionCheckState SelectionCheckState
        {
            get { return (SelectionCheckState)GetValue(SelectionCheckStateProperty); }
            set { SetValue(SelectionCheckStateProperty, value); }
        }

        #endregion Others

        #endregion

        #region Non-public properties

        private MultiSelectTreeView _lastParentTreeView;

        internal MultiSelectTreeView ParentTreeView
        {
            get
            {
                if (_lastParentTreeView != null)
                {
                    return _lastParentTreeView;
                }

                for (ItemsControl itemsControl = ParentItemsControl;
                     itemsControl != null;
                     itemsControl = ItemsControlFromItemContainer(itemsControl))
                {
                    MultiSelectTreeView treeView = itemsControl as MultiSelectTreeView;
                    if (treeView != null)
                    {
                        return _lastParentTreeView = treeView;
                    }
                }

                return null;
            }
        }

        private static bool IsControlKeyDown
        {
            get { return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control; }
        }

        private static bool IsShiftKeyDown
        {
            get { return (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift; }
        }

        private bool CanExpand
        {
            get { return HasItems; }
        }

        private bool CanExpandOnInput
        {
            get { return CanExpand && IsEnabled; }
        }

        private ItemsControl ParentItemsControl
        {
            get { return ItemsControlFromItemContainer(this); }
        }

        #endregion Non-public properties

        #region Public methods

        public override string ToString()
        {
            if (DataContext != null)
            {
                return string.Format("{0} ({1})", DataContext, base.ToString());
            }

            return base.ToString();
        }

        #endregion Public methods

        #region Protected methods

        protected static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // The item has been selected through its IsSelected property. Update the SelectedItems
            // list accordingly (this is the authoritative collection). No PreviewSelectionChanged
            // event is fired - the item is already selected.
            MultiSelectTreeViewItem item = d as MultiSelectTreeViewItem;
            if (item != null)
            {
                if ((bool)e.NewValue)
                {
                    if (!item.ParentTreeView.IsItemSelected(item.DataContext))
                    {
                        item.ParentTreeView.SelectItem(item.DataContext);
                    }

                    item.BringIntoView();
                    //todo(lw)
                    if (item.ParentTreeView.IsAbleToExpand())
                    {
                        item.Focus();
                    }
                }
                else
                {
                    item.ParentTreeView.DeselectItem(item.DataContext);
                }
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ParentTreeView == null) return;

            //System.Diagnostics.Debug.WriteLine("P(" + ParentTreeView.Name + "): " + e.Property + " " + e.NewValue);
            if (e.Property.Name == "IsEditing")
            {
                if ((bool)e.NewValue == false)
                {
                    StopEditing();
                }
            }

            if (e.Property.Name == "IsExpanded")
            {
                // Bring newly expanded child nodes into view if they'd be outside of the current view
                if ((bool)e.NewValue == true)
                {
                    if (VisualChildrenCount > 0)
                    {
                        ((FrameworkElement)GetVisualChild(VisualChildrenCount - 1)).BringIntoView();
                    }
                }

                // Deselect children of collapsed item
                // (If one resists, don't collapse)
                if ((bool)e.NewValue == false)
                {
                    // if (!ParentTreeView.DeselectRecursive(this, false))
                    // {
                    //     IsExpanded = true;
                    // }
                }
            }

            if (e.Property.Name == "IsVisible" && ParentTreeView.IsDropDownOpen)
            {
                // Deselect invisible item and its children
                // (If one resists, don't hide)
                
                // if ((bool)e.NewValue == false)
                // {
                //     if (!ParentTreeView.DeselectRecursive(this, true))
                //     {
                //         IsVisible = true;
                //     }
                // }
            }

            base.OnPropertyChanged(e);
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            // Remove all items from the SelectedItems list that are no longer in the Items
            if (oldValue != null)
            {
                MultiSelectTreeView parentTV = ParentTreeView;
                if (parentTV == null)
                    parentTV = _lastParentTreeView;
                if (parentTV != null)
                {
                    foreach (var item in oldValue)
                    {
                        parentTV.DeselectItem(item);
                        var multiselection = parentTV.Selection as SelectionMultiple;
                        if (multiselection != null)
                        {
                            multiselection.InvalidateLastShiftRoot(item);
                        }
                    }
                }
            }
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
            element.SetValue(MultiSelectTreeView.SelectItemByCheckBoxProperty, ParentTreeView.SelectItemByCheckBox);
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MultiSelectTreeViewItem;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new MultiSelectTreeViewItemAutomationPeer(this);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            FrameworkElement itemContent = (FrameworkElement)Template.FindName("PART_Header", this);
            if (itemContent == null || !((FrameworkElement)itemContent.Parent).IsMouseOver)
            {
                // A (probably disabled) child item was really clicked, do nothing here
                return;
            }

            if (IsKeyboardFocused && e.ChangedButton == MouseButton.Left) IsExpanded = !IsExpanded;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled)
            {
                Key key = e.Key;
                switch (key)
                {
                    case Key.Left:
                        if (IsExpanded)
                        {
                            IsExpanded = false;
                        }
                        else
                        {
                            ParentTreeView.Selection.SelectParentFromKey();
                        }

                        e.Handled = true;
                        break;
                    case Key.Right:
                        if (CanExpand)
                        {
                            if (!IsExpanded)
                            {
                                IsExpanded = true;
                            }
                            else
                            {
                                ParentTreeView.Selection.SelectNextFromKey();
                            }
                        }

                        e.Handled = true;
                        break;
                    case Key.Up:
                        ParentTreeView.Selection.SelectPreviousFromKey();
                        e.Handled = true;
                        break;
                    case Key.Down:
                        ParentTreeView.Selection.SelectNextFromKey();
                        e.Handled = true;
                        break;
                    case Key.Home:
                        ParentTreeView.Selection.SelectFirstFromKey();
                        e.Handled = true;
                        break;
                    case Key.End:
                        ParentTreeView.Selection.SelectLastFromKey();
                        e.Handled = true;
                        break;
                    case Key.PageUp:
                        ParentTreeView.Selection.SelectPageUpFromKey();
                        e.Handled = true;
                        break;
                    case Key.PageDown:
                        ParentTreeView.Selection.SelectPageDownFromKey();
                        e.Handled = true;
                        break;
                    case Key.A:
                        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                        {
                            ParentTreeView.Selection.SelectAllFromKey();
                            e.Handled = true;
                        }

                        break;
                    case Key.Add:
                        if (CanExpandOnInput && !IsExpanded)
                        {
                            IsExpanded = true;
                        }

                        e.Handled = true;
                        break;
                    case Key.Multiply:
                        if (CanExpandOnInput && !IsExpanded)
                        {
                            IsExpanded = true;
                            ParentTreeView.RecursiveExpand(this);
                        }

                        e.Handled = true;
                        break;
                    case Key.Subtract:
                        if (CanExpandOnInput && IsExpanded)
                        {
                            IsExpanded = false;
                        }

                        e.Handled = true;
                        break;
                    case Key.F2:
                        if (ParentTreeView.AllowEditItems && ContentTemplateEdit != null && IsFocused && IsEditable)
                        {
                            IsEditing = true;
                            e.Handled = true;
                        }

                        break;
                    case Key.Escape:
                        StopEditing();
                        e.Handled = true;
                        break;
                    case Key.Return:
                        FocusHelper.Focus(this, true);
                        IsEditing = false;
                        e.Handled = true;
                        break;
                    case Key.Space:
                        ParentTreeView.Selection.SelectCurrentBySpace();
                        e.Handled = true;
                        break;
                }
            }
        }

        private void StopEditing()
        {
            FocusHelper.Focus(this, true);
            IsEditing = false;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            // Do not call the base method because it would bring all of its children into view on
            // selecting which is not the desired behaviour.
            //base.OnGotFocus(e);
            ParentTreeView.LastFocusedItem = this;
            //System.Diagnostics.Debug.WriteLine("MultiSelectTreeViewItem.OnGotFocus(), DisplayName = " + DisplayName);
            //System.Diagnostics.Debug.WriteLine(Environment.StackTrace);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            IsEditing = false;
            //System.Diagnostics.Debug.WriteLine("MultiSelectTreeViewItem.OnLostFocus(), DisplayName = " + DisplayName);
            //System.Diagnostics.Debug.WriteLine(Environment.StackTrace);
        }

        private bool IsSelectItemByCheckBox => (bool)GetValue(MultiSelectTreeView.SelectItemByCheckBoxProperty);

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (HasItems || IsSelectItemByCheckBox)
            {
                e.Handled = true;
                return;
            }
            
            //System.Diagnostics.Debug.WriteLine("MultiSelectTreeViewItem.OnMouseDown(Item = " + this.DisplayName + ", Button = " + e.ChangedButton + ")");
            base.OnMouseDown(e);

            FrameworkElement itemContent = (FrameworkElement)Template.FindName("PART_Header", this);
            
            //comment the  code
            // if (itemContent == null || !((FrameworkElement)itemContent.Parent).IsMouseOver)
            // {
            //     // A (probably disabled) child item was really clicked, do nothing here
            //     return;
            // }

            if (e.ChangedButton == MouseButton.Left)
            {
                ParentTreeView.Selection.Select(this);
                e.Handled = true;
            }

            if (e.ChangedButton == MouseButton.Right)
            {
                if (!IsSelected)
                {
                    ParentTreeView.Selection.Select(this);
                }

                e.Handled = true;
            }
        }


        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            MultiSelectTreeView parentTV;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    // Remove all items from the SelectedItems list that have been removed from the
                    // Items list
                    parentTV = ParentTreeView;
                    if (parentTV == null)
                        parentTV = _lastParentTreeView;
                    if (parentTV != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            parentTV.DeselectItem(item);
                            var multiselection = parentTV.Selection as SelectionMultiple;
                            if (multiselection != null)
                            {
                                multiselection.InvalidateLastShiftRoot(item);
                            }
                            // Don't preview and ask, it is already gone so it must be removed from
                            // the SelectedItems list
                        }
                    }

                    break;
            }

            base.OnItemsChanged(e);
        }

        #endregion Protected methods

        #region Internal methods

        internal void InvokeMouseDown()
        {
            var e = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right);
            e.RoutedEvent = Mouse.MouseDownEvent;
            OnMouseDown(e);
        }

        internal bool SelectItemByCheckBox => ParentTreeView.SelectItemByCheckBox;

        #endregion Internal methods
    }
}