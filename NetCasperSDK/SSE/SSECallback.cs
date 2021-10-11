namespace NetCasperSDK.SSE
{
    public delegate void EventCallback(EventType eventType, int id, string payload);

    internal class SSECallback
    {
        public EventType EventType { get; }
        public string Name { get; }

        public EventCallback CallbackFn { get; }

        public SSECallback(EventType eventType, string name, EventCallback cb = null)
        {
            EventType = eventType;
            Name = name;
            CallbackFn = cb;
        }

        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !GetType().Equals(obj.GetType()))
                return false;

            var that = (SSECallback) obj;
            return this.EventType == that.EventType &&
                   this.Name == that.Name;
        }

        public override int GetHashCode()
        {
            return (int) this.EventType ^ this.Name.GetHashCode();
        }

        public override string ToString()
        {
            return EventType.ToString() + "." + Name;
        }
    }
}