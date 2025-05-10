using System;

namespace ROC.Core.Assets
{
	public interface ILoggingService
	{
		void Log(string message);
		void LogWarning(string message);
		void LogError(string message);
		void LogException(Exception exception, string context = null);
	}
}