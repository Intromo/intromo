using System;

namespace Domain
{
    public class Message
    {
        public DateTimeOffset PublishedAt { get; } = DateTimeOffset.UtcNow;
        public string Room { get; set; }
        public Guid FromId { get; set; }
        public string From { get; set; }
        public Guid? TargetId { get; set; }
        public string Body { get; set; }

        public override string ToString()
        {
            return $"[{PublishedAt:hh:mm:ss}] {From} says, \"{Body}\"";
        }
    }
}