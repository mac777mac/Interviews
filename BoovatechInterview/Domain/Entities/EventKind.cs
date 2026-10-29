using System.Collections.Frozen;

namespace Boovatech.Domain.Entities
{
    public class EventKind
    {
        public const string Fund = "fund";
        public const string Dispatch = "dispatch";
        public const string Load = "load";
        public const string Dispense = "dispense";
        public const string CountAdjust = "count_adjust";

        public static IReadOnlySet<string> All { get; } =
            (new[] { Fund, Dispatch, Load, Dispense, CountAdjust }).ToFrozenSet(
StringComparer.OrdinalIgnoreCase);
    }
}
