namespace UsbDevicesManager
{
	internal static class SupportedDevices
	{
		public static readonly List<Device> All;

		static SupportedDevices()
		{
			var wiredAsus = new AsusDevice(0xFF00, 0x0001) { VendorID = 0x0B05, ProductID = 0x1AAE, DisplayName = "Asus ROG Strix Scope II 96 Wired", DeviceType = DeviceType.Keyboard };
			var wirelessAsus = new AsusDevice(0xFF00, 0x0001, wiredAsus) { VendorID = 0x0B05, ProductID = 0x1ACE, DisplayName = "Asus ROG Strix Scope II 96 Wireless", DeviceType = DeviceType.Keyboard };

			var wiredRazer = new RazerDevice(0x1f) { VendorID = 0x1532, ProductID = 0x00B6, DisplayName = "Razer Deathadder V3 Pro Wired", DeviceType = DeviceType.Mouse };
			var wirelessRazer = new RazerDevice(0x1f, wiredRazer) { VendorID = 0x1532, ProductID = 0x00B7, DisplayName = "Razer Deathadder V3 Pro Wireless", DeviceType = DeviceType.Mouse };

			All = new() {
				wiredAsus,
				wirelessAsus,
				wiredRazer,
				wirelessRazer
			};
		}
	}
}