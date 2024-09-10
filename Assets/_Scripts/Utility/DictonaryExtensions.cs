using System.Collections;
using System.Collections.Generic;

public static class Dictionary {
    public static void AddIfNotNull<K, V>(this Dictionary<K, V> dict, K key, V val) {
        if (val != null) {
            dict.Add(key, val);
        }
    }
    public static void AddIfList<K, V>(this Dictionary<K, List<V>> dict, K key, V val) {
        if (val != null) {
            if (!dict.ContainsKey(key)) {
                dict.Add(key, new List<V>());
            }
            dict[key].Add(val);
        }
    }
    public static void RemoveIfList<K, V>(this Dictionary<K, List<V>> dict, K key, V val) {
        if (val != null) {
            if (dict.ContainsKey(key)) {
                dict[key].Remove(val);
                if (dict[key].Count == 0) {
                    dict.Remove(key);
                }
            }
        }
    }
}