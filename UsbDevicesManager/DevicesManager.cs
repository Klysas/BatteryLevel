﻿using System.Collections.ObjectModel;
using LibUsbDotNet.LibUsb;


namespace UsbDevicesManager
{
	public class DevicesManager : IDisposable
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

		//========================================================
		//	Properties
		//========================================================

		public ReadOnlyObservableCollection<Device> GetConnectedDevices()
			=> new(_supportedConnectedDevices);

		public int TimeBetweenBatteryLevelRefreshesInSeconds { get; set; } = 120;

		//========================================================
		//	Methods
		//========================================================
		//--------------------------------------------------------
		//	Public
		//--------------------------------------------------------

		public void Dispose()
		{
			_refreshDevicesListTimer?.Dispose();
			_supportedConnectedDevices.ToList().ForEach(d => d.StopBatteryLevelRefresh());
			_supportedConnectedDevices.Clear();
			_usbContext.Dispose();
		}

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
				{
					device.StopBatteryLevelRefresh();
					_supportedConnectedDevices.Remove(device);
				}
			}

			foreach (var device in connectedDevices)
			{
				var supportedDevice = SupportedDevices.All.FirstOrDefault(d => d.VendorID == device.Info.VendorId && d.ProductID == device.Info.ProductId);
				if (supportedDevice != null && !_supportedConnectedDevices.Contains(supportedDevice))
				{
					_supportedConnectedDevices.Add(supportedDevice);
					supportedDevice.StartBatteryLevelRefresh(TimeBetweenBatteryLevelRefreshesInSeconds);
				}
			}
		}
	}
}