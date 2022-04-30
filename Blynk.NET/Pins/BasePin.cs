using Blynk.NET.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Blynk.NET.Pins {
	public abstract class BasePin {

		public int PinNumber { get; set; }

		public async Task<bool> SendPinWriteAsync( BlynkConnection connection, BlynkCommand command, CancellationToken cancellationToken ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( SendPinWriteAsync ) );
				return await connection.SendAsync( command.ToArray(), cancellationToken );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( SendPinWriteAsync ) );
			}
		}
	}
}
