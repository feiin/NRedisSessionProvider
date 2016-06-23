using System;
using System.IO;
using System.Web;
using System.Web.SessionState;

namespace NRedisSessionProvider
{
	public class SessionSerializer
	{
		public static byte[] Serialize(SessionStateItemCollection items)
		{
			using (var ms = new MemoryStream())
			{
				using (var writer = new BinaryWriter(ms))
				{
					if (items != null)
						items.Serialize(writer);

					writer.Close();

					return ms.ToArray();
				}
			}
		}

		public static SessionStateItemCollection Deserialize(byte[] sessionData)
		{
			if (sessionData == null)
			{
				return new SessionStateItemCollection();
			}

			using (var ms = new MemoryStream(sessionData))
			{
				var sessionItems = new SessionStateItemCollection();

				if (ms.Length > 0)
				{
					using (BinaryReader reader = new BinaryReader(ms))
					{
						sessionItems = SessionStateItemCollection.Deserialize(reader);
					}
				}

				return sessionItems;
			}
		}
	}
}

