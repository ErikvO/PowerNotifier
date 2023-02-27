using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;

namespace ErikvO.PowerNotifierSvc;

internal class Notifier {
	private const string PUSHOVER_URL = "https://api.pushover.net/1/messages.json";
	private const int BASETIMERINTERVAL = 15;
	private int _retryCount = 0;
	private static readonly HttpClient _client = new();
	private readonly ConcurrentQueue<Notification> _notificationQueue = new();
	private readonly Timer _timer;
	private readonly string _serviceName;

	internal Notifier(string serviceName) {
		_serviceName = serviceName;
		_timer = new Timer(BASETIMERINTERVAL * 1000) {
			AutoReset = false,
			Enabled = false
		};
		_timer.Elapsed += (object sender, ElapsedEventArgs e) => new Task(SendNotifications).Start();
	}

	internal void Notify(string message, bool writeEventLog = true) {
		if (writeEventLog)
			WriteEventLog(message);
		QueueNotification(new Notification(message));
	}

	private void WriteEventLog(string message) {
		if (!EventLog.SourceExists(_serviceName))
			EventLog.CreateEventSource(_serviceName, "Application");

		EventLog.WriteEntry(_serviceName, message);
	}

	private void QueueNotification(Notification notification) {
		_notificationQueue.Enqueue(notification);

		while (_notificationQueue.Count > 15)
			_ = _notificationQueue.TryDequeue(out var _);

		if (_timer.Enabled) {
			_timer.Stop();
			_retryCount = 0;
			WriteEventLog($"New notification received for sending, resetting retry timer and count.");
		}

		new Task(SendNotifications).Start();
	}

	private void SendNotifications() {
		try {
			while (_notificationQueue.Count > 0) {
				_ = _notificationQueue.TryDequeue(out var notification);
				SendNotification(notification);
			}

			_retryCount = 0;
		}
		catch {
			var newIntervalInSeconds = Math.Pow(2, _retryCount) * BASETIMERINTERVAL;
			var newIntervalInMinutes = newIntervalInSeconds / 60;
			WriteEventLog($@"Unable to send notifications. Retrying after {(
				newIntervalInMinutes < 1
				? newIntervalInSeconds
				: (int)newIntervalInMinutes
			)} {(
			newIntervalInMinutes < 1
				? "seconds"
				: "minutes"
			)}.");
			_timer.Interval = newIntervalInSeconds * 1000;
			_timer.Enabled = true;
			_retryCount++;
			_timer.Start();
		}
	}

	private void SendNotification(Notification notification) {
		var values = new Dictionary<string, string> {
			{ "token" , "" },
			{ "user" , "" },
			{ "timestamp" , notification.UnixTimestamp },
			{ "title" , "Power Notifier" },
			{ "message" , notification.Message }
		};

		var content = new FormUrlEncodedContent(values);
		var response = _client.PostAsync(PUSHOVER_URL, content).Result;
		if (!response.IsSuccessStatusCode) {
			var responseString = response.Content.ReadAsStringAsync().Result;
			WriteEventLog(responseString);
			throw new NotificationException(responseString);
		}
	}
}