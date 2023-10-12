namespace SSLAB
{
    using UnityEngine;

    public class Bootstrapper : MonoBehaviour
    {
        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
            ServiceLocator.Instance.InitServices();
        }

        void OnDestroy()
        {
            ServiceLocator.Instance.DestroyServices();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            ServiceLocator.Instance.OnAppPauseServices(pauseStatus);
        }

        void Update()
        {
            ServiceLocator.Instance.TickServices(Time.deltaTime, Time.unscaledDeltaTime);
        }

        void FixedUpdate()
        {
            ServiceLocator.Instance.TickFixedServices(Time.fixedDeltaTime);
        }
    }
}