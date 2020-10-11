namespace Dazinator.Extensions.Options.ItemChanged.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;
    using Xunit;

    public class OptionsItemChangesMonitorTests
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
        public void Can_Get_Notified_Of_Deltas()
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

            int callCount = 0;
            services.Configure<TestOptions>((a) =>
            {
                if (!tokenSources.Any(c => c.IsCancellationRequested))
                {
                    a.Items.Add(new TestItemOptions() { Key = "A" });
                    callCount = callCount + 1;
                    return;
                }
                else
                {
                    a.Items.Add(new TestItemOptions() { Key = "B" });
                    callCount = callCount + 1;
                    return;
                }
            });

            services.AddOptionsItemChangeMonitor<TestOptions, TestItemOptions, string>((o) => o.Items);


            var sp = services.BuildServiceProvider();
            var itemMonitor = sp.GetRequiredService<IOptionsItemChangesMonitor<string, TestItemOptions>>();

            var autoEvent = new AutoResetEvent(false);

            itemMonitor.OnChange((changes) =>
            {
                Assert.Equal("Items", changes.MemberName);
                var added = changes.Changes[ItemChangeType.Added];
                Assert.Single(added);
                var addedItem = added.First();
                Assert.Equal("B", addedItem.Key);

                var removed = changes.Changes[ItemChangeType.Removed];
                Assert.Single(removed);
                var removedItem = removed.First();
                Assert.Equal("A", removedItem.Key);

                var updated = changes.Changes[ItemChangeType.Modified];
                Assert.Empty(updated);

                autoEvent.Set();
            });

            var cts = tokenSources.First();
            cts.Cancel();

            autoEvent.WaitOne(10000);

        }

        [Fact]
        public void Can_Get_Notified_Of_Deltas_Multiple_Lists()
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


            var items = new List<TestItemOptions>
            {
                new TestItemOptions() { Key = "A" }
            };
            var otherItems = new List<TestItemOptions>();
            // otherItems.Add(new TestItemOptions() { Key = "A" });

            services.Configure<TestOptions>((a) =>
            {
                if (items.Any())
                {
                    a.Items.AddRange(items);
                }
                if (otherItems.Any())
                {
                    a.OtherItems.AddRange(otherItems);
                }
            });

            services.AddOptionsItemChangeMonitor<TestOptions, TestItemOptions, string>(
                (o) => o.Items,
                (o) => o.OtherItems);

            var sp = services.BuildServiceProvider();
            var itemMonitor = sp.GetRequiredService<IOptionsItemChangesMonitor<string, TestItemOptions>>();

        //    var autoEvent = new AutoResetEvent(false);
            var autoEventAddedA = new AutoResetEvent(false);
            var autoEventAddedOtherItem = new AutoResetEvent(false);

            var callCount = 0;
            itemMonitor.OnChange((changes) =>
            {
                var called = callCount;
                if (called == 0)
                {
                    Assert.Equal(nameof(TestOptions.Items), changes.MemberName);

                    callCount = 1;
                    var added = changes.Changes[ItemChangeType.Added];
                    Assert.Single(added);

                    var removed = changes.Changes[ItemChangeType.Removed];
                    Assert.Empty(removed);

                    var updated = changes.Changes[ItemChangeType.Modified];
                    Assert.Empty(updated);

                    autoEventAddedA.Set();

                }               
                else if (called == 1)
                {
                    Assert.Equal(nameof(TestOptions.OtherItems), changes.MemberName);
                    // Assert we added new item to OtherItems.
                    var added = changes.Changes[ItemChangeType.Added];                  
                    Assert.Single(added);
                    autoEventAddedOtherItem.Set();
                }
                // callCount = called + 1;
            });

            var cts = tokenSources.First();
            // add an item key b
            items.Add(new TestItemOptions() { Key = "B" });
            cts.Cancel();
            Assert.True(autoEventAddedA.WaitOne(10000));
            //Assert.True(autoEvent.WaitOne(0));

            // autoEvent.Reset();
            // callCount = 1;
            var anotherCts = tokenSources[1];
            // Add other item
            otherItems.Add(new TestItemOptions() { Key = "B" });
            anotherCts.Cancel();
            Assert.True(autoEventAddedOtherItem.WaitOne(10000));
            //var waitHandleArray = new WaitHandle[] { autoEventAddedA, autoEventAddedOtherItem };
            //Assert.True(WaitHandle.WaitAll(waitHandleArray, 10000));



        }

    }
}
