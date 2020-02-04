using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vulpine.Core.Data;
using Vulpine.Core.Data.Tables;

namespace Vulpine.Core.Data.Tables
{
    public class TableSystem<K, E> : Table<K, E>
    {
        //uses a system dictionary to act as the internal table structure
        private System.Collections.Generic.Dictionary<K, E> table;


        public TableSystem()
        {
            table = new Dictionary<K, E>();
        }

        public TableSystem(int cap)
        {
            table = new Dictionary<K, E>(cap);
        }

        ///// <summary>
        ///// Constructs a new table, containing multiple entries. The number of
        ///// buckets is automatically determined based on the number of entries.
        ///// </summary>
        ///// <param name="pairs">The entries into the table</param>
        //public TableSystem(IEnumerable<KeyedItem<K, E>> pairs)
        //{
        //    //obtain the base capacity from the number of items
        //    int cap = (int)(pairs.Count() * 1.25) + 10;

        //    table = new Dictionary<K, E>(cap);

        //    //adds the key-value pairs one at a time
        //    foreach (var pair in pairs) Add(pair.Key, pair.Item);
        //}

        ///// <summary>
        ///// Constructs a new table, containing multiple entries. The keys
        ///// for the entries are derived from a separate key selector function.
        ///// The number of buckets is automatically determined based on the
        ///// number of entries.
        ///// </summary>
        ///// <param name="items">The items to be stored in the table</param>
        ///// <param name="selector">A function to derive the keys for each
        ///// item that is stored in the table</param>
        //public TableSystem(IEnumerable<E> items, Func<E, K> selector)
        //{
        //    //obtain the base capacity from the number of items
        //    int cap = (int)(items.Count() * 1.25) + 10;

        //    table = new Dictionary<K, E>(cap);

        //    //adds the key-value pairs one at a time
        //    foreach (var item in items)
        //    {
        //        var key = selector.Invoke(item);
        //        Add(key, item);
        //    }
        //}


        public override int Buckets
        {
            get { return -1; }
        }

        public override int Count
        {
            get { return table.Count;  }
        }

        #region Dictionary Implementation...

        /// <summary>
        /// Determines if a particular key is contained in this dictionary.
        /// It returns true if the key exists, and false otherwise.
        /// </summary>
        /// <param name="key">Key to test</param>
        /// <returns>True if the key exists, false if otherwise</returns>
        public override bool HasKey(K key)
        {
            if (key == null) return false;
            return table.ContainsKey(key);
        }

        /// <summary>
        /// Inserts a value with a given key into the dictionary. 
        /// </summary>
        /// <param name="key">Key of the item to be inserted</param>
        /// <param name="item">Item to be inserted</param>
        /// <exception cref="ArgumentNullException">If either the key or
        /// the item are null</exception>
        /// <exception cref=" InvalidOperationException">If a duplicate key
        /// is inserted when the dictionary requires unique keys</exception>
        public override void Add(K key, E item)
        {
            //asserts that the key and item are not null
            if (key == null) throw new ArgumentNullException();
            if (item == null) throw new ArgumentNullException();

            table.Add(key, item);
        }

        /// <summary>
        /// Removes all items from the dictionary, causing the dictionary
        /// to revert to it's original initialized state.
        /// </summary>
        public override void Clear()
        {
            table.Clear();
        }

        /// <summary>
        /// Creates an enumeration over all the keys and values in the
        /// dictionary, together as keyed items. If you need something
        /// more specific, consider ListKeys() or ListItems().
        /// </summary>
        /// <returns>An enumeration of keyed items</returns>
        public override IEnumerator<KeyedItem<K, E>> GetEnumerator()
        {
            foreach (KeyValuePair<K, E> pair in table)
            {
                K key = pair.Key;
                E item = pair.Value;

                yield return new KeyedItem<K, E>(key, item);
            }
        }

        #endregion ///////////////////////////////////////////////////////////////////////////

        #region Table Implementation...

        /// <summary>
        /// Retrieves a value from the table that matches the given key. If no
        /// match for the key can be found, it returns null.
        /// </summary>
        /// <param name="key">Key of the desired item</param>
        /// <returns>The matching item, or null if not found</returns>
        public override E GetValue(K key)
        {
            //asserts that the key is not null
            if (key == null) throw new ArgumentNullException();

            E holder = default(E);
            bool success = table.TryGetValue(key, out holder);
            return (success) ? holder : default(E);
        }

        /// <summary>
        /// Overwrites any existing value that might have been associated
        /// with the given key, with the new value. If the key did not
        /// previously have a value, a new key-value pair is created.
        /// </summary>
        /// <param name="key">Key to overwrite</param>
        /// <param name="item">New value for the key</param>
        /// <exception cref="ArgumentNullException">If either the key or
        /// the item are null</exception>
        public override void Overwrite(K key, E item)
        {
            //asserts that the key and item are not null
            if (key == null) throw new ArgumentNullException();
            if (item == null) throw new ArgumentNullException();

            //removes the key if it already exists in the table
            bool check = table.ContainsKey(key);
            if (check) table.Remove(key);

            //adds the new key value pair
            table.Add(key, item);
        }

        /// <summary>
        /// Removes an item from the table matching the given key. It 
        /// returns the item that was removed. If no match for the key 
        /// can be found, it returns null.
        /// </summary>
        /// <param name="key">Key of the item to remove</param>
        /// <returns>The item that was removed, or null if not found</returns>
        public override E Remove(K key)
        {
            //asserts that the key is not null
            if (key == null) throw new ArgumentNullException();

            E holder = default(E);
            bool success = table.TryGetValue(key, out holder);
            if (success) table.Remove(key);

            return (success) ? holder : default(E);
        }

        #endregion ///////////////////////////////////////////////////////////////////////////
    }
}
