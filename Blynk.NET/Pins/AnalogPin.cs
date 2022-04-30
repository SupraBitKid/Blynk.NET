using Blynk.NET.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Blynk.NET.Pins {
	public class AnalogPin : BasePin {

		public Int16 Value { get; set; }

		public async Task<bool> SendAnalogPinWriteAsync( BlynkConnection connection, UInt16? originalMessageId, CancellationToken cancelationToken ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( SendAnalogPinWriteAsync ) );
				using( var command = new BlynkCommand( BlynkCommandType.BLYNK_CMD_HARDWARE,
					originalMessageId ?? connection.NextMessageId ) ) {

					string hardwareCommand = command.GetHardwareCommandType( HardwareCommandType.AnalogWrite );
					string pinName = this.PinNumber.ToString();
					string pinValue = this.Value.ToString();

					command.Append( hardwareCommand )
						.Append( pinName )
						.Append( pinValue );

					return await base.SendPinWriteAsync( connection, command, cancelationToken );
				}
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( SendAnalogPinWriteAsync ) );
			}
		}
	}
}
