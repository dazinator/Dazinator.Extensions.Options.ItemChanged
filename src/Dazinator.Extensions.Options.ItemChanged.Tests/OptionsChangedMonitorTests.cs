namespace Dazinator.Extensions.Options.ItemChanged.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;
    using Xunit;

    public class OptionsChangedMonitorTests
    {

        public class TestOptionsChangeTokenSource : IOptionsChangeTokenSource<TestOptions>
        {
            private readonly Func<IChangeToken> _getChangeToken;

            public string Name => string.Empty;

            public TestOptionsChangeTokenSource(Func<IChangeToken> getChangeToken)
            {
                _getChangeToken = getChangeToken;
            }
            public IChangeToken GetChangeToken()
            {
                return _getChangeToken();
            }
        }

        public class TestChangeToken : IChangeToken
        {

            public bool HasChanged { get; }

            public bool ActiveChangeCallbacks { get; } = true;

            public IDisposable RegisterChangeCallback(Action<object> callback, object state)
            {
                throw new NotImplementedException();
            }

            public void FireChanged()
            {

            }
        }
        [Fact]
        public void Can_Get_Notified_Of_ChangedOptions()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging();


            var tokenSources = new List<CancellationTokenSource>();
            // var cancellationToken = cancellationTokenSource.Token;
            // var changeToken =

            var changeTokenSource = new TestOptionsChangeTokenSource(() =>
            {
                var tokenSource = new CancellationTokenSource();
                tokenSources.Add(tokenSource);
                var changeToken = new CancellationChangeToken(tokenSource.Token);
                return changeToken;
            });

            services.AddSingleton<IOptionsChangeTokenSource<TestOptions>>(changeTokenSource);

            services.Configure<TestOptions>((a) =>
            {
                if (!tokenSources.Any(c => c.IsCancellationRequested))
                {
                    a.Items.Add(new TestItemOptions() { Key = "A" });
                    return;
                }
                else
                {
                    a.Items.Add(new TestItemOptions() { Key = "B" });
                    return;
                }
            });

            services.AddOptionsChangedMonitor<TestOptions>();


            var sp = services.BuildServiceProvider();
            var itemMonitor = sp.GetRequiredService<IOptionsChangedMonitor<TestOptions>>();

            var autoEvent = new AutoResetEvent(false);

            itemMonitor.OnChange((changes) =>
            {                
                var old = changes.Old;
                Assert.Equal("A", old.Items[0].Key);

                var current = changes.Current;
                Assert.Equal("B", current.Items[0].Key);

                //var collectionDiffer = new CollectionDifferUsingKeyExpression<TestItemOptions, string>(a => a.Key);
                //var itemsChanges = collectionDiffer.GetChanges(current.Items, old.Items).ToArray();                

                autoEvent.Set();
            });

            var cts = tokenSources.First();
            cts.Cancel();

            autoEvent.WaitOne(10000);

        }



    }

}
