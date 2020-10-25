namespace NBattleshipCodingContest.Protocol
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Concurrent;

    public class PlayerHosts
    {
        private readonly ConcurrentBag<IPlayerHostConnection> freeConnections = new ();
        private readonly IServiceProvider services;
        private readonly ILogger<PlayerHosts> logger;

        public PlayerHosts(IServiceProvider services, ILogger<PlayerHosts> logger)
        {
            this.services = services;
            this.logger = logger;
        }

        public void Add(IPlayerHostConnection connection)
        {
            freeConnections.Add(connection);
        }

        internal void Free(IPlayerHostConnection connection)
        {
        }
    }
}
