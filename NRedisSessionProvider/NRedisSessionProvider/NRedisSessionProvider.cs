using System;
using System.Web;
using System.Web.SessionState;
using System.Configuration;
using System.Configuration.Provider;
using NServiceKit.Redis;

namespace NRedisProvider
{
	public class NRedisSessionProvider : SessionStateStoreProviderBase
	{
		private IRedisClientsManager _clientManager;

		public static IRedisClientsManager ConfigClientManager
		{
			get;set;
		}


		public IRedisClientsManager ClientManager
		{
			get
			{
				return _clientManager;
			}
		}

		/// <summary>
		/// <code>
		///localhost
		///127.0.0.1:6379
		///redis://localhost:6379
		///password @localhost:6379
		///clientid:password @localhost:6379
		///redis://clientid:password@localhost:6380?ssl=true&db=1
		/// </code>
		/// </summary>
		protected string RedisHost = "localhost:6379";
		protected bool Pooled = false;
		protected string Prefix = "_NRSP_";

		private IRedisClientsManager CreateClientManager()
		{
			 
			if (Pooled == true)
			{
				return new PooledRedisClientManager(RedisHost);
			}
			else
			{
				return new BasicRedisClientManager(RedisHost);
			}
		}

		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		{
			if (ConfigClientManager == null)
			{
				if (!string.IsNullOrEmpty(config["host"]))
				{
					RedisHost = config["host"];
				}

				if (!string.IsNullOrWhiteSpace(config["pooled"]))
				{
					Pooled = Convert.ToBoolean("pooled");
				}

				_clientManager = CreateClientManager(); 

			}
			else {
				_clientManager = ConfigClientManager; 
			}

			base.Initialize(name, config);
		}
		public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
		{
			throw new NotImplementedException();
		}

		public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
		{
			throw new NotImplementedException();
		}

		public override void Dispose()
		{
			throw new NotImplementedException();
		}

		public override void EndRequest(HttpContext context)
		{
			throw new NotImplementedException();
		}

		public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			throw new NotImplementedException();
		}

		public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			throw new NotImplementedException();
		}

		public override void InitializeRequest(HttpContext context)
		{
			throw new NotImplementedException();
		}

		public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
		{
			throw new NotImplementedException();
		}

		public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
		{
			throw new NotImplementedException();
		}

		public override void ResetItemTimeout(HttpContext context, string id)
		{
			throw new NotImplementedException();
		}

		public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
		{
			throw new NotImplementedException();
		}

		public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
		{
			throw new NotImplementedException();
		}
	}
}

