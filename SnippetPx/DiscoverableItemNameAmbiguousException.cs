using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SnippetPx
{
    [Serializable]
    class DiscoverableItemNameAmbiguousException<T> : DiscoverableItemNotFoundException
    {
        public List<T> PossibleMatches { get; private set; }

        public DiscoverableItemNameAmbiguousException()
        {
        }

        public DiscoverableItemNameAmbiguousException(string message)
            : base(message)
        {
        }

        protected DiscoverableItemNameAmbiguousException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public DiscoverableItemNameAmbiguousException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        internal DiscoverableItemNameAmbiguousException(string itemName, IEnumerable<T> possibleMatches, string message)
            : base(itemName, message)
        {
            PossibleMatches = possibleMatches.ToList();
        }
    }
}
