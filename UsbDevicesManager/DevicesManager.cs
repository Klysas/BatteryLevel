using System.Collections.ObjectModel;
using LibUsbDotNet.LibUsb;


namespace UsbDevicesManager
{
	public class DevicesManager
	{
		//========================================================
		//	Fields
		//========================================================

		private Timer? _refreshDevicesListTimer;
		private readonly ObservableCollection<Device> _supportedConnectedDevices;
		private readonly UsbContext _usbContext;

		//========================================================
		//	Constructors
		//========================================================

		public DevicesManager()
		{
			_supportedConnectedDevices = new ObservableCollection<Device>();
			_usbContext = new UsbContext();
		}

		~DevicesManager()
		{
			_refreshDevicesListTimer?.Dispose();
			_supportedConnectedDevices.Clear();
			_usbContext.Dispose();
		}

		//========================================================
		//	Properties
		//========================================================

		public ReadOnlyObservableCollection<Device> GetConnectedDevices()
			=> new(_supportedConnectedDevices);

		//========================================================
		//	Methods
		//========================================================
		//--------------------------------------------------------
		//	Public
		//--------------------------------------------------------

		public void Initialize()
		{
			RefreshDevicesList();
			_refreshDevicesListTimer = new Timer(_ => RefreshDevicesList(), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60));
		}

		//--------------------------------------------------------
		//	Private
		//--------------------------------------------------------

		private void RefreshDevicesList()
		{
			var connectedDevices = _usbContext.List();

			foreach (var device in new List<Device>(_supportedConnectedDevices))
			{
				if (!connectedDevices.Any(d => d.VendorId == device.VendorID && d.ProductId == device.ProductID))
					_supportedConnectedDevices.Remove(device);
			}

			foreach (var device in connectedDevices)
			{
				var supportedDevice = SupportedDevices.All.FirstOrDefault(d => d.VendorID == device.Info.VendorId && d.ProductID == device.Info.ProductId);
				if (supportedDevice != null && !_supportedConnectedDevices.Contains(supportedDevice))
					_supportedConnectedDevices.Add(supportedDevice);
			}
		}
	}
}