namespace Dazinator.Extensions.Options.ItemChanged
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOptionsItemChangeMonitor<TOptions, TOptionsItem, TKey>(this IServiceCollection services, params Expression<Func<TOptions, IEnumerable<TOptionsItem>>>[] itemsAccessors)
    where TOptionsItem : class, IHaveKey<TKey>
        {

            services.AddSingleton<IOptionsItemChangesMonitor<TKey, TOptionsItem>>((sp) =>
            {
                var accessors = itemsAccessors.Select((exp) => CreateItemsAccessor<TOptions, TOptionsItem, TKey>(exp));
                var instance = ActivatorUtilities.CreateInstance<OptionsItemChangesMonitor<TKey, TOptions, TOptionsItem>>(sp, accessors);
                return instance;
            });

            return services;
        }

        private static KeyedItemsAccessor<TOptions, TOptionsItem, TKey> CreateItemsAccessor<TOptions, TOptionsItem, TKey>(Expression<Func<TOptions, IEnumerable<TOptionsItem>>> expression)
            where TOptionsItem : IHaveKey<TKey>
        {
            var body = expression.Body;
            var member = body as MemberExpression;
            var memberName = member.Member.Name;
            return new KeyedItemsAccessor<TOptions, TOptionsItem, TKey>() { GetItems = expression.Compile(), MemberName = memberName };
        }
    }
}

