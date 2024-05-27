namespace System.Windows.Controls
{
    public partial class MultiSelectTreeViewItem
    {
        private const string GridLayoutName = "Part_LayoutGrid";
        private const string CheckBoxName = "SelectionCheckBox";
        internal CheckBox CheckBox { get; private set; }
        internal Grid LayoutGrid { get; private set; }
        private static readonly DependencyPropertyKey IndentMarginPropertyKey =
            DependencyProperty.RegisterReadOnly("IndentMargin", typeof(Thickness), typeof(MultiSelectTreeViewItem),
                new FrameworkPropertyMetadata(new Thickness()));

        public static readonly DependencyProperty IndentMarginProperty = IndentMarginPropertyKey.DependencyProperty;

        private Thickness IndentMargin
        {
            get { return (Thickness)GetValue(IndentMarginProperty); }
            set { SetValue(IndentMarginPropertyKey, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            LayoutGrid = (Grid)GetTemplateChild(GridLayoutName);
            CheckBox = (CheckBox)GetTemplateChild(CheckBoxName);
            CheckBox.Checked += CheckBoxOnChecked;
            CheckBox.Unchecked += CheckBoxOnUnchecked;
        }

        private void CheckBoxOnUnchecked(object sender, RoutedEventArgs e)
        {
            if (!ParentTreeView.SelectItemByCheckBox)
            {
                return;
            }
            
            ParentTreeView.Selection.Deselect(this);
        }

        private void CheckBoxOnChecked(object sender, RoutedEventArgs e)
        {
            if (!ParentTreeView.SelectItemByCheckBox)
            {
                return;
            }

            ParentTreeView.Selection.Select(this);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (!IsUnderMultiSelectTreeView(ParentTreeView))
            {
                return;
            }

            var lastCalculatedIndentMargin = IndentMargin;
            var currentIndentWidth = -CalculateIndentWidthOfParentGrid();
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (lastCalculatedIndentMargin.Left == currentIndentWidth)
            {
                return;
            }

            lastCalculatedIndentMargin.Left = currentIndentWidth;
            IndentMargin = lastCalculatedIndentMargin;
        } 
        
        private static bool IsUnderMultiSelectTreeView(FrameworkElement parent)
        {
            return parent is MultiSelectTreeView;
        }

        private double CalculateIndentWidthOfParentGrid()
        {
            var parentHost = ItemsControlHost as MultiSelectTreeViewItem;
            var accumulativeIndentWidth = 0d;
            var achor = this;
            while (parentHost != null)
            {
                var relativeLocation = achor.LayoutGrid.TranslatePoint(new Point(0, 0), parentHost.LayoutGrid);
                var currentMargin = relativeLocation.X;
                accumulativeIndentWidth += currentMargin;
                achor = parentHost;
                parentHost = parentHost.ItemsControlHost as MultiSelectTreeViewItem;
            }

            return accumulativeIndentWidth;
        }
        
        private ItemsControl _itemsControlHost;

        private ItemsControl ItemsControlHost
        {
            get
            {
                if (_itemsControlHost != null)
                {
                    return _itemsControlHost;
                }

                _itemsControlHost = ItemsControl.ItemsControlFromItemContainer(this);
                return _itemsControlHost;
            }
        }
    }
}