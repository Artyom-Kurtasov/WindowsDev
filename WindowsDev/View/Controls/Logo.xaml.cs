using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace WindowsDev.View.Controls
{
    /// <summary>
    /// Логика взаимодействия для Logo.xaml
    /// </summary>
    public partial class Logo : UserControl
    {
        public Logo()
        {
            InitializeComponent();
        }

        public static DependencyProperty ViewBoxWidthProperty = DependencyProperty.Register(nameof(ViewBoxWidth), typeof(double), typeof(Logo), new PropertyMetadata(null));

        public double ViewBoxWidth
        {
            get => (double)GetValue(ViewBoxWidthProperty);
            set => SetValue(ViewBoxWidthProperty, value);
        }

        public static DependencyProperty ViewBoxHeightProperty = DependencyProperty.Register(nameof(ViewBoxHeight), typeof(double), typeof(Logo), new PropertyMetadata(null));

        public double ViewBoxHeight
        {
            get => (double)GetValue(ViewBoxHeightProperty);
            set => SetValue(ViewBoxHeightProperty, value);
        }

        public static DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(Logo), new PropertyMetadata(null));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static DependencyProperty KindProperty = DependencyProperty.Register(nameof(Kind), typeof(PackIconMaterialDesignKind), typeof(Logo));

        public PackIconMaterialDesignKind Kind
        {
            get => (PackIconMaterialDesignKind)GetValue(KindProperty);
            set => SetValue(KindProperty, value);
        }
    }
}
