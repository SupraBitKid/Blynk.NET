using Blynk.NET.Interface;
using Blynk.NET.Logging;
using Blynk.NET.Pins;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blynk.NET {
	public class BlynkMessageParser {
		public const string VirtualWriteIndicator = "vw";
		public const string VirtualReadIndicator = "vr";
		public const string DigitalWriteIndicator = "dw";
		public const string DigitalReadIndicator = "dr";
		public const string AnalogWriteIndicator = "aw";
		public const string AnalogReadIndicator = "ar";
		public const string PinModeIndicator = "pm";

		public static IEnumerable<BlynkMessageParser> GetBlynkMessages( byte[] incomingBuffer, int readLength ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( GetBlynkMessages ) );
				var byteProtocolBuffer = new ByteProtocolBuffer( incomingBuffer );
				var result = new List<BlynkMessageParser>();

				while( byteProtocolBuffer.Position < readLength ) {
					result.Add( new BlynkMessageParser( byteProtocolBuffer ) );
				}

				return result;
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( GetBlynkMessages ) );
			}
		}

		private BlynkMessageParser() {}

		private IByteProtocolBuffer messageBuffer;

		protected BlynkMessageParser( IByteProtocolBuffer byteProtocol ) {
			try {
				BlynkLogManager.LogMethodBegin( "BlynkMessageParser.ctor" );
				byte commandType;
				UInt16 messageId;
				UInt16 messageLength;

				byteProtocol
					.Extract( out commandType )
					.Extract( out messageId )
					.Extract( out messageLength );

				this.BlynkCommandType = ( BlynkCommandType )commandType;
				this.MessageId = messageId;

				if( this.BlynkCommandType == BlynkCommandType.BLYNK_CMD_RESPONSE ) {
					this.ResponseCode = messageLength;
					this.MessageLength = ( UInt16 )0;
					this.messageBuffer = null;
				}
				else { 
					this.MessageLength = messageLength;
					byteProtocol.Extract( out this.messageBuffer, messageLength );
				}
			}
			catch( Exception ex ) {
				BlynkLogManager.LogException( "Error constructing message parser", ex );
			}
			finally {
				BlynkLogManager.LogMethodEnd( "BlynkMessageParser.ctor" );
			}
		}

		internal async Task<bool> ParseMessageAsync( BlynkConnection blynkConnection ) {
			bool result = true;
			try {
				BlynkLogManager.LogMethodBegin( nameof( ParseMessageAsync ) );
				BlynkLogManager.LogInformation( string.Format( "Message Received command type : {0}", this.BlynkCommandType ) );

				switch( this.BlynkCommandType ) {

					case BlynkCommandType.BLYNK_CMD_RESPONSE:
						blynkConnection.ResponseReceivedNotification?.Invoke( this.ResponseCode );
						return result;

					case BlynkCommandType.BLYNK_CMD_PING:
						return await blynkConnection.SendResponseAsync( this.MessageId );

					case BlynkCommandType.BLYNK_CMD_BRIDGE:
						return await blynkConnection.SendResponseAsync( this.MessageId );

					case BlynkCommandType.BLYNK_CMD_HARDWARE: {
						var hardwareCommandType = this.GetHardwareCommandType();
						BlynkLogManager.LogInformation( string.Format( "Hardware command type : {0}", hardwareCommandType ) );

						switch( hardwareCommandType ) {
							case HardwareCommandType.VirtualRead: {
								string pinString;

								this.messageBuffer.Extract( out pinString );

								var pinNumber = int.Parse( pinString );

								var pin = blynkConnection.VirtualPinNotification.PinReadRequest?.Invoke( pinNumber );// blynkConnection.ReadVirtualPinRequest?.Invoke( pinNumber );

								if( pin == null )
									return await blynkConnection.SendResponseAsync( this.MessageId, BlynkResponse.NO_DATA );
								else
									return await pin.SendVirtualPinWriteAsync( blynkConnection, this.MessageId, blynkConnection.CancellationToken );
							}

							case HardwareCommandType.VirtualWrite: {
								string pinNumberAsString;

								this.messageBuffer.Extract( out pinNumberAsString );

								var pin = new VirtualPin() {
									PinNumber = int.Parse( pinNumberAsString )
								};

								this.messageBuffer.Extract( pin.Values );

								blynkConnection.VirtualPinNotification.PinWriteNotification?.Invoke( pin );

								return await blynkConnection.SendResponseAsync( this.MessageId );
							}
							
							case HardwareCommandType.DigitalRead: {
								string pinString;

								this.messageBuffer.Extract( out pinString );

								var pinNumber = int.Parse( pinString );

								var pin = blynkConnection.DigitalPinNotification.PinReadRequest?.Invoke( pinNumber );// blynkConnection.ReadDigitalPinRequest?.Invoke( pinNumber );

								if( pin == null )
									return await blynkConnection.SendResponseAsync( this.MessageId, BlynkResponse.NO_DATA );
								else
									return await pin.SendDigitalPinWriteAsync( blynkConnection, this.MessageId, blynkConnection.CancellationToken );
							}

							case HardwareCommandType.DigitalWrite: {
								string pinNumberAsString;
								string valueAsString;

								this.messageBuffer.Extract( out pinNumberAsString )
									.Extract( out valueAsString );

								var pin = new DigitalPin() {
									PinNumber = int.Parse( pinNumberAsString ),
									Value = int.Parse( valueAsString ) == 1
								};

								//blynkConnection.WriteDigitalPinNotification?.Invoke( pin );
								blynkConnection.DigitalPinNotification.PinWriteNotification?.Invoke( pin );

								return await blynkConnection.SendResponseAsync( this.MessageId );
							}

							case HardwareCommandType.AnalogRead: {
								string pinString;

								this.messageBuffer.Extract( out pinString );

								var pinNumber = int.Parse( pinString );

								var pin = blynkConnection.AnalogPinNotification.PinReadRequest?.Invoke( pinNumber );// blynkConnection.ReadAnalogPinRequest( pinNumber );

								if( pin == null )
									return await blynkConnection.SendResponseAsync( this.MessageId, BlynkResponse.NO_DATA );
								else
									return await pin.SendAnalogPinWriteAsync( blynkConnection, this.MessageId, blynkConnection.CancellationToken );
							}

							case HardwareCommandType.AnalogWrite: {
								string pinNumberAsString;
								string valueAsString;

								this.messageBuffer.Extract( out pinNumberAsString )
									.Extract( out valueAsString );

								var pin = new AnalogPin() {
									PinNumber = int.Parse( pinNumberAsString ),
									Value = short.Parse( valueAsString )
								};

								//blynkConnection.WriteAnalogPinNotification?.Invoke( pin );
								blynkConnection.AnalogPinNotification.PinWriteNotification?.Invoke( pin );

								return await blynkConnection.SendResponseAsync( this.MessageId );
							}

							case HardwareCommandType.PinMode: {
								string pin;
								string mode;
								while( this.messageBuffer.Position < this.MessageLength ) {

									this.messageBuffer.Extract( out pin )
										.Extract( out mode );

									PinMode pinMode = PinMode.Invalid;

									switch( mode ) {
										case "in":
											pinMode = PinMode.Input;
											break;
										case "out":
											pinMode = PinMode.Output;
											break;
										case "pu":
											pinMode = PinMode.PullUp;
											break;
										case "pd":
											pinMode = PinMode.PullDown;
											break;
										case "pwm":
											pinMode = PinMode.Pwm;
											break;
									}

									if( pinMode != PinMode.Invalid )
										blynkConnection.PinModeNotification?.Invoke( pin, pinMode );
								}
								return await blynkConnection.SendResponseAsync( this.MessageId );
							}

						}
						break;
					}
				}
			}
			catch( Exception ex ) {
				BlynkLogManager.LogException( "Error parsing message", ex );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( ParseMessageAsync ) );
			}
			return result;
		}

		public BlynkCommandType BlynkCommandType { get; private set; }
		public UInt16 MessageLength { get; private set; }
		public UInt16 MessageId { get; private set; }
		public UInt16 ResponseCode { get; private set; }

		public HardwareCommandType GetHardwareCommandType() {
			try {
				BlynkLogManager.LogMethodBegin( nameof( GetHardwareCommandType ) );
				string stringData;
				this.messageBuffer.Extract( out stringData );
				switch( stringData ) {
					case VirtualReadIndicator:
						return HardwareCommandType.VirtualRead;
					case VirtualWriteIndicator:
						return HardwareCommandType.VirtualWrite;
					case DigitalReadIndicator:
						return HardwareCommandType.DigitalRead;
					case DigitalWriteIndicator:
						return HardwareCommandType.DigitalWrite;
					case AnalogReadIndicator:
						return HardwareCommandType.AnalogRead;
					case AnalogWriteIndicator:
						return HardwareCommandType.AnalogWrite;
					case PinModeIndicator:
						return HardwareCommandType.PinMode;
					default:
						return HardwareCommandType.Invalid;
				}
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( GetHardwareCommandType ) );
			}
		}
	}
}
