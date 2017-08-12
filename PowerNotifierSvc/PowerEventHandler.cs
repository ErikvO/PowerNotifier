using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ErikvO.PowerNotifierSvc
{
	internal class PowerEventHandler
	{
		private Notifier _notifier;
		private bool? _onBattery = null;
		private int? _lastPct = null;

		internal PowerEventHandler(Notifier notifier)
		{
			_notifier = notifier;
		}

		internal void HandlePowerEvent(int eventType, IntPtr eventData)
		{
			if (eventType == Win32.PBT_POWERSETTINGCHANGE)
			{
				Win32.POWERBROADCAST_SETTING ps = (Win32.POWERBROADCAST_SETTING)Marshal.PtrToStructure(eventData, typeof(Win32.POWERBROADCAST_SETTING));
				if (ps.PowerSetting == Win32.GUID_ACDC_POWER_SOURCE)
				{
					_onBattery = ps.Data != 0;

					if (_onBattery.Value)
						_notifier.Notify($"On battery, remaining: {Math.Ceiling(SystemInformation.PowerStatus.BatteryLifePercent * 100)}%");
					else
					{
						_notifier.Notify("Power restored");
						_lastPct = null;
					}
				}
				else if (ps.PowerSetting == Win32.GUID_BATTERY_PERCENTAGE_REMAINING)
				{
					if (_onBattery.HasValue && _onBattery.Value &&     //On battery
						ps.Data % 10 == 0 &&                            //Per 10% only
						_lastPct.HasValue && ps.Data < _lastPct.Value)  //When current value is lower than last value 			
					{
						_notifier.Notify($"Battery remaining: {ps.Data}%");
					}
					_lastPct = ps.Data;
				}
				else
					_notifier.Notify($"Unknown PowerSetting: {ps.PowerSetting}, data: {ps.Data}");
			}
			else
				_notifier.Notify($"Unknown eventType: {eventType}");
		}
	}
}