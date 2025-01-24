using System.Collections.Specialized;
using UsbDevicesManager;


namespace BatteryLevelTrayApp
{
	internal sealed class TrayApplicationContext : ApplicationContext
	{
		//========================================================
		//	Fields
		//========================================================

		private const int NotificationBatteryLevelThresholdChange = 5;
		private const int NotificationStartingBatteryLevelThreshold = 15;
		private const int TimeBetweenIconRefreshesInSeconds = 10;

		private readonly DevicesManager _devicesManager;
		private readonly Form _hiddenForm; // For showing ToolTip
		private readonly Thread _iconRefreshthread;
		private readonly Dictionary<string, int> _notifiedDevicesBatteryLevelThresholds = new();
		private readonly ToolTip _toolTip;
		private readonly NotifyIcon _trayIcon;

		private bool _iconRefreshIsRunning = true;

		//========================================================
		//	Constructors
		//========================================================

		public TrayApplicationContext()
		{
			_hiddenForm = new HiddenForm();
			_hiddenForm.Show(); // Necessary to make it a valid IWin32Window
			_toolTip = new ToolTip
			{
				ToolTipTitle = "Devices:"
			};

			var menu = new ContextMenuStrip();
			menu.Items.Add("Exit", null, onClick: Exit!);

			_trayIcon = new NotifyIcon()
			{
				Visible = true,
				ContextMenuStrip = menu
			};
			_trayIcon.MouseClick += TrayIcon_Click;

			_devicesManager = new DevicesManager();
			_devicesManager.Initialize();
			((INotifyCollectionChanged)_devicesManager.GetConnectedDevices()).CollectionChanged += Devices_CollectionChanged;
			_iconRefreshthread = new Thread(() =>
			{
				Thread.Sleep(1000); // Let devices get battery levels.
				while (_iconRefreshIsRunning)
				{
					try
					{
						UpdateIcon();
						CheckAndShowNotification();
						Thread.Sleep(TimeSpan.FromSeconds(TimeBetweenIconRefreshesInSeconds));
					}
					catch (ThreadInterruptedException) { }
				}
			});
			_iconRefreshthread.Start();
		}

		//========================================================
		//	Event handlers
		//========================================================

		private void Devices_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
			=> UpdateIcon();

		private void Exit(object sender, EventArgs e)
		{
			_iconRefreshIsRunning = false;
			_iconRefreshthread.Interrupt();
			_iconRefreshthread.Join();
			_devicesManager.Dispose();
			_trayIcon.Visible = false;
			Application.Exit();
		}

		private void TrayIcon_Click(object? sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
				return;

			_hiddenForm.Activate();
			_toolTip.Show(string.Join('\n', _devicesManager.GetConnectedDevices().Select(d => $"{d.DisplayName} - {GetPrettyBatteryLevel(d.BatteryLevel)}"))
						, _hiddenForm, _hiddenForm.PointToClient(Cursor.Position), 10000);
		}

		//========================================================
		//	Methods
		//========================================================

		private void CheckAndShowNotification()
		{
			var connectedDevices = _devicesManager.GetConnectedDevices();
			foreach (var connectedDevice in connectedDevices)
			{
				var deviceId = $"{connectedDevice.VendorID}{connectedDevice.ProductID}";
				if (!_notifiedDevicesBatteryLevelThresholds.ContainsKey(deviceId))
					_notifiedDevicesBatteryLevelThresholds[deviceId] = NotificationStartingBatteryLevelThreshold;
			}

			foreach (var connectedDevice in connectedDevices.OrderBy(d => d.BatteryLevel))
			{
				var deviceId = $"{connectedDevice.VendorID}{connectedDevice.ProductID}";
				if (connectedDevice.BatteryLevel == Device.BatteryLevelUnknown) continue;
				if (connectedDevice.BatteryLevel <= _notifiedDevicesBatteryLevelThresholds[deviceId])
				{
					_trayIcon.ShowBalloonTip(5000, "Low battery", $"{connectedDevice.DisplayName} is at {GetPrettyBatteryLevel(connectedDevice.BatteryLevel)}", ToolTipIcon.Warning);
					_notifiedDevicesBatteryLevelThresholds[deviceId] = (connectedDevice.BatteryLevel - 1) / NotificationBatteryLevelThresholdChange * NotificationBatteryLevelThresholdChange;
					break;
				}
			}
		}

		private void UpdateIcon()
		{
			var lowestChargeDevice = _devicesManager.GetConnectedDevices().MinBy(d => d.BatteryLevel);
			_trayIcon.Icon = lowestChargeDevice is null ? Properties.Resources.question_mark : GetIcon(lowestChargeDevice.BatteryLevel);
			_trayIcon.Text = lowestChargeDevice is null ? "No supported devices found." : $"{lowestChargeDevice.DisplayName}: {GetPrettyBatteryLevel(lowestChargeDevice.BatteryLevel)}";
		}

		private static string GetPrettyBatteryLevel(int batteryLevel)
			=> batteryLevel == Device.BatteryLevelUnknown ? "?" : $"{batteryLevel}%";

		private static Icon GetIcon(int percentage)
		{
			if (percentage >= 90)
				return Properties.Resources.battery_100;
			else if (percentage >= 65)
				return Properties.Resources.battery_75;
			else if (percentage >= 40)
				return Properties.Resources.battery_50;
			else if (percentage >= 15)
				return Properties.Resources.battery_25;
			else if (percentage >= 0)
				return Properties.Resources.battery_0;
			else
				return Properties.Resources.battery_unknown;
		}

		//========================================================
		//	Classes
		//========================================================

		private class HiddenForm : Form
		{
			public HiddenForm()
				: base()
			{
				ShowInTaskbar = false;
				FormBorderStyle = FormBorderStyle.None;
				StartPosition = FormStartPosition.Manual;
				Size = new Size(1, 1);
				Location = new Point(-2000, -2000);
			}

			protected override CreateParams CreateParams
			{
				get
				{
					var cp = base.CreateParams;
					cp.ExStyle |= 0x80; // WS_EX_TOOLWINDOW - Makes the form a tool window(doesn't show up in "Alt + Tab" list)
					return cp;
				}
			}
		}
	}
}