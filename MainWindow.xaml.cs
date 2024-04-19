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
using static MC_027.ModuleInfo;
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
        private void SubscribeIndicator(Ellipse indicator, ModuleInfo.STATUS statusFlag, Brush brush)
        {
            moduleInfo.WhenAnyValue(x => x.Status).Select(status => status.HasFlag(statusFlag) ? brush : null).
                Subscribe(brush => indicator.Dispatcher.Invoke(() => indicator.Fill = brush));
        }
        private readonly Brush GreenBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0));
        private void SubscribeIndicator(Ellipse indicator, ModuleInfo.SERVICE serviceFlag, Brush brush)
        {
            moduleInfo.WhenAnyValue(x => x.Service).Select(status => status.HasFlag(serviceFlag) ? brush : null).
                Subscribe(brush => indicator.Dispatcher.Invoke(() => indicator.Fill = brush));
        }

        public string StartStopButtonStr { set; get; }
        public bool TestIndicationState { set; get; }
        public bool TunesEnabledState { set; get; }
        public bool PwmEnabledState { set; get; }
        public string Resolver1AngleStr { set; get; } = string.Empty;
        public string Resolver2AngleStr { set; get; } = string.Empty;
        private static string AngleToStr(float angle)
        {
            return angle.ToString();
        }
        public int RegulatorModeIndex { set; get; }
        public int ResolversModeIndex { set; get; }
        public int AngleOffsetIndex { set; get; }
        public bool IsMasterResolver1 { set; get; }
        public bool IsMasterResolver2 { set; get; }
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
                StartStopButtonStr = isConnected ? "Стоп" : "Старт";
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
            RegulatorValues.SetBinding(IsEnabledProperty, tunesBinding);
            RegulatorParams.SetBinding(IsEnabledProperty, tunesBinding);
            RegulatorMode.SetBinding(IsEnabledProperty, tunesBinding);
            ResolversMode.SetBinding(IsEnabledProperty, tunesBinding);
            EnablePwmButton.SetBinding(IsEnabledProperty, tunesBinding);
            MasterResolver1.SetBinding(IsEnabledProperty, tunesBinding);
            MasterResolver2.SetBinding(IsEnabledProperty, tunesBinding);
            AngleOffset.SetBinding(IsEnabledProperty, tunesBinding);

            SubscribeIndicator(NeedToReloadIndicator, ModuleInfo.STATUS.NEED_TO_RELOAD, RedBrush);
            SubscribeIndicator(Resolver1ExtremeSignalIndicator, ModuleInfo.STATUS.RESOLVER_1_EXTREME_SYGNAL, RedBrush);
            SubscribeIndicator(Resolver1WeakSignalIndicator, ModuleInfo.STATUS.RESOLVER_1_WEAK_SYGNAL, RedBrush);
            SubscribeIndicator(Resolver1NoSignalIndicator, ModuleInfo.STATUS.RESOLVER_1_NO_SYGNAL, RedBrush);
            SubscribeIndicator(Resolver2ExtremeSignalIndicator, ModuleInfo.STATUS.RESOLVER_2_EXTREME_SYGNAL, RedBrush);
            SubscribeIndicator(Resolver2WeakSignalIndicator, ModuleInfo.STATUS.RESOLVER_2_WEAK_SYGNAL, RedBrush);
            SubscribeIndicator(Resolver2NoSignalIndicator, ModuleInfo.STATUS.RESOLVER_2_NO_SYGNAL, RedBrush);
            SubscribeIndicator(Break1Indicator, ModuleInfo.STATUS.BREAK1, RedBrush);
            SubscribeIndicator(Break2Indicator, ModuleInfo.STATUS.BREAK2, RedBrush);
            SubscribeIndicator(KeyboardUpIndicator, ModuleInfo.SERVICE.KEYBOARD_UP, GreenBrush);
            SubscribeIndicator(KeyboardDownIndicator, ModuleInfo.SERVICE.KEYBOARD_DOWN, GreenBrush);
            SubscribeIndicator(KeyboardEnterIndicator, ModuleInfo.SERVICE.KEYBOARD_ENTER, GreenBrush);
            moduleInfo.WhenAnyValue(x => x.Service).Select(service => service.HasFlag(ModuleInfo.SERVICE.TEST_IND)).Subscribe(state => TestIndicationState = state);
            moduleInfo.WhenAnyValue(x => x.Status).Select(status => status.HasFlag(ModuleInfo.STATUS.TUNES_ENABLED)).Subscribe(state => TunesEnabledState = state);

            moduleInfo.WhenAnyValue(x => x.DesiredAngle).Subscribe(angle => (RegulatorValues.Items[0] as RegulatorParamFloat).Value = angle);
            moduleInfo.WhenAnyValue(x => x.PwmValue).Subscribe(value => (RegulatorValues.Items[1] as RegulatorParamInt).Value = value);

            moduleInfo.WhenAnyValue(x => x.Angle1).Select(angle => AngleToStr(angle)).Subscribe(angleStr => Resolver1AngleStr = angleStr);
            moduleInfo.WhenAnyValue(x => x.Angle2).Select(angle => AngleToStr(angle)).Subscribe(angleStr => Resolver2AngleStr = angleStr);
            Resolver1Angle.SetBinding(TextBlock.TextProperty, new Binding("Resolver1AngleStr"));
            Resolver2Angle.SetBinding(TextBlock.TextProperty, new Binding("Resolver2AngleStr"));

            TestIndicationButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding("TestIndicationState") { Mode = BindingMode.OneWay });
            EnableTunesButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding("TunesEnabledState") { Mode = BindingMode.OneWay });
            FirmwareVersionTextBlock.SetBinding(TextBlock.TextProperty, new Binding("FirmwareVersion") { Source = moduleInfo });
            UniqueIdTextBlock.SetBinding(TextBlock.TextProperty, new Binding("UniqueId") { Source = moduleInfo });
            EnablePwmButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding("PwmEnabledState") { Mode = BindingMode.OneWay });

            moduleInfo.WhenAnyValue(x => x.OutputConfig).Subscribe(config =>
            {
                RegulatorModeIndex = (int)(config & OUTPUT_CONFIG.REGULATOR_MODE) >> (int)OUTPUT_CONFIG.REGULATOR_MODE_OFFSET;
                ResolversModeIndex = (int)(config & OUTPUT_CONFIG.RESOLVERS_MODE) >> (int)OUTPUT_CONFIG.RESOLVERS_MODE_OFFSET;
                AngleOffsetIndex = (((int)(config & OUTPUT_CONFIG.ANGLE_OFFSET) >> (int)OUTPUT_CONFIG.ANGLE_OFFSET_OFFSET) + 1) / 2;
                IsMasterResolver1 = !config.HasFlag(OUTPUT_CONFIG.MASTER_RESOLVER);
                IsMasterResolver2 = config.HasFlag(OUTPUT_CONFIG.MASTER_RESOLVER);
                PwmEnabledState = config.HasFlag(OUTPUT_CONFIG.PWM_ENABLE);
            });
            RegulatorMode.SetBinding(ComboBox.SelectedIndexProperty, new Binding("RegulatorModeIndex"));
            ResolversMode.SetBinding(ComboBox.SelectedIndexProperty, new Binding("ResolversModeIndex"));
            AngleOffset.SetBinding(ComboBox.SelectedIndexProperty, new Binding("AngleOffsetIndex"));
            MasterResolver1.SetBinding(RadioButton.IsCheckedProperty, new Binding("IsMasterResolver1"));
            MasterResolver2.SetBinding(RadioButton.IsCheckedProperty, new Binding("IsMasterResolver2"));
        }

        public MainWindow()
        {
            modbus = new ModbusInterface();
            moduleInfo = new ModuleInfo();

            //TryFindResource()


            InitializeComponent();
            ComPort.ItemsSource = SerialPort.GetPortNames();
            ComPort.SelectedIndex = 0;
            ModbusSpeed.ItemsSource = baudRates;
            ModbusSpeed.SelectedIndex = 5; // 230400
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
            modbus.TestIndication(!moduleInfo.Service.HasFlag(ModuleInfo.SERVICE.TEST_IND));
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

        private void EnablePwmButton_Click(object sender, RoutedEventArgs e)
        {
            if (!modbus.IsConnected)
                return;

            moduleInfo.OutputConfig ^= ModuleInfo.OUTPUT_CONFIG.PWM_ENABLE;
            modbus.SetOutputConfig(moduleInfo.OutputConfig);
            modbus.ReadOutputConfig(moduleInfo);
        }

        private void RegulatorMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!modbus.IsConnected)
                return;

            RegulatorMode.SelectionChanged -= RegulatorMode_SelectionChanged;
            {
                moduleInfo.OutputConfig = (moduleInfo.OutputConfig & ~ModuleInfo.OUTPUT_CONFIG.REGULATOR_MODE) |
                    (ModuleInfo.OUTPUT_CONFIG)(RegulatorMode.SelectedIndex << (int)ModuleInfo.OUTPUT_CONFIG.REGULATOR_MODE_OFFSET);
                modbus.SetOutputConfig(moduleInfo.OutputConfig);
                modbus.ReadOutputConfig(moduleInfo);
            }
            RegulatorMode.SelectionChanged += RegulatorMode_SelectionChanged;
        }

        private void ResolversMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!modbus.IsConnected)
                return;

            ResolversMode.SelectionChanged -= ResolversMode_SelectionChanged;
            {
                moduleInfo.OutputConfig = (moduleInfo.OutputConfig & ~OUTPUT_CONFIG.RESOLVERS_MODE) |
                (OUTPUT_CONFIG)(ResolversMode.SelectedIndex << (int)OUTPUT_CONFIG.RESOLVERS_MODE_OFFSET);
                modbus.SetOutputConfig(moduleInfo.OutputConfig);
                modbus.ReadOutputConfig(moduleInfo);
            }
            ResolversMode.SelectionChanged += ResolversMode_SelectionChanged;
        }

        private void AngleOffset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!modbus.IsConnected)
                return;

            AngleOffset.SelectionChanged -= AngleOffset_SelectionChanged;
            {
                int angleOffsetConfig = AngleOffset.SelectedIndex;
                if (angleOffsetConfig > 0)
                    angleOffsetConfig++;
                moduleInfo.OutputConfig = (moduleInfo.OutputConfig & ~OUTPUT_CONFIG.ANGLE_OFFSET) |
                    (OUTPUT_CONFIG)(angleOffsetConfig << (int)OUTPUT_CONFIG.ANGLE_OFFSET_OFFSET);
                modbus.SetOutputConfig(moduleInfo.OutputConfig);
                modbus.ReadOutputConfig(moduleInfo);
            }
            AngleOffset.SelectionChanged += AngleOffset_SelectionChanged;
        }

        private void MasterResolver_Checked(object sender, RoutedEventArgs e)
        {
            if (!modbus.IsConnected)
                return;

            RadioButton another = sender == MasterResolver1 ? MasterResolver2 : MasterResolver1;
            moduleInfo.OutputConfig ^= ModuleInfo.OUTPUT_CONFIG.MASTER_RESOLVER;
            modbus.SetOutputConfig(moduleInfo.OutputConfig);
            another.Checked -= MasterResolver_Checked;
            {
                modbus.ReadOutputConfig(moduleInfo);
            }
            another.Checked += MasterResolver_Checked;
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

            var param = (sender as DataGrid)?.SelectedValue as RegulatorParam;
            RegulatorParamChangeDialog dialog = new RegulatorParamChangeDialog(param)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                modbus.WriteRegulatorParam(param);
                if (sender == RegulatorParams)
                    modbus.ReadRegulatorParams(RegulatorParams.Items);
                if (sender == RegulatorValues)
                    modbus.ReadRegulatorValues(RegulatorValues.Items);
            }
        }
    }
}