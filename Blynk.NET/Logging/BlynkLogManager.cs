using System;
using System.Collections.Generic;
using System.Text;

namespace Blynk.NET.Logging
{
    public static class BlynkLogManager {
		static BlynkLogManager() {
			BlynkLogManager.BlynkLogger = new NullBlynkLogger();
		}

		public static IBlynkLogger BlynkLogger { get; set; }

		internal static void LogMethodBegin( string name ) {
			BlynkLogManager.BlynkLogger.LogMethodBegin( name );
		}

		internal static void LogMethodEnd( string name ) {
			BlynkLogManager.BlynkLogger.LogMethodEnd( name );
		}

		internal static void LogParameter( string name, object value ) {
			BlynkLogManager.BlynkLogger.LogParameter( name, value );
		}

		internal static void LogInformation( string message ) {
			BlynkLogManager.BlynkLogger.LogInformation( message );
		}

		internal static void LogWarning( string message ) {
			BlynkLogManager.BlynkLogger.LogWarning( message );
		}

		internal static void LogException( string message, Exception ex ) {
			BlynkLogManager.BlynkLogger.LogException( message, ex );
		}
	}
}
