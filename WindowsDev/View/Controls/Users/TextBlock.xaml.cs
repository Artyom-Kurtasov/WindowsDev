using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WindowsDev.View.Controls
{
    /// <summary>
    /// Interaction logic for TextBlock.xaml
    /// </summary>
    public partial class TextBlock : UserControl
    {
        public TextBlock()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HeaderProperty =
               DependencyProperty.Register(nameof(Header), typeof(string), typeof(TextBlock));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty FieldMarginProperty =
            DependencyProperty.Register(nameof(FieldMargin), typeof(double), typeof(TextBlock));

        public Thickness FieldMargin
        {
            get => (Thickness)GetValue(FieldMarginProperty);
            set => SetValue(FieldMarginProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(TextBlock));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextBlock));
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static new readonly DependencyProperty BorderBrushProperty = 
            DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(TextBlock));

        public new Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        public static new readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register(nameof(BorderThickness), typeof(Thickness), typeof(TextBlock));

        public new Thickness BorderThickness
        {
            get => (Thickness)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        public static new readonly DependencyProperty TextWrappingProperty = 
            DependencyProperty.Register(nameof(Wrapping), typeof(TextWrapping), typeof(TextBlock));

        public new TextWrapping Wrapping
        {
            get => (TextWrapping)GetValue(TextWrappingProperty);
            set => SetValue(TextWrappingProperty, value);
        }
    }
}
