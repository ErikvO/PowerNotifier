using System;
using System.ServiceProcess;

namespace ErikvO.PowerNotifierSvc;

public partial class PowerNotifierSvc : ServiceBase {
	private Win32.ServiceControlHandlerEx _serviceControlHandlerCallback;
	private IntPtr _hPowerSrc;
	private IntPtr _hBattCapacity;
	private readonly Notifier _notifier;
	private readonly PowerEventHandler _powerEventHandler;

	public PowerNotifierSvc() {
		InitializeComponent();
		_notifier = new Notifier(ServiceName);
		_powerEventHandler = new PowerEventHandler(_notifier);
	}

	protected override void OnStart(string[] args) {
		base.OnStart(args);
		RegisterForServiceControlHandler();
		RegisterForPowerNotifications();
		_notifier.Notify($"{ServiceName} started.", false);
	}

	protected override void OnStop() {
		UnRegisterForPowerNotifications();
		base.OnStop();
	}

	private void RegisterForServiceControlHandler() {
		_serviceControlHandlerCallback = new Win32.ServiceControlHandlerEx(ServiceControlHandler);
		_ = Win32.RegisterServiceCtrlHandlerEx(ServiceName, _serviceControlHandlerCallback, IntPtr.Zero);
		if (ServiceHandle == IntPtr.Zero) {
			var error = "Error occured while registering for ServiceControlHandler";
			_notifier.Notify(error, false);
			throw new ServiceRegistrationException(error);
		}
	}

	private void RegisterForPowerNotifications() {
		_hPowerSrc = Win32.RegisterPowerSettingNotification(
			ServiceHandle,
			ref Win32.GUID_ACDC_POWER_SOURCE,
			Win32.DEVICE_NOTIFY_SERVICE_HANDLE
		);

		_hBattCapacity = Win32.RegisterPowerSettingNotification(
			ServiceHandle,
			ref Win32.GUID_BATTERY_PERCENTAGE_REMAINING,
			Win32.DEVICE_NOTIFY_SERVICE_HANDLE
		);
	}

	private void UnRegisterForPowerNotifications() {
		_ = Win32.UnregisterPowerSettingNotification(_hPowerSrc);
		_hPowerSrc = IntPtr.Zero;

		_ = Win32.UnregisterPowerSettingNotification(_hBattCapacity);
		_hBattCapacity = IntPtr.Zero;
	}

	private int ServiceControlHandler(
		int control,
		int eventType,
		IntPtr eventData,
		IntPtr context
	) {
		switch (control) {
			case Win32.SERVICE_CONTROL_STOP:
				_notifier.Notify($"{ServiceName} stopped.", false);
				Stop();
				break;
			case Win32.SERVICE_CONTROL_SHUTDOWN:
				_notifier.Notify($"Server shutting down.", false);
				Stop();
				break;
			case Win32.SERVICE_CONTROL_POWEREVENT:
				_powerEventHandler.HandlePowerEvent(eventType, eventData);
				break;
			default:
				break;
		}

		return Win32.NO_ERROR;
	}
}