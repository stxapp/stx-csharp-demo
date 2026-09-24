using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using STX.Sdk.Helpers;

namespace STX.Sdk.Api.Services
{
    /// <summary>
    /// Makes every price, amount, balance, fee, P&amp;L and quantity in a response a string,
    /// so this API returns <c>"price": "0.5600"</c> and <c>"quantity": "2.00"</c> rather than
    /// cents and bare numbers.
    /// </summary>
    /// <remarks>
    /// The SDK data types keep the raw value (cents for money) and add a read-only
    /// <c>&lt;Field&gt;String</c> companion beside it: <c>PriceString</c>, <c>QuantityString</c>,
    /// <c>AccountBalanceString</c> and so on. The companions are marked not to serialise, so
    /// returning an SDK object as it is would still send cents. This modifier writes each field
    /// with its companion's value, under the field's own name, which lets the controllers return
    /// SDK objects unchanged.
    ///
    /// A companion is found by name: <c>Price</c> uses <c>PriceString</c>, <c>PriceCents</c>
    /// uses <c>PriceString</c>, and a price rule's <c>From</c> uses <c>FromPriceString</c>.
    /// The few money fields with no companion are formatted with
    /// <see cref="STXWireFormat.DollarString(int?)"/>, the same helper the companions use.
    /// </remarks>
    public static class AmountStringsJsonModifier
    {
        // Money fields the SDK has no companion for. They are cents, like the rest.
        private static readonly HashSet<string> CentsWithoutCompanion = new() { "PriceChange24h" };

        public static void Apply(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Kind != JsonTypeInfoKind.Object)
            {
                return;
            }

            for (var i = 0; i < typeInfo.Properties.Count; i++)
            {
                var property = typeInfo.Properties[i];

                if (property.AttributeProvider is not PropertyInfo raw || !IsNumber(raw.PropertyType))
                {
                    continue;
                }

                Func<object, object> read = CompanionReader(typeInfo.Type, raw);
                if (read is null)
                {
                    continue;
                }

                // Same name and position in the output, string value.
                var replacement = typeInfo.CreateJsonPropertyInfo(typeof(string), property.Name);
                replacement.Get = read;
                replacement.Order = property.Order;
                typeInfo.Properties[i] = replacement;
            }
        }

        private static Func<object, object> CompanionReader(Type type, PropertyInfo raw)
        {
            // The companion the SDK (or the model) declares, by its naming convention.
            var baseName = raw.Name.EndsWith("Cents") ? raw.Name[..^"Cents".Length] : raw.Name;
            var companion = type.GetProperty(baseName + "String") ?? type.GetProperty(baseName + "PriceString");

            if (companion is not null && companion.PropertyType == typeof(string))
            {
                return obj => companion.GetValue(obj);
            }

            if (CentsWithoutCompanion.Contains(raw.Name))
            {
                return obj => raw.GetValue(obj) is { } cents ? STXWireFormat.DollarString(Convert.ToDecimal(cents)) : null;
            }

            return null;
        }

        private static bool IsNumber(Type type)
        {
            var t = Nullable.GetUnderlyingType(type) ?? type;
            return t == typeof(int) || t == typeof(long) || t == typeof(decimal) || t == typeof(float) || t == typeof(double);
        }
    }
}
