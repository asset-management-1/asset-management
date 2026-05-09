namespace Be.Haven.EventBus.Constants;

/// <summary>
/// Holds all constant names for Queues, Topics, and Subscriptions used in the EventBus.
/// Centralizing these names prevents hardcoding and makes maintenance easier.
/// </summary>
public static class QueueNameConstants
{
    /// <summary>
    /// Contains the names of all Topics published and subscribed to within the system.
    /// </summary>
    public static class TopicNames
    {
        /// <summary>
        /// The Topic name for events related to National Address To-Do items.
        /// This Topic is used to publish To-Do messages from the National Address Portal.
        /// </summary>
        public const string NATION_ADDRESS_TODO_TOPIC = "nation-address-topic-todo";
    }

    /// <summary>
    /// Contains the names of Subscriptions corresponding to each Topic.
    /// </summary>
    public static class SubNames
    {
        /// <summary>
        /// The Subscription name for the NationAddressTodoTopic.
        /// This Subscription allows services to receive To-Do messages.
        /// </summary>
        public const string NATION_ADDRESS_TODO_SUB = "nation-address-topic-todo-sub";
    }
}
