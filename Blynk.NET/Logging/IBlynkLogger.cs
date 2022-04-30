using System;
using System.Collections.Generic;
using System.Text;

namespace Blynk.NET.Logging {
	public interface IBlynkLogger {
		void LogMethodBegin( string name );
		void LogMethodEnd( string name );
		void LogParameter( string name, object value );
		void LogInformation( string message );
		void LogWarning( string message );
		void LogException( string message, Exception ex );
	}
}
