#nullable enable
using System;

namespace AQ.SharedKernel
{
    public interface ITimeProvider { DateTime UtcNow { get; } }

    public sealed class FixedTimeProvider : ITimeProvider
    {
        public DateTime UtcNow { get; private set; }
        public FixedTimeProvider(DateTime utcNow) => Set(utcNow);
        public void Set(DateTime utcNow) =>
            UtcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
    }
}
