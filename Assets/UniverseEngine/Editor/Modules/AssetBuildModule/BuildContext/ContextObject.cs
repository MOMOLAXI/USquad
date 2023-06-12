namespace UniverseEngine.Editor
{
	public class ContextObject<T> : IContextObject
	{
		public T Value;
		public ContextObject(T value)
		{
			Value = value;
		}

		public static implicit operator T(ContextObject<T> contextObject)
		{
			return contextObject.Value;
		}

		public static implicit operator ContextObject<T>(T value)
		{
			return new(value);
		}
	}
}
