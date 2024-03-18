using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

static class Utils {
	public static List<int> Range(int start, int end) {
		if (start > end) return new();
		return Enumerable.Range(start, end - start + 1).ToList();
	}

	public static Action<Action<int>> ForRange(int start, int end) {
		return Range(start, end).ForEach;
	}

	public static V TryGet<K, V>(this Dictionary<K, V> d, K key) {
		return d.ContainsKey(key) ? d[key] : default;
	}

	public static string StringifyAST(Search es, AST a) => (
        Range(0, es.env.vars.Count - 1).Aggregate(a.ToString(), (s, i) => {
            string v = es.env.vars[i].ToString().Replace('(', '<').Replace(')', '>');
            return s.Replace(v, ((char)('a' + i)).ToString());
        })
    );

	public class Timer {
        public static Stopwatch sw;

        public static void Start() => sw = Stopwatch.StartNew();

        public static long End() => sw.ElapsedMilliseconds;
    }
}
