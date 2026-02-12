using System.Windows;
using System.Windows.Controls;

namespace WindowsDev.View.Controls
{
    /// <summary>
    /// Логика взаимодействия для AdvancedTextBlock.xaml
    /// </summary>
    public partial class AdvancedTextBlock : UserControl
    {
        public AdvancedTextBlock()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(AdvancedTextBlock));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty FieldMarginProperty = DependencyProperty.Register(nameof(FieldMargin), typeof(double), typeof(AdvancedTextBlock));

        public Thickness FieldMargin
        {
            get => (Thickness)GetValue(FieldMarginProperty);
            set => SetValue(FieldMarginProperty, value);
        }

        public static readonly DependencyProperty FieldMaxWidthProperty = DependencyProperty.Register(nameof(FieldMaxWidth), typeof(double), typeof(AdvancedTextBlock), new PropertyMetadata(250.0));

        public double FieldMaxWidth
        {
            get => (double)GetValue(FieldMaxWidthProperty);
            set => SetValue(FieldMaxWidthProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(AdvancedTextBlock), new PropertyMetadata(new CornerRadius(0)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping), typeof(AdvancedTextBlock), new PropertyMetadata(TextWrapping.NoWrap));

        public TextWrapping TextWrapping
        {
            get => (TextWrapping)GetValue(TextWrappingProperty);
            set => SetValue(TextWrappingProperty, value);
        }

        public static readonly DependencyProperty ScrollBarVisibilityProperty = DependencyProperty.Register(nameof(ScrollBarVisibility),typeof(ScrollBarVisibility), typeof(AdvancedTextBlock));

        public ScrollBarVisibility ScrollBarVisibility
        {
            get => (ScrollBarVisibility)GetValue(ScrollBarVisibilityProperty);
            set => SetValue(ScrollBarVisibilityProperty, value);
        }

        public static readonly DependencyProperty FieldHeightProperty = DependencyProperty.Register(nameof(FieldHeight), typeof(double), typeof(AdvancedTextBlock));

        public double FieldHeight
        {
            get => (double)GetValue(FieldHeightProperty);
            set => SetValue(FieldHeightProperty, value); 
        }
    }
}
