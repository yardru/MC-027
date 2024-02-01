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
using System.Windows.Shapes;

namespace MC_027
{
    /// <summary>
    /// Interaction logic for ResolversParamChangeDialog.xaml
    /// </summary>
    public partial class ResolversParamChangeDialog : Window
    {
        private ResolversParam param;

        public ResolversParamChangeDialog(ResolversParam param)
        {
            this.param = param;

            InitializeComponent();
            Title = "Set " + param.Description;
            for (byte i = 0; i <= 0x7f; i++)
            {
                Resolver1Value.Items.Add(param.ValueToString(i));
                Resolver2Value.Items.Add(param.ValueToString(i));
            }
            Resolver1Value.SelectedIndex = param.Value1;
            Resolver2Value.SelectedIndex = param.Value2;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            param.Value1 = (byte)Resolver1Value.SelectedIndex;
            param.Value2 = (byte)Resolver2Value.SelectedIndex;
            DialogResult = true;
        }
    }
}
