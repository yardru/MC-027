using PropertyChanged;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MC_027
{
    /// <summary>
    /// Interaction logic for Indicator.xaml
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class Indicator : UserControl
    {
        public bool State { get; set; }
        public string Caption { get; set; } = string.Empty;
        public Brush? FillerBrush { get; set; }

        public Indicator()
        {
            InitializeComponent();
            DataContext = this;
            //this.WhenAnyValue(x => x.State).Select(state => state ? FillerBrush : null).
            //    Subscribe(brush => indicator.Dispatcher.Invoke(() => indicator.Fill = brush));
        }
    }
}
