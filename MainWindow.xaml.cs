using PropertyChanged;
using ReactiveUI;
using Splat.ModeDetection;
using System;
using System.IO.Ports;
using System.Reactive.Linq;
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
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace MC_027
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [AddINotifyPropertyChangedInterface]
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

        private readonly Brush RedBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        private readonly Brush GreenBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0));
        private void SubscribeIndicator(Ellipse indicator, ModuleInfo.STATUS statusFlag, Brush brush)
        {
            moduleInfo.WhenAnyValue(x => x.Status).Select(status => status.HasFlag(statusFlag) ? brush : null).
                Subscribe(brush => indicator.Dispatcher.Invoke(() => indicator.Fill = brush));
        }

        public string StartStopButtonStr { set; get; }
        public bool TestIndicationState { set; get; }
        public bool TunesEnabledState { set; get; }
        public string Resolver1AngleStr { set; get; } = string.Empty;
        public string Resolver2AngleStr { set; get; } = string.Empty;
        private static string AngleToStr(float angle)
        {
            return angle.ToString();
        }
        public int RegulatorModeIndex { set; get; }
        public bool IsNotConnected { set; get; }

        private void Reset()
        {
            moduleInfo.Reset();
            TestIndicationState = false;
            TunesEnabledState = false;
        }

        private void DoBindings()
        {
            Binding connectionBindnig = new Binding("IsConnected") { Source = modbus, Mode = BindingMode.OneWay };
            StartStopButton.SetBinding(ToggleButton.IsCheckedProperty, connectionBindnig);
            TestIndicationButton.SetBinding(IsEnabledProperty, connectionBindnig);
            CalibrateKeyboardButton.SetBinding(IsEnabledProperty, connectionBindnig);
            EnableTunesButton.SetBinding(IsEnabledProperty, connectionBindnig);
            modbus.WhenAnyValue(x => x.IsConnected).Subscribe(isConnected =>
            {
                StartStopButtonStr = isConnected ? "Stop" : "Start";
                IsNotConnected = !isConnected;
                if (!isConnected)
                    Reset();
            });
            StartStopButton.SetBinding(ContentProperty, new Binding("StartStopButtonStr"));
            connectionBindnig = new Binding("IsNotConnected") { Mode = BindingMode.OneWay };
            ModbusAddress.SetBinding(IsEnabledProperty, connectionBindnig);
            ModbusSpeed.SetBinding(IsEnabledProperty, connectionBindnig);
            ComPort.SetBinding(IsEnabledProperty, connectionBindnig);

            Binding tunesBinding = new Binding("TunesEnabledState") { Mode = BindingMode.OneWay };
            ResolversParams.SetBinding(IsEnabledProperty, tunesBinding);
            RegulatorParams.SetBinding(IsEnabledProperty, tunesBinding);
            RegulatorMode.SetBinding(IsEnabledProperty, tunesBinding);

            SubscribeIndicator(Resolver1ExtremeSignalIndicator, ModuleInfo.STATUS.RESOLVER_1_EXTREME_SYGNAL, RedBrush);
            SubscribeIndicator(Resolver1WeakSignalIndicator, ModuleInfo.STATUS.RESOLVER_1_WEAK_SYGNAL, RedBrush);
            SubscribeIndicator(Resolver1NoSignalIndicator, ModuleInfo.STATUS.RESOLVER_1_NO_SYGNAL, RedBrush);
            SubscribeIndicator(Resolver2ExtremeSignalIndicator, ModuleInfo.STATUS.RESOLVER_2_EXTREME_SYGNAL, RedBrush);
            SubscribeIndicator(Resolver2WeakSignalIndicator, ModuleInfo.STATUS.RESOLVER_2_WEAK_SYGNAL, RedBrush);
            SubscribeIndicator(Resolver2NoSignalIndicator, ModuleInfo.STATUS.RESOLVER_2_NO_SYGNAL, RedBrush);
            SubscribeIndicator(Break1Indicator, ModuleInfo.STATUS.PWM_BREAK1, RedBrush);
            SubscribeIndicator(Break2Indicator, ModuleInfo.STATUS.PWM_BREAK2, RedBrush);
            SubscribeIndicator(KeyboardUpIndicator, ModuleInfo.STATUS.KEYBOARD_UP, GreenBrush);
            SubscribeIndicator(KeyboardDownIndicator, ModuleInfo.STATUS.KEYBOARD_DOWN, GreenBrush);
            SubscribeIndicator(KeyboardEnterIndicator, ModuleInfo.STATUS.KEYBOARD_ENTER, GreenBrush);
            moduleInfo.WhenAnyValue(x => x.Status).Select(status => status.HasFlag(ModuleInfo.STATUS.TEST_IND)).Subscribe(state => TestIndicationState = state);
            moduleInfo.WhenAnyValue(x => x.Status).Select(status => status.HasFlag(ModuleInfo.STATUS.TUNES_ENABLED)).Subscribe(state => TunesEnabledState = state);

            moduleInfo.WhenAnyValue(x => x.Angle1).Select(angle => AngleToStr(angle)).Subscribe(angleStr => Resolver1AngleStr = angleStr);
            moduleInfo.WhenAnyValue(x => x.Angle2).Select(angle => AngleToStr(angle)).Subscribe(angleStr => Resolver2AngleStr = angleStr);
            Resolver1Angle.SetBinding(TextBlock.TextProperty, new Binding("Resolver1AngleStr"));
            Resolver2Angle.SetBinding(TextBlock.TextProperty, new Binding("Resolver2AngleStr"));

            TestIndicationButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding("TestIndicationState") { Mode = BindingMode.OneWay });
            EnableTunesButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding("TunesEnabledState") { Mode = BindingMode.OneWay });
            FirmwareVersionTextBlock.SetBinding(TextBlock.TextProperty, new Binding("FirmwareVersion") { Source = moduleInfo });
            UniqueIdTextBlock.SetBinding(TextBlock.TextProperty, new Binding("UniqueId") { Source = moduleInfo });

            moduleInfo.WhenAnyValue(x => x.OutputConfig).Select(config => (int)config >> 14).Subscribe(index => RegulatorModeIndex = index);
            RegulatorMode.SetBinding(ComboBox.SelectedIndexProperty, new Binding("RegulatorModeIndex") { Mode = BindingMode.OneWay });
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

        private void TestIndicationButton_Click(object sender, RoutedEventArgs e)
        {
            modbus.TestIndication(!moduleInfo.Status.HasFlag(ModuleInfo.STATUS.TEST_IND));
        }

        private void CalibrateKeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            modbus.CalibrateKeyboard();
        }

        private void EnableTunesButton_Click(object sender, RoutedEventArgs e)
        {
            modbus.EnableTunes(!moduleInfo.Status.HasFlag(ModuleInfo.STATUS.TUNES_ENABLED));
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (modbus.IsConnected)
            {
                modbus.Stop();
                return;
            }

            if (!modbus.Start(moduleInfo))
            {
                //MessageBox.Show("Error");
                return;
            }

            modbus.ReadResolversParams(ResolversParams.Items);
            modbus.ReadRegulatorParams(RegulatorParams.Items);
        }

        private bool skip = false;
        private void RegulatorMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (skip)
            {
                skip = false;
                return;
            }

            if (!modbus.IsConnected)
                return;

            ModuleInfo.OUTPUT_CONFIG newConfig = moduleInfo.OutputConfig;
            newConfig &= ~ModuleInfo.OUTPUT_CONFIG.REGULATOR_MODE;
            newConfig |= (ModuleInfo.OUTPUT_CONFIG)(RegulatorMode.SelectedIndex << 14);
            modbus.SetOutputConfig(newConfig);
            modbus.ReadOutputConfig(moduleInfo);
            if (!moduleInfo.OutputConfig.Equals(newConfig))
            {
                skip = true;
                RegulatorMode.SelectedIndex = RegulatorModeIndex;
            }
        }

        private void ResolversParams_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!moduleInfo.Status.HasFlag(ModuleInfo.STATUS.TUNES_ENABLED))
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

        private void RegulatorParams_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!moduleInfo.Status.HasFlag(ModuleInfo.STATUS.TUNES_ENABLED))
            {
                MessageBox.Show("Tunes are disabled.");
                return;
            }

            var param = RegulatorParams.SelectedValue as RegulatorParam;
            RegulatorParamChangeDialog dialog = new RegulatorParamChangeDialog(param)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                modbus.WriteRegulatorParam(param);
                modbus.ReadRegulatorParams(RegulatorParams.Items);
            }
        }
    }
}