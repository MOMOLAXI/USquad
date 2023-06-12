using System.IO;

namespace UniverseEngine
{
	internal class FileCacheMeta
	{
		static readonly BufferWriter s_SharedBuffer = new(1024);

		public readonly string InfoFilePath;
		public readonly string DataFilePath;
		public readonly string DataFileCRC;
		public readonly long DataFileSize;
		public readonly FileInfo FileInfo;

		public FileCacheMeta(string infoFilePath, string dataFilePath, string dataFileCRC, long dataFileSize)
		{
			InfoFilePath = infoFilePath;
			DataFilePath = dataFilePath;
			DataFileCRC = dataFileCRC;
			DataFileSize = dataFileSize;
			FileInfo = new(dataFilePath);
		}

		public void WriteInfoToFile(string filePath, string dataFileCRC, long dataFileSize)
		{
			using (FileStream fs = new(filePath, FileMode.Create))
			{
				s_SharedBuffer.Clear();
				s_SharedBuffer.WriteUTF8(dataFileCRC);
				s_SharedBuffer.WriteInt64(dataFileSize);
				s_SharedBuffer.WriteToStream(fs);
				fs.Flush();
			}
		}

		public void Delete()
		{
			if (FileInfo.Exists)
			{
				FileInfo.Directory.Delete(true);
			}
		}
	}
}
