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

		private readonly Device? _alternativeDevice;
		private Timer? _batteryRefreshTimer;
		private int _skippedFailedBatteryLevelRefreshesCount = 0;

		//========================================================
		//	Constructors
		//========================================================

		public Device(Device? alternativeDevice)
		{
			_alternativeDevice = alternativeDevice;
		}

		//========================================================
		//	Properties
		//========================================================

		public int BatteryLevel { get; private set; } = BatteryLevelUnknown;
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

		public void StartBatteryLevelRefresh(int timeBetweenRefreshesInSeconds, int skipFailedBatteryLevelRefreshesCount = 3)
		{
			if (timeBetweenRefreshesInSeconds < 1) throw new ArgumentException("Value should be 1 or higher.", nameof(timeBetweenRefreshesInSeconds));
			if (skipFailedBatteryLevelRefreshesCount < 0) throw new ArgumentException("Value should be 0 or higher.", nameof(skipFailedBatteryLevelRefreshesCount));

			_batteryRefreshTimer = new Timer(_ => RefreshBatteryLevel(skipFailedBatteryLevelRefreshesCount), null, TimeSpan.Zero, TimeSpan.FromSeconds(timeBetweenRefreshesInSeconds));
		}

		public void StopBatteryLevelRefresh()
		{
			_batteryRefreshTimer?.Dispose();
			_batteryRefreshTimer = null;
		}

		//--------------------------------------------------------
		//	Protected
		//--------------------------------------------------------

		protected abstract bool TryRetrieveBatteryLevelValue(out int batteryLevel);

		//--------------------------------------------------------
		//	Private
		//--------------------------------------------------------

		private void RefreshBatteryLevel(int skipFailedBatteryLevelRefreshesCount)
		{
			try
			{
				if (TryRetrieveBatteryLevelValue(out int batteryLevel))
				{
					BatteryLevel = batteryLevel;
					_skippedFailedBatteryLevelRefreshesCount = 0;
					return;
				}
			}
			catch { }

			if (_alternativeDevice?.BatteryLevel != BatteryLevelUnknown)
			{
				BatteryLevel = _alternativeDevice!.BatteryLevel;
				return;
			}

			if (skipFailedBatteryLevelRefreshesCount > _skippedFailedBatteryLevelRefreshesCount)
			{
				_skippedFailedBatteryLevelRefreshesCount++;
				return;
			}

			_skippedFailedBatteryLevelRefreshesCount = 0;
			BatteryLevel = BatteryLevelUnknown;
		}
	}
}