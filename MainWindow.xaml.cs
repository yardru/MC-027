using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly int[] baudRates = {
            9600,
            19200,
            38400,
            57600,
            115200,
            230400,
            460800,
        };
        private const byte maxModbusAddress = 247;

        private void DoBindings()
        {
            Binding connectionBindnig = new Binding("IsConnected") { Source = modbus, Mode = BindingMode.OneWay };
            StartStopButton.SetBinding(ToggleButton.IsCheckedProperty, connectionBindnig);
            TestIndicationButton.SetBinding(IsEnabledProperty, connectionBindnig);
            CalibrateKeyboardButton.SetBinding(IsEnabledProperty, connectionBindnig);
            EnableTunesButton.SetBinding(IsEnabledProperty, connectionBindnig);
            ResolversParams.SetBinding(IsEnabledProperty, connectionBindnig);
            RegulatorParams.SetBinding(IsEnabledProperty, connectionBindnig);

            Resolver1Angle.SetBinding(TextBlock.TextProperty, new Binding("Angle1Str") { Source = moduleInfo });
            Resolver2Angle.SetBinding(TextBlock.TextProperty, new Binding("Angle2Str") { Source = moduleInfo });
            TestIndicationButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsTestIndication") { Source = moduleInfo, Mode = BindingMode.OneWay });
            EnableTunesButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsTunesEnabled") { Source = moduleInfo, Mode = BindingMode.OneWay });
        }

        public MainWindow()
        {
            modbus = new ModbusInterface();
            moduleInfo = new ModuleInfo();

            InitializeComponent();

            ComPort.ItemsSource = SerialPort.GetPortNames();
            ComPort.SelectedIndex = 0;
            ModbusSpeed.ItemsSource = baudRates;
            ModbusSpeed.SelectedIndex = 2; // 38400
            ModbusAddress.Items.Add("0 - Broadcast Exchange");
            for (int i = 1; i <= maxModbusAddress; i++)
                ModbusAddress.Items.Add(i);
            ModbusAddress.SelectedIndex = 0;
            DoBindings();
        }

        public void SetStartStopButton(bool state)
        {
            StartStopButton.Content = state ? "Stop" : "Start";
        }

        private ModbusInterface modbus;
        private ModuleInfo moduleInfo;

        private void ComPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            modbus.PortName = ComPort.SelectedValue?.ToString();
        }

        private void ModbusSpeed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            modbus.BaudRate = (int)ModbusSpeed.SelectedValue;
        }

        private void ModbusAddress_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            modbus.SlaveAddress = (byte)(int)ModbusAddress.SelectedIndex;
        }

        private void ResolversParams_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!moduleInfo.IsTunesEnabled)
            {
                MessageBox.Show("Tunes are disabled.");
                return;
            }

            ResolversParam param = ResolversParams.SelectedValue as ResolversParam;
            ResolversParamChangeDialog dialog = new ResolversParamChangeDialog(param)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                modbus.WriteResolversParam(param);
                modbus.ReadResolversParams(ResolversParams.Items);
            }
        }

        private void TestIndicationButton_Click(object sender, RoutedEventArgs e)
        {
            modbus.TestIndication(!moduleInfo.IsTestIndication);
        }

        private void CalibrateKeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            modbus.CalibrateKeyboard();
        }

        private void EnableTunesButton_Click(object sender, RoutedEventArgs e)
        {
            modbus.EnableTunes(!moduleInfo.IsTunesEnabled);
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (modbus.IsConnected)
            {
                modbus.Stop();
                SetStartStopButton(false);
                return;
            }

            if (!modbus.Start(moduleInfo))
            {
                //MessageBox.Show("Error");
                return;
            }

            modbus.ReadResolversParams(ResolversParams.Items);
            SetStartStopButton(true);
        }
    }
}