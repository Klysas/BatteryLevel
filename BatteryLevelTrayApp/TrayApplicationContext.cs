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
		private readonly NotifyIcon _trayIcon;

		//========================================================
		//	Constructors
		//========================================================

		public TrayApplicationContext()
		{
			var menu = new ContextMenuStrip();
			menu.Items.Add("Exit", null, onClick: Exit!);

			_trayIcon = new NotifyIcon()
			{
				Visible = true,
				ContextMenuStrip = menu
			};

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

		//========================================================
		//	Methods
		//========================================================

		private void UpdateIcon()
		{
			var lowestChargeDevice = _devicesManager.GetConnectedDevices().MinBy(d => d.BatteryLevel);
			_trayIcon.Icon = lowestChargeDevice is null ? Properties.Resources.question_mark : GetIcon(lowestChargeDevice.BatteryLevel);
			_trayIcon.Text = lowestChargeDevice is null ? "No supported devices found." : $"{lowestChargeDevice.DisplayName}: {lowestChargeDevice.BatteryLevel}%";
		}

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