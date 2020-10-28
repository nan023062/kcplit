using System;
using System.Collections.Generic;

namespace Engine.Core
{ 
    public class DictionarySafe<TKey, TValue> : Dictionary<TKey, TValue> 
    {
        public new TValue this[TKey key]
        {
            set { base[key] = value; }
            get
            {
                TValue value = default(TValue);
                TryGetValue(key, out value);
                return value;
            }
        }
    }
}
