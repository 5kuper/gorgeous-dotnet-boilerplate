using System.Reflection;

namespace Shared.TestKit.TestData;

public static class TestEntityIds
{
    public static TEntity SetId<TEntity>(this TEntity entity, long id)
    {
        var property = entity!
            .GetType()
            .GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Type '{typeof(TEntity).Name}' does not expose an Id property.");

        property.SetValue(entity, id);

        return entity;
    }
}
