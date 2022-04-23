using UnityEngine.Events;

namespace BarkClaw {
    public enum InterfaceEvents
    {
        RESUME,
        PAUSE,
        DEATH
    }

    public class UserInterfaceEvent : UnityEvent<InterfaceEvents>
    {

    }
}
