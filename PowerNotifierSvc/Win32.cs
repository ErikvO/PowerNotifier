using System;
using System.Runtime.InteropServices;

namespace ErikvO.PowerNotifierSvc
{
	internal class Win32
	{
		internal const int NO_ERROR = 0;
		internal const int SERVICE_CONTROL_STOP = 1;
		internal const int SERVICE_CONTROL_SHUTDOWN = 5;
		internal const int SERVICE_CONTROL_POWEREVENT = 0x0000000D;
		internal const int PBT_POWERSETTINGCHANGE = 0x8013;
		internal const int DEVICE_NOTIFY_SERVICE_HANDLE = 0x00000001;
		
		internal static Guid GUID_ACDC_POWER_SOURCE = new Guid(0x5D3E9A59, 0xE9D5, 0x4B00, 0xA6, 0xBD, 0xFF, 0x34, 0xFF, 0x51, 0x65, 0x48);
		internal static Guid GUID_BATTERY_PERCENTAGE_REMAINING = new Guid("A7AD8041-B45A-4CAE-87A3-EECBB468A9E1");

		internal delegate int ServiceControlHandlerEx(int control, int eventType, IntPtr eventData, IntPtr context);

		[DllImport("advapi32.dll", SetLastError = true)]
		internal static extern IntPtr RegisterServiceCtrlHandlerEx(string lpServiceName, ServiceControlHandlerEx cbex, IntPtr context);

		[DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
		internal static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, Int32 Flags);

		[DllImport(@"User32", EntryPoint = "UnregisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
		internal static extern bool UnregisterPowerSettingNotification(IntPtr handle);

		// This structure is sent when the PBT_POWERSETTINGSCHANGE message is sent.
		// It describes the power setting that has changed and contains data about the change
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		internal struct POWERBROADCAST_SETTING
		{
			internal Guid PowerSetting;
			internal uint DataLength;
			internal byte Data;
		}
	}
}