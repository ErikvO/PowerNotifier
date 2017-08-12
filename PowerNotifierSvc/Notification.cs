using System;

namespace ErikvO.PowerNotifierSvc
{
	internal class Notification
	{
		internal string UnixTimestamp { get; private set; }
		internal string Message { get; private set; }

		internal Notification(string message)
		{
			Message = message;
			UnixTimestamp = ((int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
		}
	}
}