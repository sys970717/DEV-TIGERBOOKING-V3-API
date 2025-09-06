// In-memory connection multiplexer removed. Use TigerBooking.Infrastructure.Services.Redis.InMemoryRedisClient instead.
// This file intentionally left minimal to avoid implementing StackExchange.Redis internals.
namespace TigerBooking.Api.Infrastructure.InMemory
{
    internal static class InMemoryMarker
    {
        public const string Message = "InMemoryConnectionMultiplexer removed: use IRedisClient implementations instead.";
    }
}
