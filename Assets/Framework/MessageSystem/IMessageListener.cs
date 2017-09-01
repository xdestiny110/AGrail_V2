namespace Framework.Message
{
    public interface IMessageListener
    {
        void OnEventTrigger(string eventType, params object[] parameters);
    }
}


