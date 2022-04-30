using Blynk.NET.Interface;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Blynk.NET { 
    public class ByteProtocolBuffer : IDisposable, IByteProtocolBuffer {
		public ByteProtocolBuffer( ) {
			this.releasableArray = Utility.ArrayManager<byte>.GetArray( 128 );
			this.memory = new Memory<byte>( this.releasableArray ); // new byte[ 1024 ] );
			this.Clear();
		}

		public ByteProtocolBuffer( byte[] incomingBuffer ) {
			this.memory = new Memory<byte>( incomingBuffer );
			this.releasableArray = null;
			this.Clear();
		}

		public ByteProtocolBuffer( ByteProtocolBuffer that, int length ) {
			this.memory = that.memory.Slice( that.memoryOffset, length );
			this.releasableArray = null;
			this.Clear();
		}

		public void Clear() {
			this.memoryOffset = 0;
		}

		private Span<byte> GetSpanFromMemory(  ) {
			return this.memory.Span.Slice( this.memoryOffset );
		}

		private Span<byte> GetSpanFromMemoryAbsolute( int offset, int length ) {
			return this.memory.Span.Slice( offset, length );
		}

		private Span<byte> GetSpanFromMemory( int length ) {
			return this.memory.Span.Slice( this.memoryOffset, length );
		}

		private Span<byte> GetAppendableSpan( int length ) {
			var result = this.memory.Span.Slice( this.memoryOffset, length );
			this.memoryOffset += length;
			return result;
		}

		private ReadOnlySpan<byte> GetParseableSpan( int length ) {
			var result = this.memory.Span.Slice( this.memoryOffset, length );
			this.memoryOffset += length;
			return ( ReadOnlySpan<byte> )result;
		}

		private Memory<byte> memory;
		private int memoryOffset;
		private byte[] releasableArray;


		public IByteProtocolBuffer Append( Int16 data ) {
			BinaryPrimitives.WriteInt16BigEndian( this.GetAppendableSpan( sizeof( Int16 ) ), data );
			return this;
		}

		public IByteProtocolBuffer Append( Int32 data ) {
			BinaryPrimitives.WriteInt32BigEndian( this.GetAppendableSpan( sizeof( Int32 ) ), data );
			return this;
		}

		public IByteProtocolBuffer Append( Int64 data ) {
			BinaryPrimitives.WriteInt64BigEndian( this.GetAppendableSpan( sizeof( Int64 ) ), data );
			return this;
		}

		public IByteProtocolBuffer Append( UInt16 data ) {
			BinaryPrimitives.WriteUInt16BigEndian( this.GetAppendableSpan( sizeof( UInt16 ) ), data );
			return this;
		}

		public IByteProtocolBuffer Append( UInt32 data ) {
			BinaryPrimitives.WriteUInt32BigEndian( this.GetAppendableSpan( sizeof( UInt32 ) ), data );
			return this;
		}

		public IByteProtocolBuffer Append( UInt64 data ) {
			BinaryPrimitives.WriteUInt64BigEndian( this.GetAppendableSpan( sizeof( UInt64 ) ), data );
			return this;
		}

		//public IByteProtocolBuffer Append( float data )
		//public IByteProtocolBuffer Append( double data )

		public IByteProtocolBuffer Append( string data ) {
			var span = this.GetAppendableSpan( data.Length + 1 );
			var asBytes = Encoding.ASCII.GetBytes( data );
			asBytes.CopyTo( span );
			span[ data.Length ] = ( byte )0;
			return this;
		}

		public IByteProtocolBuffer Append( string data, int length ) {
			var span = this.GetAppendableSpan( length );
			var dataAsSpan = new Span<byte>( Encoding.ASCII.GetBytes( data ) );
			dataAsSpan.CopyTo( span );
			return this;
		}

		public IByteProtocolBuffer Append( byte[] data ) {
			var span = this.GetAppendableSpan( data.Length );
			data.CopyTo( span );
			return this;
		}

		public IByteProtocolBuffer Append( byte data ) {
			var span = GetAppendableSpan( sizeof( byte ) );
			span[ 0 ] = data;
			return this;
		}

		public IByteProtocolBuffer Extract( out byte data ) {
			var span = GetParseableSpan( sizeof( byte ) );
			data = span[ 0 ];
			return this;
		}

		public IByteProtocolBuffer Extract( out Int16 data ) {
			var span = GetParseableSpan( sizeof( Int16 ) );
			data = BinaryPrimitives.ReadInt16BigEndian( span );
			return this;
		}

		public IByteProtocolBuffer Extract( out Int32 data ) {
			var span = GetParseableSpan( sizeof( Int32 ) );
			data = BinaryPrimitives.ReadInt32BigEndian( span );
			return this;
		}

		public IByteProtocolBuffer Extract( out Int64 data ) {
			var span = GetParseableSpan( sizeof( Int64 ) );
			data = BinaryPrimitives.ReadInt64BigEndian( span );
			return this;
		}

		public IByteProtocolBuffer Extract( out UInt16 data ) {
			var span = GetParseableSpan( sizeof( UInt16 ) );
			data = BinaryPrimitives.ReadUInt16BigEndian( span );
			return this;
		}

		public IByteProtocolBuffer Extract( out UInt32 data ) {
			var span = GetParseableSpan( sizeof( UInt32 ) );
			data = BinaryPrimitives.ReadUInt32BigEndian( span );
			return this;
		}

		public IByteProtocolBuffer Extract( out UInt64 data ) {
			var span = GetParseableSpan( sizeof( UInt64 ) );
			data = BinaryPrimitives.ReadUInt64BigEndian( span );
			return this;
		}

		//public IByteProtocolBuffer Extract( out float data )
		//public IByteProtocolBuffer Extract( out double data )

		public IByteProtocolBuffer Extract( out string data, int length ) {
			var span = GetParseableSpan( length );
			data = Encoding.ASCII.GetString( span.ToArray() );
			return this;
		}

		public IByteProtocolBuffer Extract( out string data ) {
			var span = GetSpanFromMemory();
			var stringEnd = span.IndexOf( ( byte )0 );
			if( stringEnd == -1 )
				stringEnd = this.Length - this.Position;

			var stringSpan = span.Slice( 0, stringEnd );

			data = Encoding.ASCII.GetString( stringSpan.ToArray() );
			this.memoryOffset += stringEnd + 1;
			return this;
		}

		public IByteProtocolBuffer Extract( out IByteProtocolBuffer protocolBuffer, int length ) {
			protocolBuffer = new ByteProtocolBuffer( this, length );
			this.memoryOffset += length;
			return this;
		}

		public void SetValue( UInt16 data, UInt16 offset ) {
			BinaryPrimitives.WriteUInt16BigEndian( this.GetSpanFromMemoryAbsolute( offset, sizeof( Int16 ) ), data );
		}

		public UInt16 Length { get { return ( UInt16 )this.memory.Length; } }
		public UInt16 Position { get { return ( UInt16 )this.memoryOffset; } }

		public byte[] ToArray() {
			return this.memory.Slice( 0, this.memoryOffset ).ToArray();
		}

		public IByteProtocolBuffer Append( List<string> data ) {
			data.ForEach( d => this.Append( d ) );
			return this;
		}

		public IByteProtocolBuffer Extract( List<string> data ) {
			while( this.Position < this.Length ) {
				this.Extract( out string tempData );
				data.Add( tempData );
			}
			return this;
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose( bool disposing ) {
			if( !disposedValue ) {
				if( disposing ) {
					if( this.releasableArray != null ) {
						Utility.ArrayManager<byte>.ReleaseArray( this.releasableArray );
						this.releasableArray = null;
					}
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~ByteProtocolBuffer() {
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
