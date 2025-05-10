using System;
using UnityEngine;

namespace ROC.Core.Assets
{
	public class LoggingService : ILoggingService
	{
		private readonly bool _isDebugBuild;

		public LoggingService()
		{
			_isDebugBuild = Debug.isDebugBuild;
		}

		public void Log(string message)
		{
			if (_isDebugBuild)
				Debug.Log($"[ROC] {message}");
		}

		public void LogWarning(string message)
		{
			Debug.LogWarning($"[ROC] {message}");
		}

		public void LogError(string message)
		{
			Debug.LogError($"[ROC] {message}");
		}

		public void LogException(Exception exception, string context = null)
		{
			string contextMessage = string.IsNullOrEmpty(context) ? string.Empty : $" ({context})";
			Debug.LogError($"[ROC] Exception{contextMessage}: {exception.Message}\n{exception.StackTrace}");
		}
	}
}