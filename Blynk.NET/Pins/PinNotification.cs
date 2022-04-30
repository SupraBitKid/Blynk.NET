using System;

namespace Blynk.NET.Pins {

	public class PinNotification<PinType> where PinType : BasePin {

		public Action<PinType> PinWriteNotification { get; set; }

		public Func<int, PinType> PinReadRequest { get; set; }
	}
}