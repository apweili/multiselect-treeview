using System.ComponentModel;
using System.Windows.Threading;

namespace System.Windows.Controls
{
    public partial class MultiSelectTreeView
    {
        public static readonly DependencyProperty MaxDropDownHeightProperty
            = DependencyProperty.Register("MaxDropDownHeight", typeof(double), typeof(MultiSelectTreeView),
                new FrameworkPropertyMetadata(SystemParameters.PrimaryScreenHeight / 3));

        /// <summary>
        ///     The maximum height of the popup
        /// </summary>
        [Bindable(true), Category("Layout")]
        [TypeConverter(typeof(LengthConverter))]
        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for IsDropDownOpen
        /// </summary>
        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register(
                "IsDropDownOpen",
                typeof(bool),
                typeof(MultiSelectTreeView),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnIsDropDownOpenChanged),
                    new CoerceValueCallback(CoerceIsDropDownOpen)));

        private static object CoerceIsDropDownOpen(DependencyObject d, object value)
        {
            if (!(bool)value)
            {
                return value;
            }
            
            var cb = (MultiSelectTreeView) d;
            if (!cb.IsLoaded)
            {
                cb.RegisterToOpenOnLoad();
                return false;
            }

            return value;
        }
        
        private void RegisterToOpenOnLoad()
        {
            Loaded += OpenOnLoad;
        }
 
        private void OpenOnLoad(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(() => CoerceValue(IsDropDownOpenProperty), DispatcherPriority.Input);
        }

        private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Whether or not the "popup" for this control is currently open
        /// </summary>
        [Bindable(true), Browsable(false), Category("Appearance")]
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, false); }
        }

        private static readonly DependencyPropertyKey SelectionBoxItemStringFormatPropertyKey =
            DependencyProperty.RegisterReadOnly("SelectionBoxItemStringFormat", typeof(String),
                typeof(MultiSelectTreeView),
                new FrameworkPropertyMetadata((String)null));

        /// <summary>
        /// The DependencyProperty for the SelectionBoxItemProperty
        /// </summary>
        public static readonly DependencyProperty SelectionBoxItemStringFormatProperty =
            SelectionBoxItemStringFormatPropertyKey.DependencyProperty;

        /// <summary>
        /// Used to set the item DataStringFormat
        /// </summary>
        public String SelectionBoxItemStringFormat
        {
            get { return (String)GetValue(SelectionBoxItemStringFormatProperty); }
            private set { SetValue(SelectionBoxItemStringFormatPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey SelectionBoxItemTemplatePropertyKey =
            DependencyProperty.RegisterReadOnly("SelectionBoxItemTemplate", typeof(DataTemplate),
                typeof(MultiSelectTreeView),
                new FrameworkPropertyMetadata((DataTemplate)null));

        /// <summary>
        /// The DependencyProperty for the SelectionBoxItemProperty
        /// </summary>
        public static readonly DependencyProperty SelectionBoxItemTemplateProperty =
            SelectionBoxItemTemplatePropertyKey.DependencyProperty;

        /// <summary>
        /// Used to set the item DataTemplate
        /// </summary>
        public DataTemplate SelectionBoxItemTemplate
        {
            get { return (DataTemplate)GetValue(SelectionBoxItemTemplateProperty); }
            private set { SetValue(SelectionBoxItemTemplatePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey SelectionBoxItemPropertyKey =
            DependencyProperty.RegisterReadOnly("SelectionBoxItem", typeof(object), typeof(MultiSelectTreeView),
                new FrameworkPropertyMetadata(string.Empty));
        
        // This property is used as a Style Helper.
        // When the SelectedItem is a UIElement a VisualBrush is created and set to the Fill property
        // of a Rectangle. Then we set SelectionBoxItem to that rectangle.
        // For data items, SelectionBoxItem is set to a string.
        /// <summary>
        /// The DependencyProperty for the SelectionBoxItemProperty
        /// </summary>
        public static readonly DependencyProperty SelectionBoxItemProperty =
            SelectionBoxItemPropertyKey.DependencyProperty;

        /// <summary>
        /// Used to display the selected item
        /// </summary>
        public object SelectionBoxItem
        {
            get { return GetValue(SelectionBoxItemProperty); }
            private set { SetValue(SelectionBoxItemPropertyKey, value); }
        }
        
        private static readonly DependencyPropertyKey IsIncludeRemarkPropertyKey =
            DependencyProperty.RegisterReadOnly("IsIncludeRemark", typeof(bool), typeof(MultiSelectTreeView),
                new FrameworkPropertyMetadata(false));
        
        public static readonly DependencyProperty IsIncludeRemarkProperty =
            IsIncludeRemarkPropertyKey.DependencyProperty;
        
        public bool IsIncludeRemark
        {
            get { return (bool)GetValue(IsIncludeRemarkProperty); }
            private set { SetValue(IsIncludeRemarkPropertyKey, value); }
        }

        /// <summary>
        /// DependencyProperty for IsEditable
        /// </summary>
        public static readonly DependencyProperty IsEditableProperty = ComboBox.IsEditableProperty.AddOwner(
            typeof(MultiSelectTreeView), new FrameworkPropertyMetadata(
                false,
                new PropertyChangedCallback(OnIsEditableChanged)));


        /// <summary>
        ///     True if this ComboBox is editable.
        /// </summary>
        /// <value></value>
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        private static void OnIsEditableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}