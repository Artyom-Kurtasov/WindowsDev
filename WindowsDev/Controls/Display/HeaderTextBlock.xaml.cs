using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WindowsDev.Controls.Users
{
    public partial class HeaderTextBlock : UserControl
    {
        public HeaderTextBlock()
        {
            InitializeComponent();

            SetResourceReference(HeaderForegroundProperty, "MahApps.Brushes.ThemeForeground");
        }

        public static readonly DependencyProperty HeaderProperty =
               DependencyProperty.Register(nameof(Header), typeof(string), typeof(HeaderTextBlock));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty FieldMarginProperty =
            DependencyProperty.Register(nameof(FieldMargin), typeof(double), typeof(HeaderTextBlock));

        public Thickness FieldMargin
        {
            get => (Thickness)GetValue(FieldMarginProperty);
            set => SetValue(FieldMarginProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HeaderTextBlock));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(HeaderTextBlock));
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static new readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(HeaderTextBlock));

        public new Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        public static readonly DependencyProperty WrappingProperty =
            DependencyProperty.Register(nameof(Wrapping), typeof(TextWrapping), typeof(HeaderTextBlock));

        public TextWrapping Wrapping
        {
            get => (TextWrapping)GetValue(WrappingProperty);
            set => SetValue(WrappingProperty, value);
        }

        public static readonly DependencyProperty HeaderForegroundProperty =
            DependencyProperty.Register(nameof(HeaderForeground), typeof(Brush), typeof(HeaderTextBlock));

        public Brush HeaderForeground
        {
            get => (Brush)GetValue(HeaderForegroundProperty);
            set => SetValue(HeaderForegroundProperty, value);
        }

        public static readonly DependencyProperty HeaderFontSizeProperty =
            DependencyProperty.Register(nameof(HeaderFontSize), typeof(int), typeof(HeaderTextBlock), new PropertyMetadata(14));

        public int HeaderFontSize
        {
            get => (int)GetValue(HeaderFontSizeProperty);
            set => SetValue(HeaderFontSizeProperty, value);
        }

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register(nameof(Watermark), typeof(string), typeof(HeaderTextBlock));

        public string Watermark
        {
            get => (string)GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }
    }
}


