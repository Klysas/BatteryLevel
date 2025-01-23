using HidSharp;


namespace UsbDevicesManager
{
	internal sealed class AsusDevice : Device
	{
		//========================================================
		//	Fields
		//========================================================

		private const int MessageLength = 0x40;

		private readonly int _usagePage;
		private readonly int _usage;

		//========================================================
		//	Constructors
		//========================================================

		public AsusDevice(int usagePage, int usage, Device? alternativeDevice = null)
			: base(alternativeDevice)
		{
			_usagePage = usagePage;
			_usage = usage;
			ManufacturerName = "Asus";
		}

		//========================================================
		//	Methods
		//========================================================
		//--------------------------------------------------------
		//	Protected
		//--------------------------------------------------------

		protected override bool TryRetrieveBatteryLevelValue(out int batteryLevel)
		{
			var hidDevice = DeviceList.Local.GetHidDevices(VendorID, ProductID)
							.FirstOrDefault(d => d.GetReportDescriptor().DeviceItems
								.Any(item => item.Usages.GetAllValues()
									.Any(index =>
									{
										uint usagePage = (index & 0xFFFF0000) >> 16;
										uint usage = index & 0x0000FFFF;
										return usagePage == _usagePage && usage == _usage;
									})
								)
							);

			if (hidDevice == null)
			{
				batteryLevel = BatteryLevelUnknown;
				return false;
			}

			using var hidStream = hidDevice.Open();

			if (hidStream == null)
			{
				batteryLevel = BatteryLevelUnknown;
				return false;
			}

			hidStream.ReadTimeout = 250;
			hidStream.Write(GenerateBatteryLevelRequestMessage());

			/**
			 * If device is online, then first 3 bytes are the same.
			 * If device is sleeping, then first 3 bytes returned are [0x02, 0xFF, 0xAA].
			 * Indexes 6 and 11 seem to hold battery level.
			 **/
			var result = new byte[MessageLength];
			int bytesRead = hidStream.Read(result);

			if (bytesRead != MessageLength)
			{
				batteryLevel = BatteryLevelUnknown;
				return false;
			}

			if (result[1] == 0x12)
			{
				batteryLevel = result[6];
				return true;
			}

			batteryLevel = BatteryLevelUnknown;
			return false;
		}

		//--------------------------------------------------------
		//	Private Static
		//--------------------------------------------------------

		private static byte[] GenerateBatteryLevelRequestMessage()
		{
			var msg = new byte[MessageLength];
			msg[0] = 0x02; // Report ID
			msg[1] = 0x12; // Command type
			msg[2] = 0x01; // Command
			return msg;
		}
	}
}