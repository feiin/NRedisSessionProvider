using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.SessionState;

namespace NRedisSessionProvider
{
	public class SessionSerializer
	{
		public static Dictionary<string, object> Serialize(SessionStateItemCollection items)
		{
			if (items == null || items.Count == 0)
			{
				return null;

			}

			var  dict = new Dictionary<string, object>(items.Count);
			for (int i = 0; i < items.Keys.Count; i++)
			{
				var key = items.Keys[i];
				dict.Add(key, items[key]);
			}
			return dict;
		}

		public static SessionStateItemCollection Deserialize(Dictionary<string, object> sessionData)
		{
			if (sessionData == null || sessionData.Count == 0)
			{
				return new SessionStateItemCollection();
			}

			var collections = new SessionStateItemCollection();

			foreach (KeyValuePair<string, object> kvp in sessionData)
			{
				collections[kvp.Key] = kvp.Value;
			}
			return collections;
		}
	}
}

