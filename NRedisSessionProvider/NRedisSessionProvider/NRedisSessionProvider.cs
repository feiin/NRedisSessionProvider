using System;
using System.Web;
using System.Web.SessionState;
using System.Configuration;
using System.Configuration.Provider;
using NServiceKit.Redis;
using NRedisSessionProvider;
using System.Web.Configuration;
using System.IO;

namespace NRedisProvider
{
	public class NRedisSessionProvider : SessionStateStoreProviderBase
	{
		private IRedisClientsManager _clientManager;

		public static IRedisClientsManager ConfigClientManager
		{
			get;set;
		}

		protected IRedisClientsManager ClientManager
		{
			get
			{
				return _clientManager;
			}
		}


		protected IRedisClient RedisClient
		{
			get
			{
				if (ClientManager != null)
				{
					return ClientManager.GetClient();
				}
				return null;
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
		protected SessionStateSection sessionStateSection = null;

		protected int Timeout {
			get
			{
				if (sessionStateSection != null)
				{
					return Convert.ToInt32(sessionStateSection.Timeout.TotalMinutes);
				}
				return 30;
			}
		}

		/// <summary>
		/// Gets the redis key for session id.
		/// </summary>
		/// <returns>The redis key.</returns>
		/// <param name="id">the session id.</param>
		private string GetRedisKey(string id)
		{
			return string.Format("{0}{1}", !string.IsNullOrEmpty(this.Prefix) ? this.Prefix + ":" : "", id);
		}

		/// <summary>
		/// Creates the client manager.
		/// </summary>
		/// <returns>The client manager.</returns>
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

		/// <summary>
		/// Inits the redis config.
		/// </summary>
		/// <returns>The redis config.</returns>
		/// <param name="host">host see href="https://github.com/feiin/NRedisSessionProvider"</param>
		/// <param name="pooled">Pooled.</param>
		public static void InitRedisConfig(string host,bool pooled)
		{
			 
			if (pooled == true)
			{
			     ConfigClientManager = new PooledRedisClientManager(host);
			}
			else
			{
				 ConfigClientManager = new BasicRedisClientManager(host);
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
					Pooled = Convert.ToBoolean(config["pooled"]);
				}

				_clientManager = CreateClientManager(); 

			}
			else {
				_clientManager = ConfigClientManager; 
			}

			sessionStateSection = (SessionStateSection)ConfigurationManager.GetSection("system.web/sessionState");
			base.Initialize(name, config);

		}


		public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
		{
			return new SessionStateStoreData(new SessionStateItemCollection(),
			   SessionStateUtility.GetSessionStaticObjects(context),
			   timeout);
		}

		public SessionStateStoreData CreateNewStoreData(HttpContext context, SessionStateItemCollection items, int timeout)
		{
			return new SessionStateStoreData(items,
			   SessionStateUtility.GetSessionStaticObjects(context),
			   timeout);
		}

		/// <summary>
		/// Creates the uninitialized item.
		/// </summary>
		/// <returns>The uninitialized item.</returns>
		/// <param name="context">Context.</param>
		/// <param name="id">Identifier.</param>
		/// <param name="timeout">Timeout.</param>
		public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return;
			}

			using (var client = this.RedisClient)
			{
				var sessionItem = new SessionItem();
				sessionItem.CreatedAt = DateTime.Now.ToUniversalTime();
				sessionItem.LockAt = DateTime.Now.ToUniversalTime();
				sessionItem.LockID = 0;
				sessionItem.Timeout = timeout;
				sessionItem.Locked = false;
				sessionItem.Data = SessionSerializer.Serialize(new SessionStateItemCollection());
				sessionItem.Flags = 0;

				client.Set<SessionItem>(this.GetRedisKey(id), sessionItem, DateTime.UtcNow.AddMinutes(timeout));
			}
		}

		public override void Dispose()
		{
			//throw new NotImplementedException();
		}

		public override void EndRequest(HttpContext context)
		{
			//throw new NotImplementedException();
		}

		public SessionStateStoreData GetSessionItem(bool lockSession, HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
		{

			SessionStateStoreData item = null;
			lockAge = TimeSpan.Zero;
			lockId = null;
			locked = false;
			actionFlags = 0;
			if (string.IsNullOrWhiteSpace(id))
			{
				return null;
			}


			var sessionKey = this.GetRedisKey(id);

			using (var client = this.RedisClient)
			{
				var currentItem = client.Get<SessionItem>(sessionKey);
				if (lockSession)
				{
					locked = false;
					if (currentItem != null)
					{
						if (!currentItem.Locked)
						{
							currentItem.Locked = true;
							currentItem.LockAt = DateTime.UtcNow;
							client.Set<SessionItem>(sessionKey, currentItem, DateTime.UtcNow.AddMinutes(sessionStateSection.Timeout.TotalMinutes));
						}
						else {
							locked = true;
						}
					}
				}

				if (currentItem != null)
				{
					lockId = currentItem.LockID;
					lockAge = DateTime.UtcNow.Subtract(currentItem.LockAt);
					actionFlags = (SessionStateActions)currentItem.Flags;
				}

				if (currentItem != null && !locked)
				{
					lockId = (int?)lockId + 1;
					currentItem.LockID = lockId != null ? (int)lockId : 0;
					currentItem.Flags = 0;

					client.Set<SessionItem>(sessionKey, currentItem, DateTime.UtcNow.AddMinutes(this.Timeout));
 
					if (actionFlags == SessionStateActions.InitializeItem)
					{
						item = CreateNewStoreData(context, this.Timeout);
					}
					else {
						item = CreateNewStoreData(context,SessionSerializer.Deserialize(currentItem.Data),this.Timeout);
					}
				}
			}

			return item;
		}


		public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			return GetSessionItem(false, context, id, out locked, out lockAge, out lockId, out actions);
 		}

		public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			return GetSessionItem(true, context, id, out locked, out lockAge, out lockId, out actions);
		}

		public override void InitializeRequest(HttpContext context)
		{
			//throw new NotImplementedException();
		}

		public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
		{
			var sessionKey = this.GetRedisKey(id);
			using (var client = this.RedisClient)
			{
				var sessionItem = client.Get<SessionItem>(sessionKey);

				if (sessionItem != null && (int?)lockId == sessionItem.LockID)
				{
					sessionItem.Locked = false;
					client.Set<SessionItem>(sessionKey,sessionItem, DateTime.UtcNow.AddMinutes(this.Timeout));
				}
			}
 		}

		public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
		{
			var sessionKey = this.GetRedisKey(id);
			using (var client = this.RedisClient)
			{

				client.Remove(sessionKey);
			}
		}

		public override void ResetItemTimeout(HttpContext context, string id)
		{
			var sessionKey = this.GetRedisKey(id);
			using (var client = this.RedisClient)
			{
				
				client.ExpireEntryAt(sessionKey, DateTime.UtcNow.AddMinutes(this.Timeout));
			}
		}

		public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
		{
			using (var client = this.RedisClient)
			{
				var sessionItems = item.Items as SessionStateItemCollection;

				var sessionKey = this.GetRedisKey(id);
				if (newItem)
				{
					var sessionItem = new SessionItem();
					sessionItem.CreatedAt = DateTime.UtcNow;
					sessionItem.LockAt = DateTime.UtcNow;
					sessionItem.LockID = 0;
					sessionItem.Timeout = item.Timeout;
					sessionItem.Locked = false;
					sessionItem.Data = SessionSerializer.Serialize(sessionItems);
					sessionItem.Flags = 0;
					client.Set<SessionItem>(sessionKey, sessionItem, DateTime.UtcNow.AddMinutes(item.Timeout));
				}
				else {
					var currentSessionItem = client.Get<SessionItem>(sessionKey);
					if (currentSessionItem != null && currentSessionItem.LockID == (int?)lockId)
					{
						currentSessionItem.Locked = false;
						currentSessionItem.Data = SessionSerializer.Serialize(sessionItems);
						client.Set<SessionItem>(sessionKey, currentSessionItem, DateTime.UtcNow.AddMinutes(item.Timeout));
					}
				}
			}
		}

		public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
		{
			return false;
		}


	}
}

