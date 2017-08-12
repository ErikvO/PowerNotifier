using System;
using System.ServiceProcess;

namespace ErikvO.PowerNotifierSvc
{
	public partial class PowerNotifierSvc : ServiceBase
	{
		private Win32.ServiceControlHandlerEx serviceControlHandlerCallback;
		private IntPtr hPowerSrc;
		private IntPtr hBattCapacity;
		private Notifier _notifier;
		private PowerEventHandler _powerEventHandler;

		public PowerNotifierSvc()
		{
			InitializeComponent();
			_notifier = new Notifier(this.ServiceName);
			_powerEventHandler = new PowerEventHandler(_notifier);
		}

		protected override void OnStart(string[] args)
		{
			base.OnStart(args);
			RegisterForServiceControlHandler();
			RegisterForPowerNotifications();
			_notifier.Notify($"{this.ServiceName} started.", false);
		}

		protected override void OnStop()
		{
			UnRegisterForPowerNotifications();
			base.OnStop();
		}

		private void RegisterForServiceControlHandler()
		{
			serviceControlHandlerCallback = new Win32.ServiceControlHandlerEx(ServiceControlHandler);
			Win32.RegisterServiceCtrlHandlerEx(this.ServiceName, serviceControlHandlerCallback, IntPtr.Zero);
			if (this.ServiceHandle == IntPtr.Zero)
			{
				String error = "Error occured while registering for ServiceControlHandler";
				_notifier.Notify(error, false);
				throw new Exception(error);
			}
		}

		private void RegisterForPowerNotifications()
		{
			hPowerSrc = Win32.RegisterPowerSettingNotification(this.ServiceHandle, ref Win32.GUID_ACDC_POWER_SOURCE, Win32.DEVICE_NOTIFY_SERVICE_HANDLE);
			hBattCapacity = Win32.RegisterPowerSettingNotification(this.ServiceHandle, ref Win32.GUID_BATTERY_PERCENTAGE_REMAINING, Win32.DEVICE_NOTIFY_SERVICE_HANDLE);
		}

		private void UnRegisterForPowerNotifications()
		{
			Win32.UnregisterPowerSettingNotification(hPowerSrc);
			hPowerSrc = IntPtr.Zero;

			Win32.UnregisterPowerSettingNotification(hBattCapacity);
			hBattCapacity = IntPtr.Zero;
		}

		private int ServiceControlHandler(int control, int eventType, IntPtr eventData, IntPtr context)
		{
			switch (control)
			{
				case Win32.SERVICE_CONTROL_STOP:
					_notifier.Notify($"{this.ServiceName} stopped.", false);
					this.Stop();
					break;
				case Win32.SERVICE_CONTROL_SHUTDOWN:
					_notifier.Notify($"Server shutting down.", false);
					this.Stop();
					break;
				case Win32.SERVICE_CONTROL_POWEREVENT:
					_powerEventHandler.HandlePowerEvent(eventType, eventData);
					break;
			}

			return Win32.NO_ERROR;
		}
	}
}