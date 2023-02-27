using System;
using System.Runtime.Serialization;

namespace ErikvO.PowerNotifierSvc;

[Serializable]
public class ServiceRegistrationException : Exception {
	public ServiceRegistrationException(string message)
		: base(message) { }

	protected ServiceRegistrationException(SerializationInfo info, StreamingContext context)
		: base(info, context) { }
}