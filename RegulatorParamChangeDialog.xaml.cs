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
    public partial class RegulatorParamChangeDialog : Window
    {
        private RegulatorParam param;

        public RegulatorParamChangeDialog(RegulatorParam param)
        {
            this.param = param;
            InitializeComponent();
            Title = "Set " + param.Description;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                param.ValueStr = ParamValue.Text;
                DialogResult = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
