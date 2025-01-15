namespace UsbDevicesManager
{
	internal sealed class RazerDevice : Device
	{
		private readonly int _transactionId;

		public RazerDevice(int transactionId)
		{
			_transactionId = transactionId;
			ManufacturerName = "Razer";
		}
	}
}