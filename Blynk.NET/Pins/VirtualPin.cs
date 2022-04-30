using Blynk.NET.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Blynk.NET.Pins {
	public class VirtualPin : BasePin {

		private List<string> values;

		public List<string> Values {
			get {
				if( values == null )
					values = new List<string>();

				return values;
			}
		}

		public async Task<bool> SendVirtualPinWriteAsync( BlynkConnection connection, UInt16? originalMessageId, CancellationToken cancellationToken ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( SendVirtualPinWriteAsync ) );
				using( var command = new BlynkCommand( BlynkCommandType.BLYNK_CMD_HARDWARE,
					originalMessageId ?? connection.NextMessageId ) ) {

					string hardwareCommand = command.GetHardwareCommandType( HardwareCommandType.VirtualWrite );
					string pinName = this.PinNumber.ToString();

					command.Append( hardwareCommand )
						.Append( pinName );

					this.Values.ForEach( v => command.Append( v ) );

					return await base.SendPinWriteAsync( connection, command, cancellationToken );
				}
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( SendVirtualPinWriteAsync ) );
			}
		}
	}
}
