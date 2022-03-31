using StackExchange.Redis;

namespace MvcBankingApplication.Utils
{
    public class RedisConnectorHelper
    {
        private static Lazy<ConnectionMultiplexer> LazyConnection;

        static RedisConnectorHelper()
        {
            LazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect("localhost");
            });
        }

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return LazyConnection.Value;
            }
        }
    }
}
