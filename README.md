# SSLAB (Simple Service Locator and Boostrapper)

I have used ServiceLocator pattern in multiple projects from games to apps for Unity. Although it is considered anti-pattern I belieave that it is better fit than DI (Dependency Injection) in 90% of the times. SSLAB gives you a super simple way to abstract your different modules behind interfaces, to create a separation from Unity's APIs and to incorporate unit-testing if you so desire. It is the skeleton on top of which you can build anything you want. The whole package is less than 200 lines of code thus it is extremely easy to extend with whatever is needed for your project. **All merge requests are welcome! Feel free to fork it and do whatever you want with SSLAB! If you have any question (or you found a bug) you can raise an issue. Happy game developing! <3**

## Setup

Include this package in your project. Information how this is done can be found on [Unity docs](https://docs.unity3d.com/Manual/upm-ui-giturl.html). 
(TODO: Add info how to include specific versions in the future)

Create an `AppBootstrapper` script that inherits from `Bootstrapper`. Go to `Edit->ProjectSettings->Script Execution Order` and move `AppBootstrapper` above Default Time.

Override `Awake()` and instantiate all your non-MonoBehaviour serivces:
```C#
using SSLAB;

public class AppBootstrapper : Bootstrapper
{
    protected override void Awake()
    {
        base.Awake();

        ServiceLocator.Instance.RegisterService(new InventoryService());
        ServiceLocator.Instance.RegisterService(new PersistanceService());
    }
}
```

All service should inherit from `IService`. After registring with `ServiceLocator.RegisterService()` they can be accessed with their interface: `ServiceLocator.Instance.GetService<IInventoryService>()`. When registering service you can give them priority of initailization (which will also be the order of update). This will override the default order of insertion.

## Simple Service

Lets say there is another service somewhere with this interface :
```C#
//inventory item
public class Item() { }

public interface IPersistanceService
{
    void Persist(Item[] inventoryItems);
}
```

Lets see how a non-behaviour serivce would be implemented:

```C#
//I usually put the interface above the service in the same file
public interface IInventoryService: IService, IInitializable,ITickableUnscaled
{
    void AddItem(Item item);
    void RemoveItem(Item item);
    void Persist();
}

public class InventoryService : IInventoryService
{
    IPersistanceService _persistanceService;
    List<Item> _items;

    public void Init()
    {
        //get references to other services in Init() method, do not get services inside Update!
        //Init() method is called before Start() of normal scripts but after Awake()
        _persistanceService = ServiceLocator.Instance.GetService<IPersistanceService>();
        _items = new List<Item>();
    }

    public void AddItem(Item item)
    {
        _items.Add(item);
    }

    public void RemoveItem(Item item)
    {
        _items.Remove(item);
    }

    public void Persist()
    {
        _persistanceService.Persist(_items.ToArray());
    }

    public void TickUnscaled(float deltaTime)
    {
        //lets say that game is paused in inventory screen but we still need time for animation or some VFX
    }
}
```

## Mono Service

Lets take a look how a MonoBehaviour based service would be implemented:

```C#
public interface ICoroutineRunnerService : IService
{
    public Coroutine StartCoroutine(IEnumerator routine);
    public void StopCoroutine(Coroutine routine);
}
public class CoroutineRunnerService : MonoBehaviour, ICoroutineRunnerService
{
    void Awake()
    {
        //Register service in Awake()
        ServiceLocator.Instance.RegisterService(this);
    }
    void OnDestroy()
    {
        //Do not forget to clean up. Not needed if service is a singleton
        ServiceLocator.Instance.UnregisterService(this);
    }
}
```

When possible, use non-behaviour services to decrease dependency on Unity. I would recommend using `ITickable` rather than using `Update()` inside your MonoBehaviour services.

## SSLAB important types:

1. `ServiceLocator` which should be the only true singleton in your project. It is accessed with its singleton accessor `ServiceLocator.Instance`. 
    - `IService`: all services should inherit from this base intrface. It is an empty interface.
    - `IInitializable`: if you want your service to have an `Init()` method (equivalent to `MonoBehaviour.Start()`) it should inherit from this interface.
    - `IDestroyable`: if you want `OnDestroy()` equivalent.
    - `ITickable`: if you want `Update()` equivalent.
    - `ITickableUnscaled`: if you want `Update()` equivalent with unscalde delta time
    - `ITickableFixed`: if you want `FixedUpdate()` equivalent with fixed delta time
    - `IAppPause`: if you want to `OnApplicationPause()` equivalent (this is an example of typical Unity events bubbling to MonoBehaviours, you can easily reproduce this pattern and create whatever Unity event equivalent you need)

1. `Bootstrapper` which is the driver of all Unity related events and access to Unity APIs. It must be called before default time of scripts.

## Testing

If you write all your logic behind services then it is extremely easy to unit-test them. All you see is other services with interfaces that are easily mockable. Of course, you should not access Unity APIs straight but extend SSLAB with whatever you might need.