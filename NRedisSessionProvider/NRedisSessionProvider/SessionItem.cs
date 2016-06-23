using System;
using System.Web.SessionState;

namespace NRedisSessionProvider
{
	public class SessionItem
	{
		public DateTime CreatedAt { get; set; }
		public DateTime LockAt { get; set; }
		public int LockID { get; set; }
		public int Timeout { get; set; }
		public bool Locked { get; set; }
		public int Flags { get; set; }
		public byte[] Data { get; set; }

	}
}

