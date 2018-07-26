using Signals.Settings;

namespace GameWorld
{
    namespace Settings
    {
        public class BaseSettings
        {
            [Inject]
            public Changed ChangedSignal { get; private set; }

            private bool serializationEnabled;

            [PostConstruct]
            public void PostConstruct()
            {
                serializationEnabled = false;
                OnDeserialize();
                serializationEnabled = true;
            }

            protected virtual void OnDeserialize()
            {
            }

            protected virtual void OnSerialize()
            {
            }

            protected void ApplyChanges()
            {
                OnSerialize();
            }

            protected void OnSettingsChanged()
            {
                ChangedSignal.Dispatch();
                if (serializationEnabled)
                {
                    OnSerialize();
                }
            }
        }
    }
}