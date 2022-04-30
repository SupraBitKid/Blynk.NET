using System;
using System.Collections.Generic;
using System.Linq;

namespace Blynk.NET.Utility
{
	public class ArrayManager<T> where T : new() {

		protected class ArrayEntry {
			public T[] Array;
			public bool InUse;
		}

		protected List<ArrayEntry> ArrayEntries = new List<ArrayEntry>();

		protected ArrayManager() {}

		private static ArrayManager<T> singleton = new ArrayManager<T>();

		protected T[] GetArrayImpl( uint atLeastItemCount ) {
			ArrayEntry foundEntry = null;
			lock( this.ArrayEntries ) {
				foundEntry = this.ArrayEntries.FirstOrDefault( ae => ae.InUse == false && ae.Array.Length >= atLeastItemCount );
				if( foundEntry == null ) {
					foundEntry = new ArrayEntry() {
						Array = new T[ atLeastItemCount ],
						InUse = true
					};
					this.ArrayEntries.Add( foundEntry );
				}
			}
			return foundEntry.Array;
		}

		protected void ReleaseArrayImpl( T[] array ) {
			ArrayEntry foundEntry = null;
			lock( this.ArrayEntries ) {
				foundEntry = this.ArrayEntries.FirstOrDefault( ae => ae.Array == array );
				if( foundEntry != null ) {
					foundEntry.InUse = false;
				}
			}
		}

		public static T[] GetArray( uint desiredItemCount ) {
			return ArrayManager<T>.singleton.GetArrayImpl( desiredItemCount );
		}

		public static T[] GetArray( int desiredItemCount ) {
			return ArrayManager<T>.singleton.GetArrayImpl( ( uint )desiredItemCount );
		}

		public static void ReleaseArray( T[] array ) {
			ArrayManager<T>.singleton.ReleaseArrayImpl( array );
		}
    }
}
