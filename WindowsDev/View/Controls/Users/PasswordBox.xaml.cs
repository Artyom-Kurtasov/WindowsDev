using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
            DependencyProperty.Register(nameof(HeaderFontSize), typeof(int), typeof(PasswordBox), new PropertyMetadata(12));

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
            DependencyProperty.Register(nameof(Password), typeof(string), typeof(PasswordBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordChanged));

        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty IsValidationEnabledProperty =
            DependencyProperty.Register(nameof(IsValidationEnabled), typeof(bool), typeof(PasswordBox));

        public bool IsValidationEnabled
        {
            get => (bool)GetValue(IsValidationEnabledProperty);
            set => SetValue(IsValidationEnabledProperty, value);
        }

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PasswordBox)d;

            if (!control.IsLoaded) return;

            if (control.IsValidationEnabled)
            {
                control.Validate();
            }
        }

        private void Validate()
        {
            if (PART_PasswordBox == null) return;

            var binding = BindingOperations.GetBindingExpression(PART_PasswordBox, PasswordBox.PasswordProperty);
            if (binding == null) return;

            if (!Password.Any(char.IsDigit))
            {
                Validation.MarkInvalid(
                    binding,
                    new ValidationError(new ExceptionValidationRule(), binding, "Any Numbers!", null));
            }
            else
            {
                Validation.ClearInvalid(binding);
            }
        }
    }
}

