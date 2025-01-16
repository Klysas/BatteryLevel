using System.Collections.Specialized;
using UsbDevicesManager;


namespace BatteryLevelTrayApp
{
	internal sealed class TrayApplicationContext : ApplicationContext
	{
		//========================================================
		//	Fields
		//========================================================

		private readonly DevicesManager _devicesManager;
		private readonly Form _hiddenForm; // For showing ToolTip
		private readonly ToolTip _toolTip;
		private readonly NotifyIcon _trayIcon;

		//========================================================
		//	Constructors
		//========================================================

		public TrayApplicationContext()
		{
			_hiddenForm = new Form
			{
				ShowInTaskbar = false,
				FormBorderStyle = FormBorderStyle.None,
				StartPosition = FormStartPosition.Manual,
				Size = new Size(1, 1),
				Location = new Point(-2000, -2000)
			};
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
			_ = new System.Threading.Timer(_ => UpdateIcon(), null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
		}

		//========================================================
		//	Event handlers
		//========================================================

		private void Devices_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
			=> UpdateIcon();

		private void Exit(object sender, EventArgs e)
		{
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
			else if (percentage >= 70)
				return Properties.Resources.battery_75;
			else if (percentage >= 45)
				return Properties.Resources.battery_50;
			else if (percentage >= 20)
				return Properties.Resources.battery_25;
			else if (percentage >= 0)
				return Properties.Resources.battery_0;
			else
				return Properties.Resources.battery_unknown;
		}
	}
}