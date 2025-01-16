namespace UsbDevicesManager
{
	public enum DeviceType
	{
		Headset,
		Keyboard,
		Mouse,
	}

	public abstract class Device
	{
		//========================================================
		//	Fields
		//========================================================

		public const int BatteryLevelUnknown = -1;

		private Timer? _batteryRefreshTimer;

		//========================================================
		//	Properties
		//========================================================

		public int BatteryLevel { get; protected set; } = BatteryLevelUnknown;
		public DeviceType DeviceType { get; init; }
		public string DisplayName { get; init; } = string.Empty;
		public string ManufacturerName { get; init; } = string.Empty;
		public int ProductID { get; init; }
		public int VendorID { get; init; }

		//========================================================
		//	Methods
		//========================================================
		//--------------------------------------------------------
		//	Public
		//--------------------------------------------------------

		public void StartBatteryLevelRefresh(int timeBetweenRefreshesInSeconds)
		{
			_batteryRefreshTimer = new Timer(_ => RefreshBatteryLevel(), null, TimeSpan.Zero, TimeSpan.FromSeconds(timeBetweenRefreshesInSeconds));
		}

		public void StopBatteryLevelRefresh()
		{
			_batteryRefreshTimer?.Dispose();
			_batteryRefreshTimer = null;
		}

		//--------------------------------------------------------
		//	Protected
		//--------------------------------------------------------

		protected abstract void RefreshBatteryLevel();
	}
}