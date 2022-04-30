using System;
using System.Collections.Generic;

namespace Blynk.NET.Interface {
	public interface IByteProtocolBuffer {
		ushort Length { get; }
		ushort Position { get; }
		void Clear();
		byte[] ToArray();
		void SetValue( UInt16 data, UInt16 offset );

		IByteProtocolBuffer Append( byte data );
		IByteProtocolBuffer Append( byte[] data );
		IByteProtocolBuffer Append( int data );
		IByteProtocolBuffer Append( long data );
		IByteProtocolBuffer Append( short data );
		IByteProtocolBuffer Append( string data );
		IByteProtocolBuffer Append( string data, int length );
		IByteProtocolBuffer Append( uint data );
		IByteProtocolBuffer Append( ulong data );
		IByteProtocolBuffer Append( ushort data );
		IByteProtocolBuffer Append( List<string> data );

		IByteProtocolBuffer Extract( out byte data );
		IByteProtocolBuffer Extract( out int data );
		IByteProtocolBuffer Extract( out long data );
		IByteProtocolBuffer Extract( out short data );
		IByteProtocolBuffer Extract( out string data );
		IByteProtocolBuffer Extract( out uint data );
		IByteProtocolBuffer Extract( out ulong data );
		IByteProtocolBuffer Extract( out ushort data );
		IByteProtocolBuffer Extract( out IByteProtocolBuffer protocolBuffer, int length );
		IByteProtocolBuffer Extract( out string data, int length );
		IByteProtocolBuffer Extract( List<string> data );

	}
}