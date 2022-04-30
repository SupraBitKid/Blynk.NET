using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Blynk.NET.Logging;
using Blynk.NET.Pins;

namespace Blynk.NET {
	public class BlynkConnection : IDisposable {

		public BlynkConnection( string host, int port, string authentication ) { //, bool withSSL ) {
			try {
				BlynkLogManager.LogMethodBegin( "BlynkConnection.ctor" );
				this.host = host;
				this.port = port;
				this.authentication = authentication;
				this.tcpClient = new TcpClient( AddressFamily.InterNetwork );
				this.tcpClient.NoDelay = true;
				this.backgroundCancellationTokenSource = new CancellationTokenSource();
				//this.withSSL = withSSL;
				this.PingIntervalInMilliseconds = 5000;
				this.BackgroundReadIntervalInMilliseconds = 100;
			}
			finally {
				BlynkLogManager.LogMethodEnd( "BlynkConnection.ctor" );
			}
		}

		private string host;
		private int port;
		private string authentication;
		private TcpClient tcpClient;
		private NetworkStream tcpStream;
		//private bool withSSL;
		//private SslStream sslStream;
		private object messageidLock = new object();
		private UInt16 messageID;
		private CancellationTokenSource backgroundCancellationTokenSource;

		public PinNotification<DigitalPin> DigitalPinNotification { get; private set; } = new PinNotification<DigitalPin>();
		public PinNotification<AnalogPin> AnalogPinNotification { get; private set; } = new PinNotification<AnalogPin>();
		public PinNotification<VirtualPin> VirtualPinNotification { get; private set; } = new PinNotification<VirtualPin>();

		public Action<string,PinMode> PinModeNotification { get; set; }
		public Action<int> ResponseReceivedNotification{ get; set; }

		internal UInt16 NextMessageId {
			get {
				lock( this.messageidLock ) {
					return this.messageID++;
				}
			}
		}

		public bool Connected {
			get {
				return this.tcpClient?.Connected ?? false;
			}
		}

		public CancellationToken CancellationToken {
			get {
				return this.backgroundCancellationTokenSource.Token;
			}
		}

		public int PingIntervalInMilliseconds { get; set; }
		public int BackgroundReadIntervalInMilliseconds { get; set; }

		public bool Connect() {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Connect ) );
				CancellationTokenSource cancellationTokenSource = new CancellationTokenSource( 50000 ); // this.withSSL ? 10000 : 5000 ); // five seconds for connect & login
				return this.ConnectAsync( cancellationTokenSource.Token ).GetAwaiter().GetResult();
			}
			catch( Exception ex ) {
				BlynkLogManager.LogException( "Exception while connecting", ex );
				return false;
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Connect ) );
			}
		}

		public async Task<bool> ConnectAsync( CancellationToken cancellationToken ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( ConnectAsync ) );
				this.messageID = 1;

				await this.tcpClient.ConnectAsync( this.host, this.port );

				if( this.tcpClient.Connected ) {
					this.tcpClient.NoDelay = true;
					this.tcpStream = this.tcpClient.GetStream();

					//if( this.withSSL ) {
					//	this.sslStream = new SslStream( this.tcpStream );
					//	await this.sslStream.AuthenticateAsClientAsync( this.host );
					//}

					using( var loginCommand = new BlynkCommand( BlynkCommandType.BLYNK_CMD_LOGIN, this.NextMessageId ) ) {
						loginCommand.Append( this.authentication, this.authentication.Length );

						var dummy2 = Task.Run( () => this.GetMessageOnSchedule( this.backgroundCancellationTokenSource.Token ) );

						await this.SendAsync( loginCommand.ToArray(), cancellationToken );
					}

					var dummy = Task.Run( () => this.SendPingsOnSchedule( this.backgroundCancellationTokenSource.Token ) );

					return true;
				}
				return false;
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( ConnectAsync ) );
			}
		}

		public void Disconnect() {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Disconnect ) );
				this.backgroundCancellationTokenSource.Cancel();
				this.tcpStream.Close();
				this.tcpClient.Close();
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Disconnect ) );
			}
		}

		internal async Task<bool> SendResponseAsync( UInt16 originalMessageId, BlynkResponse response = BlynkResponse.OK ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( SendResponseAsync ) );
				var command = new BlynkCommand( BlynkCommandType.BLYNK_CMD_RESPONSE, originalMessageId, false );

				command.Append( ( byte )0 )
					.Append( ( byte )response );

				return await this.SendAsync( command.ToArray(), backgroundCancellationTokenSource.Token );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( SendResponseAsync ) );
			}
		}

		public async Task<bool> SendAsync( byte[] byteBuffer, CancellationToken cancellationToken ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( SendAsync ) );
				await this.tcpStream.WriteAsync( byteBuffer, 0, byteBuffer.Length, cancellationToken );
				await this.tcpStream.FlushAsync( cancellationToken );
				return true;
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( SendAsync ) );
			}
		}

		internal async Task<bool> ReceiveAsync( CancellationToken cancellationToken ) {
			byte[] receiveBuffer = null;
			try {
				BlynkLogManager.LogMethodBegin( nameof( ReceiveAsync ) );
				if( this.tcpStream.DataAvailable ) {
					BlynkLogManager.LogInformation( "data available" );

					receiveBuffer = Utility.ArrayManager<byte>.GetArray( this.tcpClient.Available ); // new byte[ this.tcpClient.Available ];
					int count = await this.tcpStream.ReadAsync( receiveBuffer, 0, receiveBuffer.Length, cancellationToken );

					var commands = BlynkMessageParser.GetBlynkMessages( receiveBuffer, count );

					return await this.ProcessMessagesAsync( commands );
				}
				//else {
				//	Console.Write( "." );
				//}
				return false;
			}
			finally {
				if( receiveBuffer != null ) {
					Utility.ArrayManager<byte>.ReleaseArray( receiveBuffer );
					receiveBuffer = null;
				}

				BlynkLogManager.LogMethodEnd( nameof( ReceiveAsync ) );
			}
		}

		internal async Task GetMessageOnSchedule( CancellationToken cancellationToken ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( GetMessageOnSchedule ) );
				while( !cancellationToken.IsCancellationRequested ) {
					try {
						if( this.Connected ) {
							var receivedMessages = await this.ReceiveAsync( cancellationToken );
							if( receivedMessages )
								BlynkLogManager.LogInformation( "received" );
							await Task.Delay( this.BackgroundReadIntervalInMilliseconds, cancellationToken );
						}
						else {
							BlynkLogManager.LogInformation( "not connected" );
							await Task.Delay( 10 * this.BackgroundReadIntervalInMilliseconds, cancellationToken );
						}
					}
					catch( TaskCanceledException ) { }
					catch( Exception ex ) {
						BlynkLogManager.LogException( "Background message receiver", ex );
					}
				}
				BlynkLogManager.LogInformation( "canceled" );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( GetMessageOnSchedule ) );
			}
		}

		private async Task SendPingsOnSchedule( CancellationToken cancellationToken ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( SendPingsOnSchedule ) );
				while( !cancellationToken.IsCancellationRequested ) {
					try {
						if( this.Connected ) {
							BlynkLogManager.LogInformation( "ping" );
							var pingCommand = new BlynkCommand( BlynkCommandType.BLYNK_CMD_PING, NextMessageId, false );

							pingCommand.Append( ( Int16 )0 );

							await this.SendAsync( pingCommand.ToArray(), cancellationToken );
						}
						else {
							BlynkLogManager.LogInformation( "not connected" );
						}
						await Task.Delay( this.PingIntervalInMilliseconds, cancellationToken );
					}
					catch( TaskCanceledException ){ }
					catch( Exception ex ) {
						BlynkLogManager.LogException( "Background ping sender", ex );
					}
				}
				BlynkLogManager.LogInformation( "canceled" );
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( SendPingsOnSchedule ) );
			}
		}

		private async Task<bool> ProcessMessagesAsync( IEnumerable<BlynkMessageParser> blynkMessages ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( ProcessMessagesAsync ) );
				bool totalResult = false;
				foreach( var blynkMessage in blynkMessages ) {
					bool result = await blynkMessage.ParseMessageAsync( this );
					totalResult = totalResult && result;
				}
				return totalResult;
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( ProcessMessagesAsync ) );
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose( bool disposing ) {
			try {
				BlynkLogManager.LogMethodBegin( nameof( Dispose ) );
				if( !disposedValue ) {
					if( disposing ) {
						if( this.tcpClient != null ) {
							this.tcpClient.Dispose();
							this.tcpClient = null;
						}

						if( this.tcpStream != null ) {
							this.tcpStream.Dispose();
							this.tcpStream = null;
						}
					}

					disposedValue = true;
				}
			}
			finally {
				BlynkLogManager.LogMethodEnd( nameof( Dispose ) );
			}
		}

		public void Dispose() {
			Dispose( true );
		}
		#endregion
	}
}
