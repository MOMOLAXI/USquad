using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace UniverseEngine
{
	public class BufferReader
	{
		readonly byte[] m_Buffer;
		int m_Index;

		/// <summary>
		/// 是否有效
		/// </summary>
		public bool IsValid => m_Buffer != null && m_Buffer.Length != 0;

		/// <summary>
		/// 缓冲区容量
		/// </summary>
		public int Capacity => m_Buffer.Length;

		public BufferReader(byte[] buffer)
		{
			m_Buffer = buffer;
			m_Index = 0;
		}

		public byte[] ReadBytes(int count)
		{
			CheckReaderIndex(count);
			byte[] data = new byte[count];
			Buffer.BlockCopy(m_Buffer, m_Index, data, 0, count);
			m_Index += count;
			return data;
		}

		public byte ReadByte()
		{
			CheckReaderIndex(1);
			return m_Buffer[m_Index++];
		}

		public bool ReadBool()
		{
			CheckReaderIndex(1);
			return m_Buffer[m_Index++] == 1;
		}

		public short ReadInt16()
		{
			CheckReaderIndex(2);
			if (BitConverter.IsLittleEndian)
			{
				short value = (short)(m_Buffer[m_Index] | m_Buffer[m_Index + 1] << 8);
				m_Index += 2;
				return value;
			}
			else
			{
				short value = (short)(m_Buffer[m_Index] << 8 | m_Buffer[m_Index + 1]);
				m_Index += 2;
				return value;
			}
		}

		public ushort ReadUInt16()
		{
			return (ushort)ReadInt16();
		}

		public int ReadInt32()
		{
			CheckReaderIndex(4);
			if (BitConverter.IsLittleEndian)
			{
				int value = m_Buffer[m_Index] | m_Buffer[m_Index + 1] << 8 | m_Buffer[m_Index + 2] << 16 | m_Buffer[m_Index + 3] << 24;
				m_Index += 4;
				return value;
			}
			else
			{
				int value = m_Buffer[m_Index] << 24 | m_Buffer[m_Index + 1] << 16 | m_Buffer[m_Index + 2] << 8 | m_Buffer[m_Index + 3];
				m_Index += 4;
				return value;
			}
		}

		public uint ReadUInt32()
		{
			return (uint)ReadInt32();
		}

		public long ReadInt64()
		{
			CheckReaderIndex(8);
			if (BitConverter.IsLittleEndian)
			{
				int i1 = m_Buffer[m_Index] | m_Buffer[m_Index + 1] << 8 | m_Buffer[m_Index + 2] << 16 | m_Buffer[m_Index + 3] << 24;
				int i2 = m_Buffer[m_Index + 4] | m_Buffer[m_Index + 5] << 8 | m_Buffer[m_Index + 6] << 16 | m_Buffer[m_Index + 7] << 24;
				m_Index += 8;
				return (uint)i1 | (long)i2 << 32;
			}
			else
			{
				int i1 = m_Buffer[m_Index] << 24 | m_Buffer[m_Index + 1] << 16 | m_Buffer[m_Index + 2] << 8 | m_Buffer[m_Index + 3];
				int i2 = m_Buffer[m_Index + 4] << 24 | m_Buffer[m_Index + 5] << 16 | m_Buffer[m_Index + 6] << 8 | m_Buffer[m_Index + 7];
				m_Index += 8;
				return (uint)i2 | (long)i1 << 32;
			}
		}

		public ulong ReadUInt64()
		{
			return (ulong)ReadInt64();
		}

		public string ReadUTF8()
		{
			ushort count = ReadUInt16();
			if (count == 0)
				return string.Empty;

			CheckReaderIndex(count);
			string value = Encoding.UTF8.GetString(m_Buffer, m_Index, count);
			m_Index += count;
			return value;
		}

		public void ReadInt32Array(List<int> result)
		{
			if (result == null)
			{
				return;
			}

			result.Clear();
			ushort count = ReadUInt16();
			for (int i = 0; i < count; i++)
			{
				int value = ReadInt32();
				result.Add(value);
			}
		}

		public void ReadInt64Array(List<long> result)
		{
			if (result == null)
			{
				return;
			}
			
			result.Clear();
			ushort count = ReadUInt16();
			for (int i = 0; i < count; i++)
			{
				long value = ReadInt64();
				result.Add(value);
			}
		}

		public void ReadUTF8Array(List<string> result)
		{
			if (result == null)
			{
				return;
			}
			
			result.Clear();
			ushort count = ReadUInt16();
			for (int i = 0; i < count; i++)
			{
				string value = ReadUTF8();
				result.Add(value);
			}
		}

		[Conditional("DEBUG")]
		void CheckReaderIndex(int length)
		{
			if (m_Index + length > Capacity)
			{
				throw new IndexOutOfRangeException();
			}
		}
	}
}
