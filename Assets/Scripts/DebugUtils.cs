﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DefaultNamespace
{

    public static class DebugUtils
    {

        public static string ToString<T>(List<T> list, Func<T, string> toString) {
            if (list.Count == 0) {
                return $"List[_EMPTY_]";
            }
            StringBuilder builder = new StringBuilder();
            builder.Append($"List[");
            foreach (var item in list) {
                builder.Append($"{toString(item)}, ");
            }

            builder.Append($"]");
            return builder.ToString();
        }
        
        public static string ToString<K, V>(Dictionary<K, V> dictionary, Func<K, string> toStringK, Func<V, string> toStringV) {
            if (dictionary.Count == 0) {
                return $"List[_EMPTY_]";
            }
            
            StringBuilder builder = new StringBuilder();
            builder.Append($"Dictionary [");
            foreach (var (key, value) in dictionary) {
                builder.Append($"({toStringK(key)} -> {toStringV(value)}), ");
            }
            builder.Append($"]\n");
            return builder.ToString();
        }  
    }
}