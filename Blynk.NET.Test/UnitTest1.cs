using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blynk.NET.Test {
	[TestClass]
	public class UnitTest1 {
		[TestMethod]
		public void TestMethod1() {
			using( var vut = new ByteProtocolBuffer() ) {

				byte as8 = ( byte )1;
				Int16 as16 = 2;
				Int32 as32 = 3;
				Int64 as64 = 4;

				UInt16 asu16 = 5;
				UInt32 asu32 = 6;
				UInt64 asu64 = 7;

				string asString1 = Guid.NewGuid().ToString();
				string asString2 = Guid.NewGuid().ToString();

				vut.Append( as8 )
					.Append( as16 )
					.Append( as32 )
					.Append( as64 )
					.Append( asString1 )
					.Append( asu16 )
					.Append( asu32 )
					.Append( asu64 )
					.Append( asString1 )
					.Append( asString2 );

				var asArray = vut.ToArray();


				using( var extractor = new ByteProtocolBuffer( asArray ) ) {

					byte out8;
					Int16 out16;
					Int32 out32;
					Int64 out64;

					UInt16 outu16;
					UInt32 outu32;
					UInt64 outu64;

					string outString1;
					string outString2;
					string outString3;

					extractor.Extract( out out8 )
						.Extract( out out16 )
						.Extract( out out32 )
						.Extract( out out64 )
						.Extract( out outString1 )
						.Extract( out outu16 )
						.Extract( out outu32 )
						.Extract( out outu64 )
						.Extract( out outString2 )
						.Extract( out outString3 );

					Assert.AreEqual( as8, out8 );
					Assert.AreEqual( as16, out16 );
					Assert.AreEqual( as32, out32 );
					Assert.AreEqual( as64, out64 );
					Assert.AreEqual( asString1, outString1 );
					Assert.AreEqual( asu16, outu16 );
					Assert.AreEqual( asu32, outu32 );
					Assert.AreEqual( asu64, outu64 );
					Assert.AreEqual( asString1, outString2 );
					Assert.AreEqual( asString2, outString3 );
				}
			}
		}
	}
}
