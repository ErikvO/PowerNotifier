using System.ServiceProcess;

namespace ErikvO.PowerNotifierSvc
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
				new PowerNotifierSvc()
			};
			ServiceBase.Run(ServicesToRun);
		}
	}
}