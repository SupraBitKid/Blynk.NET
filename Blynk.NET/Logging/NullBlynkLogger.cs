using System;
using System.Collections.Generic;
using System.Text;

namespace Blynk.NET.Logging {
	public class NullBlynkLogger : IBlynkLogger {
		public void LogException( string message, Exception ex ) { }
		public void LogInformation( string message ) { }
		public void LogMethodBegin( string name ) { }
		public void LogMethodEnd( string name ) { }
		public void LogParameter( string name, object value ) { }
		public void LogWarning( string message ) { }
	}
}
