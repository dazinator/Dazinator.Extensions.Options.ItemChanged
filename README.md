## The problem

`Microsoft.Extensions.Options` provides `IOptionsMonitor<TOptions>` with which you can be notified of a new TOptions when configuration changes.

However:

1. The callback it fires doesn't tell you what the previous instance was so you can't do any diffing to work out what precisely has changed.
2. Options classes can have members that are lists / arrays of items. Suppose you want to quickly determine whether new items have been added, or existing ones removed or modified - there is no easy mechanism to do that.


## Solution

The solution is most likely to create your own "service" that caches the current Options instance, and then listens for changes, and then does your diffing logic between the cached instance and the new instance when they occur.
This is pretty much what this library provides, by way of a few services and utility classes - but this library only provides some very basic stuff right now.

### Get notified of old and new instance

Configure your options as normal and then register the following service:

```csharp
   services.AddOptionsChangedMonitor<TestOptions>();

```

You can now inject `IOptionsChangedMonitor<TestOptions>` and register a callback via its `OnChange` method - to be notified when your TOptions changes but also to be given the old instance, not just the new instance.
```csharp

  var itemMonitor = sp.GetRequiredService<IOptionsChangedMonitor<TestOptions>>(); // inject this
  itemMonitor.OnChange((changes) =>
  {                
      var old = changes.Old;

      var current = changes.Current;
      Assert.NotEqual(current, old);
  });


```

At the most basic level you can now do whatever diffing logic you need in this callback - I have only added a basic utility class for diffing arrays / lists at present.

Note: This project is very much in it's infancy and "Idea" stage, heavily subject to change and - may not even be that useful :-)

## Comparing Arrays

Suppose your `Options` class has a property that is an Array or List

```csharp
public class MyOptions
{
    public MyOptions()
    {

    }

    public List<Thing> Things { get; set; }
}

public class Thing 
{
        public string Key { get; set; }
}

```

When configuration changes, you want to identify what items are new, modified, or removed.

You can do this using a utility class called `CollectionDifferUsingKeyExpression` which will return
you an `IEnumerable<Difference>` when asked to compare two IEnumerable`s matching via a key:

```csharp

  var itemMonitor = sp.GetRequiredService<IOptionsChangedMonitor<MyOptions>>(); // inject this
  var collectionDiffer = new CollectionDifferUsingKeyExpression<Thing, string>(a => a.Key);

  itemMonitor.OnChange((changes) =>
  {                
      var old = changes.Old;
      var current = changes.Current;

      var differences = collectionDiffer.GetChanges(current.Items, old.Items).ToArray();    
      
      foreach (var difference in differences)
      {
         var currentItem = difference.CurrentItem;
         var oldItem = difference.OldItem;

         switch(item.ChangeType)
         {
             case ItemChangeType.Added:
                 ItemAdded(currentItem);
                 break;
             case ItemChangeType.Removed:
                 ItemRemoved(oldItem);
                 break;
             case ItemChangeType.Modified:
                 ItemModified(currentItem, oldItem);
                 break;
         }

      }

  });


```

It's called `CollectionDifferUsingKeyExpression` because you pass in an expression that selects the property to use for the "Key" to match with.
There is another called `CollectionDifferUsingInterface` which enforces instead that your item class implements an interface: `IHaveKey<TKey>` which forces a `.Key` property of that type.
The expression based approach is more flexible and doesn't require any type changes.

 `CollectionDifferUsingKeyExpression` and `CollectionDifferUsingInterface` are both `CollectionDiffer`'s and their responsibility is to compare two IEnumerable's and report on the differences in terms of New, Removed, or Modified items.

## IOptionsItemChangesMonitor

If you care about being notified of differences on an items / list / array property, you can call `AddOptionsItemChangeMonitor()` to register this as a service in its own right:


```csharp
 // pre-requisites.
 services.AddOptions();
 services.AddLogging();

 services.Configure<MyOptions>(config); // configure your options as normal.
 services.AddOptionsItemChangeMonitor<MyOptions, Thing, string>((o) => o.Key, (o) => o.Things);

```

and:

```csharp
public class MyService
{
   public MyService(IOptionsItemChangesMonitor<string, Thing> itemsMonitor)
   {
        itemsMonitor.OnChange((deltas) =>
        {
            // You still have access to the old and new TOptions instance here:
            var oldOptions = deltas.Old;
            var newOptions = delats.Current;

            // But now also the "differences" between the `Things` are available
            foreach (var item in deltas.Differences)
            {
               var currentItem = item.CurrentItem; // will be null if item removed.
               var oldItem = item.OldItem; // will be null if item added.

               switch(item.ChangeType)
               {
                   case ItemChangeType.Added:
                       DoSomethingWithNewItem(currentItem);
                       break;
                   case ItemChangeType.Removed:
                       DoSomeCleanupOnOldItem(oldItem);
                       break;
                   case ItemChangeType.Modified:
                       ComputeSomeChangesToItem(currentItem, oldItem);
                       break;
               }
            }            
        });
   }
}
```

The downside of doing it this way is this callback is only fired when there are actually some differences on the items to report.
So if you also need to respond to other property changes (not items) then this mechanism won't be for you.

If your `TOptions` class has multiple list / array properties of the same item type like this:

```csharp
public class MyOptions
{

    public List<Thing> Things { get; set; }

    public List<Thing> OtherThings { get; set; } 

}

```

You can still track changes for multiple lists / arrays of the same type with one service registration like so (In this case, `.Things` and `.OtherThings`):

```csharp
 services.AddOptionsItemChangeMonitor<MyOptions, Thing, string>((o) => o.Key,
        (o) => o.Things, (o) => o.OtherThings);

```

Then use the "MemberName" property when being notified of item changes:

```csharp
public class MyService
{
   public MyService(IOptionsItemChangesMonitor<string, Thing> itemsMonitor)
   {
        itemsMonitor.OnChange((deltas) =>
        {
            if(deltas.MemberName == nameof(MyOptions.Things))
            {
                ThingsChanged(deltas);
            }
            else if(deltas.MemberName == nameof(MyOptions.OtherThings))
            {
                OtherThingsChanged(deltas);
            }           
        });
   }
}
```

Note: this library is of fairly limited use at present, it doesn't currently work with "named" options, it serves a fairly niche scenario of my own, let me know if you have any suggestions or ideas for enhancements that you'd like to see.
