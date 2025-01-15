namespace UsbDevicesManager
{
	internal static class SupportedDevices
	{
		public static readonly List<Device> All = new()
		{
			new RazerDevice(0x1f) {VendorID = 0x1532, ProductID = 0x00B6, DisplayName = "Razer Deathadder V3 Pro Wired", DeviceType = DeviceType.Mouse},
			new RazerDevice(0x1f) {VendorID = 0x1532, ProductID = 0x00B7, DisplayName = "Razer Deathadder V3 Pro Wireless", DeviceType = DeviceType.Mouse},
		};
	}
}