using Prism.Events;

namespace DockPrismSample.Events;

public record DocumentClosedPayload(string Id, string Title);

public class DocumentClosedEvent : PubSubEvent<DocumentClosedPayload>;

public record DocumentSavedPayload(string Id, string Title, string Timestamp);

public class DocumentSavedEvent : PubSubEvent<DocumentSavedPayload>;
