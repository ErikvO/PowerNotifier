using System;
using System.Runtime.Serialization;

namespace ErikvO.PowerNotifierSvc;

[Serializable]
public class NotificationException : Exception {
	public NotificationException(string message)
		: base(message) { }

	protected NotificationException(SerializationInfo info, StreamingContext context)
		: base(info, context) { }
}