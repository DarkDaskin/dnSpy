#if NETFRAMEWORK
// ReSharper disable once CheckNamespace
namespace System.Buffers.Binary {
	public static class BinaryPrimitives {
		public static ushort ReverseEndianness(ushort value) =>
			(ushort)(((value & 0xFF) << 8) | ((value & 0xFF00) >> 8));

		public static short ReverseEndianness(short value) =>
			(short)ReverseEndianness((ushort)value);

		public static uint ReverseEndianness(uint value) =>
			(uint)(((value & 0xFF) << 24) | ((value & 0xFF00) << 8) | ((value & 0xFF0000) >> 8) | ((value & 0xFF000000) >> 24));

		public static int ReverseEndianness(int value) =>
			(int)ReverseEndianness((uint)value);

		public static ulong ReverseEndianness(ulong value) =>
			((ulong)ReverseEndianness((uint)value) << 32) + (ulong)ReverseEndianness((uint)(value >> 32));

		public static long ReverseEndianness(long value) =>
			(long)ReverseEndianness((ulong)value);
	}
}
#endif
