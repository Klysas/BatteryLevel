using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;


namespace UsbDevicesManager
{
	internal sealed class RazerDevice : Device
	{
		//========================================================
		//	Fields
		//========================================================

		private const int HidRequestSetReport = 0x09;
		private const int HidRequestGetReport = 0x01;
		private const int UsbTypeClass = 0x20;
		private const int UsbRecipientInterface = 0x01;
		private const int UsbDirectionOut = 0x00;
		private const int UsbDirectionIn = 0x80;
		private const int UsbTypeRequestOut = UsbTypeClass | UsbRecipientInterface | UsbDirectionOut;
		private const int UsbTypeRequestIn = UsbTypeClass | UsbRecipientInterface | UsbDirectionIn;
		private const int UsbSetupPacketValue = 0x300;
		private const int UsbSetupPacketIndex = 0x00;
		private const int UsbReportLength = 90;
		private const int StatusSuccess = 0x02;

		private readonly int _transactionId;

		//========================================================
		//	Constructors
		//========================================================

		public RazerDevice(int transactionId, Device? alternativeDevice = null)
			: base(alternativeDevice)
		{
			_transactionId = transactionId;
			ManufacturerName = "Razer";
		}

		//========================================================
		//	Methods
		//========================================================
		//--------------------------------------------------------
		//	Protected
		//--------------------------------------------------------

		protected override bool TryRetrieveBatteryLevelValue(out int batteryLevel)
		{
			using var context = new UsbContext();
			var usbDevice = context.Find(d => d.VendorId == VendorID && d.ProductId == ProductID);

			if (usbDevice == null)
			{
				batteryLevel = BatteryLevelUnknown;
				return false;
			}

			usbDevice.Open();
			var msg = GenerateMessage(_transactionId);
			SendControlMsg(usbDevice, msg, 500);
			var res = ReadResponseMsg(usbDevice);
			usbDevice.Close();
			usbDevice.Dispose();

			if (res == null)
			{
				batteryLevel = BatteryLevelUnknown;
				return false;
			}

			if (res[0] == StatusSuccess)
			{
				batteryLevel = (int)(res[9] / 255.0 * 100);
				return true;
			}

			batteryLevel = BatteryLevelUnknown;
			return false;
		}

		//--------------------------------------------------------
		//	Private Static
		//--------------------------------------------------------

		private static byte[] GenerateMessage(int transactionId)
		{
			var header = new byte[] { 0x00, (byte)transactionId, 0x00, 0x00, 0x00, 0x02, 0x07, 0x80 };

			var crc = 0;
			for (var i = 2; i < header.Length; i++)
			{
				crc ^= header[i];
			}

			var data = new byte[80];
			var crcData = new byte[] { (byte)crc, 0 };

			return header.Concat(data).Concat(crcData).ToArray();
		}

		private static byte[]? ReadResponseMsg(IUsbDevice usbDev)
		{
			var responseBuffer = new byte[UsbReportLength];
			var setupPacket = new UsbSetupPacket(UsbTypeRequestIn, HidRequestGetReport, UsbSetupPacketValue, UsbSetupPacketIndex, responseBuffer.Length);

			var ec = usbDev.ControlTransfer(setupPacket, responseBuffer, 0, responseBuffer.Length);
			if (ec == 0)
				return null;

			return UsbReportLength != responseBuffer.Length ? null : responseBuffer;
		}

		private static void SendControlMsg(IUsbDevice usbDev, byte[] data, int waitInMiliseconds)
		{
			var setupPacket = new UsbSetupPacket(UsbTypeRequestOut, HidRequestSetReport, UsbSetupPacketValue, UsbSetupPacketIndex, data.Length);

			var ec = usbDev.ControlTransfer(setupPacket, data, 0, data.Length);
			if (ec == 0)
				return;

			Thread.Sleep(waitInMiliseconds);
		}
	}
}