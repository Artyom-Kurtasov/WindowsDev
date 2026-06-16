using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WindowsDev.Business.Services.Registration.Validation;

namespace WindowsDev.Controls.Users
{
    public partial class HeaderPasswordBox : UserControl
    {
        public HeaderPasswordBox()
        {
            InitializeComponent();

            SetResourceReference(HeaderForegroundProperty, "MahApps.Brushes.ThemeForeground");
        }

        public static DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(HeaderPasswordBox), new PropertyMetadata(null));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static DependencyProperty HeaderFontSizeProperty =
            DependencyProperty.Register(nameof(HeaderFontSize), typeof(int), typeof(HeaderPasswordBox), new PropertyMetadata(12));

        public int HeaderFontSize
        {
            get => (int)GetValue(HeaderFontSizeProperty);
            set => SetValue(HeaderFontSizeProperty, value);
        }

        public static DependencyProperty HorizontalAlignmentHyperlinkProperty =
            DependencyProperty.Register(nameof(HorizontalAlignmentHyperlink), typeof(HorizontalAlignment), typeof(HeaderPasswordBox));

        public HorizontalAlignment HorizontalAlignmentHyperlink
        {
            get => (HorizontalAlignment)GetValue(HorizontalAlignmentHyperlinkProperty);
            set => SetValue(HorizontalAlignmentHyperlinkProperty, value);
        }

        public static DependencyProperty LinkTextProperty =
            DependencyProperty.Register(nameof(LinkText), typeof(string), typeof(HeaderPasswordBox));

        public string LinkText
        {
            get => (string)GetValue(LinkTextProperty);
            set => SetValue(LinkTextProperty, value);
        }

        public static DependencyProperty LinkCommandProperty =
            DependencyProperty.Register(nameof(LinkCommand), typeof(ICommand), typeof(HeaderPasswordBox));

        public ICommand LinkCommand
        {
            get => (ICommand)GetValue(LinkCommandProperty);
            set => SetValue(LinkCommandProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HeaderPasswordBox));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register(nameof(Watermark), typeof(string), typeof(HeaderPasswordBox));

        public string Watermark
        {
            get => (string)GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(nameof(Password), typeof(string), typeof(HeaderPasswordBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordChanged));

        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty IsValidationEnabledProperty =
            DependencyProperty.Register(nameof(IsValidationEnabled), typeof(bool), typeof(HeaderPasswordBox));

        public bool IsValidationEnabled
        {
            get => (bool)GetValue(IsValidationEnabledProperty);
            set => SetValue(IsValidationEnabledProperty, value);
        }

        public static readonly DependencyProperty HeaderForegroundProperty =
            DependencyProperty.Register(nameof(HeaderForeground), typeof(Brush), typeof(HeaderPasswordBox));

        public Brush HeaderForeground
        {
            get => (Brush)GetValue(HeaderForegroundProperty);
            set => SetValue(HeaderForegroundProperty, value);
        }

        public static readonly DependencyProperty HasMinimumLengthProperty =
            DependencyProperty.Register(nameof(HasMinimumLength), typeof(bool), typeof(HeaderPasswordBox));

        public bool HasMinimumLength
        {
            get => (bool)GetValue(HasMinimumLengthProperty);
            set => SetValue(HasMinimumLengthProperty, value);
        }

        public static readonly DependencyProperty HasNumberProperty =
            DependencyProperty.Register(nameof(HasNumber), typeof(bool), typeof(HeaderPasswordBox));

        public bool HasNumber
        {
            get => (bool)GetValue(HasNumberProperty);
            set => SetValue(HasNumberProperty, value);
        }

        public static readonly DependencyProperty HasUppercaseProperty =
            DependencyProperty.Register(nameof(HasUppercase), typeof(bool), typeof(HeaderPasswordBox));

        public bool HasUppercase
        {
            get => (bool)GetValue(HasUppercaseProperty);
            set => SetValue(HasUppercaseProperty, value);
        }

        public static readonly DependencyProperty HasSymbolProperty =
            DependencyProperty.Register(nameof(HasSymbol), typeof(bool), typeof(HeaderPasswordBox));

        public bool HasSymbol
        {
            get => (bool)GetValue(HasSymbolProperty);
            set => SetValue(HasSymbolProperty, value);
        }

        public static readonly DependencyProperty ShowRequirementsProperty =
            DependencyProperty.Register(nameof(ShowRequirements), typeof(bool), typeof(HeaderPasswordBox));

        public bool ShowRequirements
        {
            get => (bool)GetValue(ShowRequirementsProperty);
            set => SetValue(ShowRequirementsProperty, value);
        }

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (HeaderPasswordBox)d;

            string password = control.Password ?? string.Empty;

            control.HasMinimumLength = PasswordValidator.HasMinimumLength(password);
            control.HasNumber = PasswordValidator.HasNumber(password);
            control.HasUppercase = PasswordValidator.HasUppercase(password);
            control.HasSymbol = PasswordValidator.HasSymbol(password);
        }
    }
}



