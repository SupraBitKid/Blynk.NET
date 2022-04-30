using System;
using System.Collections.Generic;
using Blynk.NET.Interface;
using Blynk.NET.Logging;

namespace Blynk.NET {
	public enum BlynkCommandType {
		BLYNK_CMD_RESPONSE = 0,
		BLYNK_CMD_REGISTER = 1,
		BLYNK_CMD_LOGIN = 2,
		BLYNK_CMD_SAVE_PROF = 3,
		BLYNK_CMD_LOAD_PROF = 4,
		BLYNK_CMD_GET_TOKEN = 5,
		BLYNK_CMD_PING = 6,
		BLYNK_CMD_ACTIVATE = 7,
		BLYNK_CMD_DEACTIVATE = 8,
		BLYNK_CMD_REFRESH = 9,
		BLYNK_CMD_GET_GRAPH_DATA = 10,
		BLYNK_CMD_GET_GRAPH_DATA_RESPONSE = 11,

		// notification
		BLYNK_CMD_TWEET = 12,
		BLYNK_CMD_EMAIL = 13,
		BLYNK_CMD_NOTIFY = 14,
		BLYNK_CMD_BRIDGE = 15,
		BLYNK_CMD_HARDWARE_SYNC = 16,
		BLYNK_CMD_INTERNAL = 17,
		BLYNK_CMD_SMS = 18,
		BLYNK_CMD_PROPERTY = 19,
		BLYNK_CMD_HARDWARE = 20,

		// dashboard
		BLYNK_CMD_CREATE_DASH = 21,
		BLYNK_CMD_SAVE_DASH = 22,
		BLYNK_CMD_DELETE_DASH = 23,

		BLYNK_CMD_LOAD_PROF_GZ = 24,
		BLYNK_CMD_SYNC = 25,
		BLYNK_CMD_SHARING = 26,
		BLYNK_CMD_ADD_PUSH_TOKEN = 27,

		//sharing commands
		BLYNK_CMD_GET_SHARED_DASH = 29,
		BLYNK_CMD_GET_SHARE_TOKEN = 30,
		BLYNK_CMD_REFRESH_SHARE_TOKEN = 31,
		BLYNK_CMD_SHARE_LOGIN = 32,

		BLYNK_CMD_REDIRECT = 41,
		BLYNK_CMD_DEBUG_PRINT = 55,
		BLYNK_CMD_EVENT_LOG = 64
	}

	public enum BlynkStatus {
		BLYNK_SUCCESS = 200,
		BLYNK_QUOTA_LIMIT_EXCEPTION = 1,
		BLYNK_ILLEGAL_COMMAND = 2,
		BLYNK_NOT_REGISTERED = 3,
		BLYNK_ALREADY_REGISTERED = 4,
		BLYNK_NOT_AUTHENTICATED = 5,
		BLYNK_NOT_ALLOWED = 6,
		BLYNK_DEVICE_NOT_IN_NETWORK = 7,
		BLYNK_NO_ACTIVE_DASHBOARD = 8,
		BLYNK_INVALID_TOKEN = 9,
		BLYNK_ILLEGAL_COMMAND_BODY = 11,
		BLYNK_GET_GRAPH_DATA_EXCEPTION = 12,
		BLYNK_NO_DATA_EXCEPTION = 17,
		BLYNK_DEVICE_WENT_OFFLINE = 18,
		BLYNK_SERVER_EXCEPTION = 19,
		BLYNK_NTF_INVALID_BODY = 13,
		BLYNK_NTF_NOT_AUTHORIZED = 14,
		BLYNK_NTF_ECXEPTION = 15,
		BLYNK_TIMEOUT = 16,
		BLYNK_NOT_SUPPORTED_VERSION = 20,
		BLYNK_ENERGY_LIMIT = 21
	}

	public enum BlynkResponse {
		OK = 200,
		QUOTA_LIMIT = 1,
		ILLEGAL_COMMAND = 2,
		USER_NOT_REGISTERED = 3,
		USER_ALREADY_REGISTERED = 4,
		USER_NOT_AUTHENTICATED = 5,
		NOT_ALLOWED = 6,
		DEVICE_NOT_IN_NETWORK = 7,
		NO_ACTIVE_DASHBOARD = 8,
		INVALID_TOKEN = 9,
		ILLEGAL_COMMAND_BODY = 11,
		GET_GRAPH_DATA = 12,
		NOTIFICATION_INVALID_BODY = 13,
		NOTIFICATION_NOT_AUTHORIZED = 14,
		NOTIFICATION_ERROR = 15,
		BLYNK_TIMEOUT = 16,
		NO_DATA = 17,
		DEVICE_WENT_OFFLINE = 18,
		SERVER_ERROR = 19,
		NOT_SUPPORTED_VERSION = 20,
		ENERGY_LIMIT = 21,
		FACEBOOK_USER_LOGIN_WITH_PASS = 22
	}

	public enum HardwareCommandType {
		Invalid = 0,
		VirtualWrite,
		VirtualRead,
		DigitalWrite,
		DigitalRead,
		AnalogWrite,
		AnalogRead,
		PinMode,
	}

	public enum PinMode {
		Invalid = 0,
		Input,
		Output,
		PullUp,
		PullDown,
		Pwm
	}

	public class BlynkCommand : IDisposable, IByteProtocolBuffer {
		internal BlynkCommand( BlynkCommandType commandType, UInt16 messageId )
			: this( commandType, messageId, true ) { }

		internal BlynkCommand( BlynkCommandType commandType, UInt16 messageId, bool appendLength ) {
			this.MessageBuffer = new ByteProtocolBuffer();
			this.CommandType = commandType;
			this.appendLength = appendLength;

			this.MessageBuffer.Append( ( byte )commandType );
			this.MessageBuffer.Append( messageId );
			if( this.appendLength )
				this.MessageBuffer.Append( ( UInt16 )0 ); // length minus header computed later

		}

		private bool appendLength = false;
		private ByteProtocolBuffer MessageBuffer { get; set; }

		public BlynkCommandType CommandType { get; protected set; }

		public ushort Length => this.MessageBuffer.Length;

		public ushort Position => this.MessageBuffer.Position;

		public string GetHardwareCommandType( HardwareCommandType value ) {
			BlynkLogManager.LogMethodBegin( nameof( GetHardwareCommandType ) );

			switch( value ) {
				case HardwareCommandType.VirtualRead:
					return BlynkMessageParser.VirtualReadIndicator;
				case HardwareCommandType.VirtualWrite:
					return BlynkMessageParser.VirtualWriteIndicator;
				case HardwareCommandType.DigitalRead:
					return BlynkMessageParser.DigitalReadIndicator;
				case HardwareCommandType.DigitalWrite:
					return BlynkMessageParser.DigitalWriteIndicator;
				case HardwareCommandType.AnalogRead:
					return BlynkMessageParser.AnalogReadIndicator;
				case HardwareCommandType.AnalogWrite:
					return BlynkMessageParser.AnalogWriteIndicator;
				case HardwareCommandType.PinMode:
					return BlynkMessageParser.PinModeIndicator;
			}

			BlynkLogManager.LogMethodEnd( nameof( GetHardwareCommandType ) );

			return string.Empty;
		}

		public IByteProtocolBuffer Append( byte data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Append ) );
				return this.MessageBuffer.Append( data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Append ) );
			}
		}

		public IByteProtocolBuffer Append( byte[] data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Append ) );
				return this.MessageBuffer.Append( data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Append ) );

			}
		}

		public IByteProtocolBuffer Append( int data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Append ) );
				return this.MessageBuffer.Append( data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Append ) );

			}
		}

		public IByteProtocolBuffer Append( long data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Append ) );
				return this.MessageBuffer.Append( data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Append ) );

			}
		}

		public IByteProtocolBuffer Append( short data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Append ) );
				return this.MessageBuffer.Append( data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Append ) );

			}
		}

		public IByteProtocolBuffer Append( string data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Append ) );
				return this.MessageBuffer.Append( data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Append ) );

			}
		}

		public IByteProtocolBuffer Append( string data, int length ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Append ) );
				return this.MessageBuffer.Append( data, length );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Append ) );

			}
		}

		public IByteProtocolBuffer Append( uint data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Append ) );
				return this.MessageBuffer.Append( data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Append ) );

			}
		}

		public IByteProtocolBuffer Append( ulong data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Append ) );
				return this.MessageBuffer.Append( data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Append ) );

			}
		}

		public IByteProtocolBuffer Append( ushort data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Append ) );
				return this.MessageBuffer.Append( data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Append ) );

			}
		}

		public void Clear() {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Clear ) );
				this.MessageBuffer.Clear();
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Clear ) );

			}
		}

		public IByteProtocolBuffer Extract( out byte data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Extract ) );
				return this.MessageBuffer.Extract( out data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Extract ) );
			}
		}

		public IByteProtocolBuffer Extract( out int data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Extract ) );
				return this.MessageBuffer.Extract( out data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Extract ) );
			}
		}

		public IByteProtocolBuffer Extract( out long data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Extract ) );
				return this.MessageBuffer.Extract( out data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Extract ) );
			}
		}

		public IByteProtocolBuffer Extract( out short data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Extract ) );
				return this.MessageBuffer.Extract( out data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Extract ) );
			}
		}

		public IByteProtocolBuffer Extract( out string data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Extract ) );
				return this.MessageBuffer.Extract( out data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Extract ) );
			}
		}

		public IByteProtocolBuffer Extract( out string data, int length ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Extract ) );
				return this.MessageBuffer.Extract( out data, length );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Extract ) );
			}
		}

		public IByteProtocolBuffer Extract( out uint data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Extract ) );
				return this.MessageBuffer.Extract( out data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Extract ) );
			}
		}

		public IByteProtocolBuffer Extract( out ulong data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Extract ) );
				return this.MessageBuffer.Extract( out data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Extract ) );
			}
		}

		public IByteProtocolBuffer Extract( out ushort data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Extract ) );
				return this.MessageBuffer.Extract( out data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Extract ) );
			}
		}

		public byte[] ToArray() {
			try {
				BlynkLogManager.LogMethodBegin( nameof( ToArray ) );
				if( this.appendLength ) {
					UInt16 bodyLength = ( ushort )( this.MessageBuffer.Position - 5 );
					this.MessageBuffer.SetValue( bodyLength, 3 );
				}
				return this.MessageBuffer.ToArray();
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( ToArray ) );
			}
		}

		public void SetValue( ushort data, ushort offset ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( SetValue ) );
				this.MessageBuffer.SetValue( data, offset );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( SetValue ) );
			}
		}

		public IByteProtocolBuffer Extract( out IByteProtocolBuffer protocolBuffer, int length ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Extract ) );
				protocolBuffer = null;
				return this.MessageBuffer.Extract( out protocolBuffer, length );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Extract ) );
			}
		}

		public IByteProtocolBuffer Append( List<string> data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Append ) );

				return this.MessageBuffer.Append( data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Append ) );
			}
		}

		public IByteProtocolBuffer Extract( List<string> data ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Extract ) );
				return this.MessageBuffer.Extract( data );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Extract ) );
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose( bool disposing ) {
			if( !disposedValue ) {
				if( disposing ) {
					if( this.MessageBuffer != null ) {
						this.MessageBuffer.Dispose();
						this.MessageBuffer = null;
					}
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~BlynkCommand() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose() {
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose( true );
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
