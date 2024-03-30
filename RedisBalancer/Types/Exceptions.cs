namespace RedisBalancer.Types
{
	public class RedisDbTransactionException : Exception
	{
		public RedisDbTransactionException() { }
		public RedisDbTransactionException(string message) : base(message) { }
		public RedisDbTransactionException(string message, Exception inner) : base(message, inner) { }
	}
}
