using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace UniverseEngine
{
	public class AssetException : Exception
	{
		public AssetException()
		{
		}

		protected AssetException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public AssetException(string message) : base(message)
		{
		}

		public AssetException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
