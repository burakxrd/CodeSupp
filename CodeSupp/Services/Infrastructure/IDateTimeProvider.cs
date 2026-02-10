using System;

namespace CodeSupp.Services.Infrastructure
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}