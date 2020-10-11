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
 
 If your Options instance has a "List" (or Array) of items, and you only care to know what the delta's are between the old and the new version then you can use this library - which basically does the above.

Example:

```csharp
public class MyOptions
{

    public List<Thing> Things { get; set; } // You want to be notified of the delta's'
}

pubic class Thing : IHaveKey<string> // Your items must have a "Key" property - you are free to choose the type for the key.
{    
  public string Key { get; set; }
}

```

Then in startup:

```csharp
 // pre-requisites.
 services.AddOptions();
 services.AddLogging();

 services.Configure<MyOptions>(config);
 services.AddOptionsItemChangesMonitor<MyOptions, Thing, string>((o) => o.Things);

```

Note: the third generic type argument `string` in the example above, is the type of the `Key` property that exists on each item. In this case we used a `string`.
Now you can now inject an "items level" monitor to listen for delta's in this list:

```csharp
public class MyService
{
   public MyService(IOptionsItemChangesMonitor<string, Thing> itemsMonitor)
   {
        itemsMonitor.OnChange((deltas) =>
        {
            // added is a list of `Thing`'s that were present in the new Items list compared to the old one, based on new keys being found.
            var added = deltas.Changes[ItemChangeType.Added];
            
            // is a list of `Thing`'s that were present in the old Items list but not in the new list, based on old keys no longer found.
            var removed = deltas.Changes[ItemChangeType.Removed];

            // is a list of `Thing`'s that were present in both old and new items list (based on key matches in both lists) but equality comparison failed between the matching items.
            // You can override Equals() on your Thing() class to take control over this.
            var updated = deltas.Changes[ItemChangeType.Modified];               

        });
   }
}
```

Suppose your options class has multiple list properties of the same type.
You want to track all of them.


```csharp
public class MyOptions
{

    public List<Thing> Things { get; set; } // You want to be notified of the delta's'

    public List<Thing> OtherThings { get; set; } // You want to be notified of the delta's'

}

```

You can listen to changes in muliple list / array properties of the same item type like so:

```csharp
 services.AddOptionsItemChangesMonitor<MyOptions, Thing, string>(
    (o) => o.Things, (o) => o.OtherThings);

```

Here is another example where you have multiple list properties of the same type, and another of a different type:

```
public class MyOptions
{

    public List<Thing> Things { get; set; } 

    public List<Thing> OtherThings { get; set; } 

    public List<DifferentThing> DifferentThings { get; set; }

}
```

You'd register like:

```
 services.AddOptionsItemChangesMonitor<MyOptions, Thing, string>((o) => o.Things, (o) => o.OtherThings);
 services.AddOptionsItemChangesMonitor<MyOptions, DifferentThing, string>((o) => o.DifferentThings);

```

To distinguish between which list changed when listening to multiple lists of the same type, 
check "MemberName" argument:

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
