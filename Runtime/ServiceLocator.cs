namespace SSLAB
{
    using System;
    using System.Collections.Generic;

    public interface IService
    {
    }

    public interface IInitializable
    {
        void Init();
    }
    public interface IDestroyable
    {
        void Destroy();
    }
    public interface IAppPause
    {
        void OnAppPause(bool pause);
    }
    public interface ITickable
    {
        void Tick(float deltaTime);
    }
    public interface ITickableUnscaled
    {
        void TickUnscaled(float deltaTime);
    }
    public interface ITickableFixed
    {
        void TickFixed(float fixedDeltaTime);
    }

    public class ServiceLocator : Singleton<ServiceLocator>
    {
        List<Tuple<int, IService>> _services = new List<Tuple<int, IService>>();

        public void RegisterService(IService service, int priority = 0)
        {
            for (int i = 0; i < _services.Count; i++)
            {
                var tuple = _services[i];
                if (tuple.Item2 == service)
                {
                    throw new ArgumentException(service.GetType().Name + " already registered! Unregister previous instance!");
                }
            }

            _services.Add(new Tuple<int, IService>(priority, service));
        }

        public void UnregisterService(IService service)
        {
            for (int i = 0; i < _services.Count; i++)
            {
                if (_services[i].Item2 == service)
                {
                    _services.RemoveAt(i);
                    return;
                }
            }
        }

        public void UnregisterService<T>() where T : IService
        {
            _services.RemoveAll(s => s.Item2 is T);
        }

        public T GetService<T>()
        {
            for (int i = 0; i < _services.Count; i++)
            {
                if (typeof(T).IsAssignableFrom(_services[i].Item2.GetType()))
                {
                    return (T)_services[i].Item2;
                }
            }

            return default(T);
        }

        public void InitServices()
        {
            _services.Sort((a, b) => a.Item1 - b.Item1);

            foreach (var tuple in _services)
            {
                var service = tuple.Item2 as IInitializable;
                if (service != null)
                {
                    service.Init();
                }
            }
        }

        public void DestroyServices()
        {
            foreach (var tuple in _services)
            {
                var service = tuple.Item2 as IDestroyable;
                if (service != null)
                {
                    service.Destroy();
                }
            }
            _services.Clear();
        }

        public void OnAppPauseServices(bool pause)
        {
            foreach (var tuple in _services)
            {
                var service = tuple.Item2 as IAppPause;
                if (service != null)
                {
                    service.OnAppPause(pause);
                }
            }
        }

        public void TickServices(float deltaTime, float unscaledDeltaTime)
        {
            for (int i = 0; i < _services.Count; i++)
            {
                var service = _services[i].Item2;
                var scaled = service as ITickable;
                if (scaled != null)
                {
                    scaled.Tick(deltaTime);
                }
                var unscaled = service as ITickableUnscaled;
                if (unscaled != null)
                {
                    unscaled.TickUnscaled(unscaledDeltaTime);
                }
            }
        }

        public void TickFixedServices(float fixedDeltaTime)
        {
            for (int i = 0; i < _services.Count; i++)
            {
                var fixedTickable = _services[i].Item2 as ITickableFixed;
                if (fixedTickable != null)
                {
                    fixedTickable.TickFixed(fixedDeltaTime);
                }
            }
        }
    }
}