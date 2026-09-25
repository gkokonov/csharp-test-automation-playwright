using System.Globalization;
using System.Reflection;
using CsharpTestAutomation.Framework.Common.Utilities.CustomAttributes;
using FizzWare.NBuilder;

namespace CsharpTestAutomation.Framework.Common.Utilities;

/// <summary>
/// Abstract base class for test data builders backed by NBuilder.
/// <para>
/// Concrete builders inherit this class and implement <see cref="Default"/> to establish
/// sensible default property values. Callers then chain <see cref="With{TProperty}"/> overrides
/// for test-specific values and call <see cref="Build"/> to materialise the model.
/// </para>
/// <para>
/// <see cref="BuildFromDictionary"/> provides a dynamic mapping path: it applies the concrete
/// <see cref="Default"/> values and then overlays properties from a
/// <see cref="Dictionary{TKey,TValue}">Dictionary&lt;string, object&gt;</see>, matching by
/// property name (case-insensitive) or by any <see cref="PropertyAlias"/> attributes declared on
/// the property. Type mismatches are resolved automatically — enums are parsed by name, strings
/// are converted to <see cref="DateTime"/> via <see cref="System.Globalization.CultureInfo.InvariantCulture"/>,
/// and all other types fall back to <see cref="Convert.ChangeType(object, Type)"/>.
/// </para>
/// </summary>
/// <typeparam name="TModel">The model type this builder produces.</typeparam>
public abstract class BaseBuilder<TModel>
{
    private readonly ISingleObjectBuilder<TModel> _builder;

    protected BaseBuilder()
    {
        _builder = Builder<TModel>.CreateNew();
    }

    public abstract BaseBuilder<TModel> Default();

    public BaseBuilder<TModel> With<TProperty>(Func<TModel, TProperty> with)
    {
        _builder.With(with);

        return this;
    }

    public TModel Build()
    {
        return _builder.Build();
    }

    public TModel BuildFromDictionary(Dictionary<string, object> sourceMap)
    {
        TModel model = Default()._builder.Build();
        Type type = typeof(TModel);

        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanWrite)
                continue;

            IEnumerable<PropertyAlias> aliases = property.GetCustomAttributes<PropertyAlias>();
            foreach (KeyValuePair<string, object> kvp in sourceMap)
            {
                if (string.Equals(kvp.Key, property.Name, StringComparison.OrdinalIgnoreCase) || aliases != null && aliases!.Any(x => x.Alias.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    var value = kvp.Value;
                    if (value != null && property.PropertyType != value.GetType())
                    {
                        Type targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                        if (targetType.IsEnum)
                        {
                            value = Enum.Parse(targetType, (value as string)!, true);
                        }
                        else if (targetType == typeof(DateTime) && value is string dateString)
                        {
                            value = DateTime.Parse(dateString, CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            value = Convert.ChangeType(value, targetType);
                        }
                    }
                    property.SetValue(model, value);

                    break;
                }
            }
        }

        return model;
    }
}
