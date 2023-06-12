using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace UniverseEngine
{
	/// <summary>
	/// 数据存储以小端字节序为标准
	/// </summary>
	public class BufferWriter
	{
		readonly byte[] m_Buffer;
		int m_Index;

		public BufferWriter(int capacity)
		{
			m_Index = 0;
			m_Buffer = new byte[capacity];
		}

		/// <summary>
		/// 缓冲区容量
		/// </summary>
		public int Capacity => m_Buffer.Length;

		/// <summary>
		/// 清空缓冲区
		/// </summary>
		public void Clear()
		{
			m_Index = 0;
		}

		/// <summary>
		/// 将有效数据写入文件流
		/// </summary>
		public void WriteToStream(FileStream fileStream)
		{
			fileStream.Write(m_Buffer, 0, m_Index);
		}

		/// <summary>
		/// 异步写入有效数据
		/// </summary>
		/// <param name="fileStream"></param>
		public async UniTask<bool> WriteToStreamAsync(FileStream fileStream)
		{
			try
			{
				await fileStream.WriteAsync(m_Buffer, 0, m_Index);
			}
			catch (Exception e)
			{
				Log.Exception(e);
				return false;
			}

			return true;
		}

		public void WriteBytes(byte[] data)
		{
			int count = data.Length;
			CheckWriterIndex(count);
			Buffer.BlockCopy(data, 0, m_Buffer, m_Index, count);
			m_Index += count;
		}
		public void WriteByte(byte value)
		{
			CheckWriterIndex(1);
			m_Buffer[m_Index++] = value;
		}

		public void WriteBool(bool value)
		{
			WriteByte((byte)(value ? 1 : 0));
		}

		public void WriteInt16(short value)
		{
			WriteUInt16((ushort)value);
		}

		public void WriteUInt16(ushort value)
		{
			CheckWriterIndex(2);
			m_Buffer[m_Index++] = (byte)value;
			m_Buffer[m_Index++] = (byte)(value >> 8);
		}

		public void WriteInt32(int value)
		{
			WriteUInt32((uint)value);
		}

		public void WriteUInt32(uint value)
		{
			CheckWriterIndex(4);
			m_Buffer[m_Index++] = (byte)value;
			m_Buffer[m_Index++] = (byte)(value >> 8);
			m_Buffer[m_Index++] = (byte)(value >> 16);
			m_Buffer[m_Index++] = (byte)(value >> 24);
		}

		public void WriteInt64(long value)
		{
			WriteUInt64((ulong)value);
		}

		public void WriteUInt64(ulong value)
		{
			CheckWriterIndex(8);
			m_Buffer[m_Index++] = (byte)value;
			m_Buffer[m_Index++] = (byte)(value >> 8);
			m_Buffer[m_Index++] = (byte)(value >> 16);
			m_Buffer[m_Index++] = (byte)(value >> 24);
			m_Buffer[m_Index++] = (byte)(value >> 32);
			m_Buffer[m_Index++] = (byte)(value >> 40);
			m_Buffer[m_Index++] = (byte)(value >> 48);
			m_Buffer[m_Index++] = (byte)(value >> 56);
		}

		public void WriteUTF8(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				WriteUInt16(0);
			}
			else
			{
				byte[] bytes = Encoding.UTF8.GetBytes(value);
				int count = bytes.Length;
				if (count > ushort.MaxValue)
					throw new FormatException($"Write string length cannot be greater than {ushort.MaxValue} !");

				WriteUInt16(Convert.ToUInt16(count));
				WriteBytes(bytes);
			}
		}

		public void WriteInt32Array(IReadOnlyList<int> values)
		{
			if (values == null)
			{
				WriteUInt16(0);
			}
			else
			{
				int count = values.Count;
				if (count > ushort.MaxValue)
				{
					throw new FormatException($"Write array length cannot be greater than {ushort.MaxValue} !");
				}

				WriteUInt16(Convert.ToUInt16(count));
				foreach (int value in values)
				{
					WriteInt32(value);
				}
			}
		}

		public void WriteInt64Array(long[] values)
		{
			if (values == null)
			{
				WriteUInt16(0);
			}
			else
			{
				int count = values.Length;
				if (count > ushort.MaxValue)
					throw new FormatException($"Write array length cannot be greater than {ushort.MaxValue} !");

				WriteUInt16(Convert.ToUInt16(count));
				for (int i = 0; i < count; i++)
				{
					WriteInt64(values[i]);
				}
			}
		}

		public void WriteUTF8Array(List<string> values)
		{
			if (values == null)
			{
				WriteUInt16(0);
			}
			else
			{
				int count = values.Count;
				if (count > ushort.MaxValue)
				{
					throw new FormatException($"Write array length cannot be greater than {ushort.MaxValue} !");
				}

				WriteUInt16(Convert.ToUInt16(count));
				for (int i = 0; i < count; i++)
				{
					WriteUTF8(values[i]);
				}
			}
		}

		[Conditional("DEBUG")]
		private void CheckWriterIndex(int length)
		{
			if (m_Index + length > Capacity)
			{
				throw new IndexOutOfRangeException();
			}
		}
	}
}
