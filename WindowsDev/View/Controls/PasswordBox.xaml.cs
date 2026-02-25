using System.Windows;
using System.Windows.Controls;

namespace WindowsDev.View.Controls
{
    /// <summary>
    /// Логика взаимодействия для PasswordBox.xaml
    /// </summary>
    public partial class PasswordBox : UserControl
    {
        public PasswordBox()
        {
            InitializeComponent();
        }

        public static DependencyProperty HeaderProperty = 
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(PasswordBox), new PropertyMetadata(null));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static DependencyProperty HeaderFontSizeProperty = 
            DependencyProperty.Register(nameof(HeaderFontSize), typeof(int), typeof(PasswordBox));
    
        public int HeaderFontSize
        {
            get => (int)GetValue(HeaderFontSizeProperty);
            set => SetValue(HeaderFontSizeProperty, value);
        }

        public static DependencyProperty HorizontalAlignmentHyperlinkProperty = 
            DependencyProperty.Register(nameof(HorizontalAlignmentHyperlink), typeof(HorizontalAlignment), typeof(PasswordBox));

        public HorizontalAlignment HorizontalAlignmentHyperlink
        {
            get => (HorizontalAlignment)GetValue(HorizontalAlignmentHyperlinkProperty);
            set => SetValue(HorizontalAlignmentHyperlinkProperty, value);
        }

        public static DependencyProperty LinkTextProperty = 
            DependencyProperty.Register(nameof(LinkText), typeof(string), typeof(PasswordBox));

        public string LinkText
        {
            get => (string)GetValue(LinkTextProperty);
            set => SetValue(LinkTextProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty = 
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(PasswordBox));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty FieldMaxWidthProperty = 
            DependencyProperty.Register(nameof(FieldMaxWidth), typeof(double), typeof(PasswordBox), new PropertyMetadata(200.0));

        public double FieldMaxWidth
        {
            get => (double)GetValue(FieldMaxWidthProperty);
            set => SetValue(FieldMaxWidthProperty, value);
        }

        public static readonly DependencyProperty FieldMinWidthProperty = 
            DependencyProperty.Register(nameof(FieldMinWidth), typeof(double), typeof(PasswordBox), new PropertyMetadata(200.0));

        public double FieldMinWidth
        {
            get => (double)GetValue(FieldMinWidthProperty);
            set => SetValue(FieldMinWidthProperty, value);
        }

        public static readonly DependencyProperty UseWatermarkProperty = 
            DependencyProperty.Register(nameof(UseWatermark), typeof(bool), typeof(PasswordBox));

        public bool UseWatermark
        {
            get => (bool)GetValue(UseWatermarkProperty);
            set => SetValue(UseWatermarkProperty, value);
        }

        public static readonly DependencyProperty WatermarkProperty = 
            DependencyProperty.Register(nameof(Watermark), typeof(string), typeof(PasswordBox));

        public string Watermark
        {
            get => (string)GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(nameof(Password), typeof(string), typeof(PasswordBox), new PropertyMetadata(string.Empty));

        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }
    }
}

