using System;
using System.Collections.Generic;

// [PixelCrushers] Adds Tuples, which are missing from Unity's .NET.
namespace Language.Lua {

	/// <summary>
	/// Implements the Tuple classes missing from Monodevelop's .NET implementation.
	/// </summary>
	public static class Tuple {

		public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) {
			return new Tuple<T1, T2>(item1, item2);
		}
		
		public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) {
			return new Tuple<T1, T2, T3>(item1, item2, item3);
		}

	}

	public sealed class Tuple<T1, T2> {

		private readonly T1 item1;
		private readonly T2 item2;
		
		public T1 Item1	{ get { return item1; }	}
		public T2 Item2 { get { return item2; } }
		
		public Tuple(T1 item1, T2 item2) {
			this.item1 = item1;
			this.item2 = item2;
		}
		
		public override bool Equals(object o) {
			if (o == null) return false;
			if (o.GetType() != typeof(Tuple<T1, T2>)) return false;
			var other = (Tuple<T1, T2>) o;
			return (this == other);
		}
		
		public static bool operator==(Tuple<T1, T2> a, Tuple<T1, T2> b) {
			if ((object)a == null && (object)b == null) return true; 
			if ((object)a != null && (object)b == null) return false; 
			if ((object)a == null && (object)b != null) return false; 
			return a.item1.Equals(b.item1) && a.item2.Equals(b.item2);            
		}
		
		public static bool operator!=(Tuple<T1, T2> a, Tuple<T1, T2> b)	{
			return !(a == b);
		}
		
		public override int GetHashCode() {
			int hash = 17;
			hash = hash * 23 + item1.GetHashCode();
			hash = hash * 23 + item2.GetHashCode();
			return hash;
		}
		
	}

	public sealed class Tuple<T1, T2, T3> {

		private readonly T1 item1;
		private readonly T2 item2;
		private readonly T3 item3;
		
		public T1 Item1 { get { return item1; } }
		public T2 Item2 { get { return item2; }	}
		public T3 Item3	{ get { return item3; } }
		
		public Tuple(T1 item1, T2 item2, T3 item3) {
			this.item1 = item1;
			this.item2 = item2;
			this.item3 = item3;
		}
		
		public override bool Equals(object o) {
			if (o.GetType() != typeof(Tuple<T1, T2, T3>)) return false;
			var other = (Tuple<T1, T2, T3>)o;
			return (this == other);
		}
		
		public static bool operator==(Tuple<T1, T2, T3> a, Tuple<T1, T2, T3> b)	{
			return a.item1.Equals(b.item1) && a.item2.Equals(b.item2) && a.item3.Equals(b.item3);            
		}
		
		public static bool operator!=(Tuple<T1, T2, T3> a, Tuple<T1, T2, T3> b)	{
			return !(a == b);
		}

		public override int GetHashCode() {
			int hash = 17;
			hash = hash * 23 + item1.GetHashCode();
			hash = hash * 23 + item2.GetHashCode();
			hash = hash * 23 + item3.GetHashCode();
			return hash;
		}
		
	}

}