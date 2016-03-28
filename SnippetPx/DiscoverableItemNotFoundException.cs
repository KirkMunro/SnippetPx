using System;
using System.Runtime.Serialization;

namespace SnippetPx
{
    [Serializable]
    class DiscoverableItemNotFoundException : Exception
    {
        public string ItemName { get; private set; }

        public DiscoverableItemNotFoundException()
        {
        }

        public DiscoverableItemNotFoundException(string message)
            : base(message)
        {
        }

        protected DiscoverableItemNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public DiscoverableItemNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        internal DiscoverableItemNotFoundException(string itemName, string message)
            : base(message)
        {
            ItemName = itemName;
        }
    }
}
