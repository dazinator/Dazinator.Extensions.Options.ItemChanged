## The problem

`Microsoft.Extensions.Options` let's you configure an options class.
This could have a collection of items:

```csharp
public class MyOptions
{
    public MyOptions()
    {

    }

    public List<Thing> Things { get; set; }
}

```

You can then use `IOptionsMonitor<MyOptions>` to listen for new instances of `MyOptions` when there is some change at runtime (like a configuration change).

However suppose when the configuration changes, you want to know which `Thing` in the `Things` list was changed since the previous list? 
 - Are there new Items present that weren't present before?
 - Are there existing items that were present before but have now changed?
 - Are there items that were present before but have now been removed?

 You can achieve this by creating your own service that caches your current Options instance, and then whenever you are passed a changed instance from `IOptionsMonitor` - do your own `diffing` mechanism to work out
 what the differences are between the old item and the new item and then take some actions based on those differences.
 

 ## One Solution

 If your Options instance has a "List" (or Array) of items, and you only care to know what the delta's are between the old items list and the new list, then you can use this library - which basically compares the two lists, comparing "Key" properties on the items to ascertain which ones have been added, removed, or modified.

Example:

```csharp
public class MyOptions
{
    public List<Thing> Things { get; set; } // You want to be notified of the delta's'
}

pubic class Thing 
{    
  public string Key { get; set; }
}

```

Then in startup:

```csharp
 // pre-requisites.
 services.AddOptions();
 services.AddLogging();

 services.Configure<MyOptions>(config); // configure your options as normal.
 services.AddOptionsItemChangeMonitor<MyOptions, Thing, string>((o) => o.Key, (o) => o.Things);

```

Now you can now inject an "items level" monitor to be notified of delta's in the list:

```csharp
public class MyService
{
   public MyService(IOptionsItemChangesMonitor<string, Thing> itemsMonitor)
   {
        itemsMonitor.OnChange((deltas) =>
        {
            foreach (var item in deltas.Differences)
            {
               var currentItem = item.CurrentItem;
               var oldItem = item.OldItem;

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

Suppose your options class has multiple lists:


```csharp
public class MyOptions
{

    public List<Thing> Things { get; set; }

    public List<Thing> OtherThings { get; set; } 

}

```

You can track multiple list members of the same type (In this case, `.Things` and `.OtherThings`):

```csharp
 services.AddOptionsItemChangeMonitor<MyOptions, Thing, string>((o) => o.Key,
        (o) => o.Things, (o) => o.OtherThings);

```

Then check the "MemberName" property when being notified of changes
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
