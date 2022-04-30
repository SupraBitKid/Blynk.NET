using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Blynk.NET.IoT {
	public sealed class StartupTask : IBackgroundTask {
		public async void Run( IBackgroundTaskInstance taskInstance ) {
			BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

			//BlynkLogManager.BlynkLogger = new ConsoleBlynkLogger( ConsoleBlynkLogger.LogLevels.LogExceptions
			//		| ConsoleBlynkLogger.LogLevels.LogWarnings
			//		| ConsoleBlynkLogger.LogLevels.LogInformation );

			var connection = new BlynkConnection( "blynk-cloud.com", 8442, "-secret here-" );
			if( connection.Connect() ) {

				InitializePins();

				connection.AnalogPinNotification.PinWriteNotification = pin => {
					//Console.WriteLine( "Analog pin received A{0} : {1}", pin.PinNumber, pin.Value );
					Program.analogPinList[ pin.PinNumber ].Value = pin.Value;
				};

				connection.DigitalPinNotification.PinWriteNotification = pin => {
					//Console.WriteLine( "Digital pin received D{0} : {1}", pin.PinNumber, pin.Value );
					Program.digitalPinList[ pin.PinNumber ].Value = pin.Value;
				};

				connection.AnalogPinNotification.PinReadRequest = pinNumber => {
					//Console.WriteLine( "Analog pin A{0} requested", pinNumber );
					var result = Program.analogPinList[ pinNumber ];
					result.Value = ( short )( ( result.Value + 1 ) % 16 );
					return result;
				};

				connection.DigitalPinNotification.PinReadRequest = pinNumber => {
					//Console.WriteLine( "Digital pin D{0} requested", pinNumber );
					var result = Program.digitalPinList[ pinNumber ];
					result.Value = !result.Value;
					return result;
				};

				connection.VirtualPinNotification.PinReadRequest = pinNumber => {
					//Console.WriteLine( "Virtual pin V{0} requested", pinNumber );
					var result = Program.virtualPinList[ pinNumber ];
					result.Values.Clear();
					result.Values.Add( "255" );
					return result;
				};

				//connection.PinModeNotification = ( pinNumber, pinMode ) => Console.WriteLine( "PinMode {0} : {1}", pinNumber, pinMode );

				//connection.ResponseReceivedNotification = ( code ) => Console.WriteLine( "Response : {0}", code );

				//Console.WriteLine( "hit a key to exit" );
				//Console.ReadKey();

				//connection.Disconnect();
			}
			else {
				deferral.Complete();
			}

			//Console.WriteLine( "exiting" );
			//System.Threading.Thread.Sleep( 2000 );
		}
	}

	static List<Pins.VirtualPin> virtualPinList = new List<Pins.VirtualPin>();
	static List<Pins.AnalogPin> analogPinList = new List<Pins.AnalogPin>();
	static List<Pins.DigitalPin> digitalPinList = new List<Pins.DigitalPin>();

	static void InitializePins() {
		for( short index = 0; index < 256; index++ ) {
			virtualPinList.Add( new Pins.VirtualPin() { PinNumber = index } );
			analogPinList.Add( new Pins.AnalogPin() { PinNumber = index, Value = index } );
			digitalPinList.Add( new Pins.DigitalPin() { PinNumber = index, Value = ( ( index % 2 ) == 0 ) } );
		}
	}
}

