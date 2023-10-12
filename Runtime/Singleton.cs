namespace SSLAB
{
    public class Singleton<T> where T : class, new()
    {
        static T sInstance;

        public static T Instance
        {
            get
            {
                if (sInstance == null)
                {
                    sInstance = new T();
                }

                return sInstance;
            }
        }
    }
}