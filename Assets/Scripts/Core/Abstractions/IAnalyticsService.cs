using System.Collections.Generic;

namespace IdlePancake.Core.Abstractions
{
    public interface IAnalyticsService
    {
        void TrackEvent(string eventName, IReadOnlyDictionary<string, object>? payload = null);
    }
}
