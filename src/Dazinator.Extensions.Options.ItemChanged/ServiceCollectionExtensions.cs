namespace Dazinator.Extensions.Options.ItemChanged
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Dazinator.Extensions.Options.ItemChanged.Tests;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOptionsItemChangeMonitor<TOptions, TOptionsItem, TKey>(this IServiceCollection services, Func<TOptionsItem, TKey> keySelector, params Expression<Func<TOptions, IEnumerable<TOptionsItem>>>[] itemsAccessors)
    where TOptionsItem : class, IHaveKey<TKey>
        {          
            services.AddSingleton<IOptionsItemsChangedMonitor<TKey, TOptionsItem>>((sp) =>
            {
                var itemsDiffer = new CollectionDifferUsingKeyExpression<TOptionsItem, TOptionsItem, TKey>(keySelector, keySelector);
                var accessors = itemsAccessors.Select((exp) => CreateItemsAccessor<TOptions, TOptionsItem, TKey>(exp));
                var instance = ActivatorUtilities.CreateInstance<OptionsItemsChangedMonitor<TKey, TOptions, TOptionsItem>>(sp, itemsDiffer, accessors);
                return instance;
            });

            return services;
        }

        private static ItemsMemberAccessor<TOptions, TOptionsItem> CreateItemsAccessor<TOptions, TOptionsItem, TKey>(Expression<Func<TOptions, IEnumerable<TOptionsItem>>> expression)
            where TOptionsItem : IHaveKey<TKey>
        {
            var body = expression.Body;
            var member = body as MemberExpression;
            var memberName = member.Member.Name;
            return new ItemsMemberAccessor<TOptions, TOptionsItem>() { GetItems = expression.Compile(), MemberName = memberName };
        }
    }
}

