using System.IO;

namespace UniverseEngine
{
	public class BundleStream : FileStream
	{
		public const byte KEY = 64;

		public BundleStream(string path,
		                    FileMode mode,
		                    FileAccess access,
		                    FileShare share,
		                    int bufferSize,
		                    bool useAsync)
			: base(path, mode, access, share, bufferSize, useAsync) { }
		public BundleStream(string path, FileMode mode) : base(path, mode) { }

		public override int Read(byte[] array, int offset, int count)
		{
			int index = base.Read(array, offset, count);
			
			for (int i = 0; i < array.Length; i++)
			{
				array[i] ^= KEY;
			}
			return index;
		}
	}
}
