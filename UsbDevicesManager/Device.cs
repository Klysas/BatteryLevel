namespace UsbDevicesManager
{
	public enum DeviceType
	{
		Headset,
		Keyboard,
		Mouse,
	}

	public class Device
	{
		//========================================================
		//	Properties
		//========================================================

		public int BatteryLevel { get; private set; } = -1;
		public DeviceType DeviceType { get; init; }
		public string DisplayName { get; init; } = string.Empty;
		public string ManufacturerName { get; init; } = string.Empty;
		public int ProductID { get; init; }
		public int VendorID { get; init; }
	}
}