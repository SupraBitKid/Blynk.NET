using System;
using System.Collections.Generic;
using System.Text;

namespace Blynk.NET.Logging
{
	public class ConsoleBlynkLogger : IBlynkLogger {
		[Flags]
		public enum LogLevels {
			NoLogging = 0,
			LogExceptions = 1,
			LogWarnings = 2,
			LogInformation = 4,
			LogMethods = 8,
			LogParameters = 16,
		};

		private ConsoleBlynkLogger() { }
		private LogLevels loglevels;

		public ConsoleBlynkLogger( LogLevels logLevels ) {
			this.loglevels = logLevels;
		}

		public void LogException( string message, Exception ex ) {
			if( this.loglevels.HasFlag( LogLevels.LogExceptions ) ) {
				Console.WriteLine( "Exception: {0}", message );
				Console.WriteLine( ex.ToString() );
			}
		}

		public void LogInformation( string message ) {
			if( this.loglevels.HasFlag( LogLevels.LogInformation ) ) {
				Console.WriteLine( "Information: {0}", message );
			}
		}

		public void LogMethodBegin( string name ) {
			if( this.loglevels.HasFlag( LogLevels.LogMethods ) ) {
				Console.WriteLine( "Method Begin: {0}", name );
			}
		}

		public void LogMethodEnd( string name ) {
			if( this.loglevels.HasFlag( LogLevels.LogMethods ) ) {
				Console.WriteLine( "Method End: {0}", name );
			}
		}

		public void LogParameter( string name, object value ) {
			if( this.loglevels.HasFlag( LogLevels.LogParameters ) ) {
				Console.WriteLine( "\t{0} = {1}", name, value );
			}
		}

		public void LogWarning( string message ) {
			if( this.loglevels.HasFlag( LogLevels.LogWarnings ) ) {
				Console.WriteLine( "Warning: {0}", message );
			}
		}
	}
}
