using Blynk.NET.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Blynk.NET.Pins {
	public class DigitalPin : BasePin {

		public bool Value { get; set; }

		public async Task<bool> SendDigitalPinWriteAsync( BlynkConnection connection, UInt16? originalMessageId, CancellationToken cancellationToken ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( SendDigitalPinWriteAsync ) );
				using( var command = new BlynkCommand( BlynkCommandType.BLYNK_CMD_HARDWARE,
					originalMessageId ?? connection.NextMessageId ) ) {

					string hardwareCommand = command.GetHardwareCommandType( HardwareCommandType.DigitalWrite );
					string pinName = this.PinNumber.ToString();
					char pinValue = this.Value ? '1' : '0';

					command.Append( hardwareCommand )
						.Append( pinName )
						.Append( pinValue );

					return await base.SendPinWriteAsync( connection, command, cancellationToken );
				}
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( SendDigitalPinWriteAsync ) );
			}
		}
	}
}
