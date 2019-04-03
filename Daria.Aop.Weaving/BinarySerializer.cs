using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Daria.Aop.Weaving
{
	public interface IBinarySerializer
	{
		byte[] Serialize<T>(T obj);
		T Deserialize<T>(byte[] bytes);
	}
	
	public class BinarySerializer : IBinarySerializer
	{
		private readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();
        
		public byte[] Serialize<T>(T obj)
		{
			using (var stream = new MemoryStream())
			{
				_binaryFormatter.Serialize(stream, obj);
				return stream.ToArray();
			}
		}

		public T Deserialize<T>(byte[] bytes)
		{
			throw new NotImplementedException();
		}
	}
}