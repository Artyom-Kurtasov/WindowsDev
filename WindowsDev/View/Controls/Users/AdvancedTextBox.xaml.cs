using System.Windows;
using System.Windows.Controls;

namespace WindowsDev.View.Controls
{
    /// <summary>
    /// Логика взаимодействия для AdvancedTextBlock.xaml
    /// </summary>
    public partial class AdvancedTextBox : UserControl
    {
        public AdvancedTextBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(AdvancedTextBox));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty FieldMarginProperty =
            DependencyProperty.Register(nameof(FieldMargin), typeof(double), typeof(AdvancedTextBox));

        public Thickness FieldMargin
        {
            get => (Thickness)GetValue(FieldMarginProperty);
            set => SetValue(FieldMarginProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(AdvancedTextBox));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping), typeof(AdvancedTextBox), new PropertyMetadata(TextWrapping.NoWrap));

        public TextWrapping TextWrapping
        {
            get => (TextWrapping)GetValue(TextWrappingProperty);
            set => SetValue(TextWrappingProperty, value);
        }

        public static readonly DependencyProperty ScrollBarVisibilityProperty =
            DependencyProperty.Register(nameof(ScrollBarVisibility), typeof(ScrollBarVisibility), typeof(AdvancedTextBox));

        public ScrollBarVisibility ScrollBarVisibility
        {
            get => (ScrollBarVisibility)GetValue(ScrollBarVisibilityProperty);
            set => SetValue(ScrollBarVisibilityProperty, value);
        }

        public static readonly DependencyProperty FieldHeightProperty =
            DependencyProperty.Register(nameof(FieldHeight), typeof(double), typeof(AdvancedTextBox));

        public double FieldHeight
        {
            get => (double)GetValue(FieldHeightProperty);
            set => SetValue(FieldHeightProperty, value);
        }

        public static readonly DependencyProperty UseWatermarkProperty =
            DependencyProperty.Register(nameof(UseWatermark), typeof(bool), typeof(AdvancedTextBox));

        public bool UseWatermark
        {
            get => (bool)GetValue(UseWatermarkProperty);
            set => SetValue(UseWatermarkProperty, value);
        }

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register(nameof(Watermark), typeof(string), typeof(AdvancedTextBox));

        public string Watermark
        {
            get => (string)GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
                DependencyProperty.Register(nameof(Text), typeof(string), typeof(AdvancedTextBox),
                    new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}

