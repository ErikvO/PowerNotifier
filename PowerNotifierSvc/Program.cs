using System;
using System.ServiceProcess;

namespace ErikvO.PowerNotifierSvc;

internal static class Program {
	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	private static void Main() {
		ServiceBase[] ServicesToRun;
		ServicesToRun = new ServiceBase[] {
			new PowerNotifierSvc()
		};
		ServiceBase.Run(ServicesToRun);
	}
}