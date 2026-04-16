using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WindowsDev.Controls.Users
{
    /// <summary>
    /// Interaction logic for TextBlock.xaml
    /// </summary>
    public partial class MyTextBlock : UserControl
    {
        public MyTextBlock()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HeaderProperty =
               DependencyProperty.Register(nameof(Header), typeof(string), typeof(MyTextBlock));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty FieldMarginProperty =
            DependencyProperty.Register(nameof(FieldMargin), typeof(double), typeof(MyTextBlock));

        public Thickness FieldMargin
        {
            get => (Thickness)GetValue(FieldMarginProperty);
            set => SetValue(FieldMarginProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(MyTextBlock));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(MyTextBlock));
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static new readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(MyTextBlock));

        public new Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        public static readonly DependencyProperty WrappingProperty =
            DependencyProperty.Register(nameof(Wrapping), typeof(TextWrapping), typeof(MyTextBlock));

        public TextWrapping Wrapping
        {
            get => (TextWrapping)GetValue(WrappingProperty);
            set => SetValue(WrappingProperty, value);
        }
    }
}


