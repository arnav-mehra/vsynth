using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

static class Utils {
	public static List<int> Range(int start, int end) {
		if (start > end) return new();
		return Enumerable.Range(start, end - start + 1).ToList();
	}

	public static V TryGet<K, V>(this Dictionary<K, V> d, K key) {
		return d.ContainsKey(key) ? d[key] : default;
	}

	public static string StringifyAST(Search es, AST a) {
        var vars = es.examples[0].env.vars;
        return Range(0, vars.Count - 1).Aggregate(a.ToString(), (s, i) => {
            string v = vars[i].ToString().Replace('(', '<').Replace(')', '>');
            return s.Replace(v, ((char)('a' + i)).ToString());
        });
    }

    public static string CodifyAST(Search es, AST a) {
        var vars = es.examples[0].env.vars;
        string return_value = Range(0, vars.Count - 1).Aggregate(a.ToCode(), (s, i) => {
            string v = vars[i].ToString().Replace('(', '<').Replace(')', '>');
            return s.Replace(v, ((char)('a' + i)).ToString());
        });
        string args = Range(0, vars.Count - 1)
            .Select(i => "Vector3 " + (char)('a' + i))
            .Aggregate((a, b) => a + ", " + b);
        return $"public static Vector3 GeneratedFunction({args})" + " {\n" +
            "\t" + $"return {return_value};" + "\n" +
        "}";
    }

	public static string ToString(this ResultBuffer rb, Search s) {
        var header = "\n\tout_err\tdrawing_err\tcomplexity\tast";
        var table = rb.Count == 0 ? "\n"
            : rb.ConvertAll(r =>
                string.Format("{0,6:##0.000}", r.out_err)
                + "\t" + string.Format("{0,6:##0.000}", r.h_err)
                + "\t\t" + r.ast.complexity
                + "\t\t" + StringifyAST(s, r.ast)
                //+ "\n" + CodifyAST(s, r.ast)
            )
            .Aggregate((acc, ast) => acc + "\n\t" + ast);
        return header + "\n\t" + table;
    }

    public static int CountInversions(this ResultBuffer rb) {
        int inv_count = 0;
        for (int i = 0; i < rb.Count; i++) 
            for (int j = i + 1; j < rb.Count; j++) 
                if (rb[i].out_err > rb[j].out_err) inv_count++;
        return inv_count;
    }

    public class Timer {
        public static Stopwatch sw;

        public static void Start() => sw = Stopwatch.StartNew();
        public static long End() => sw.ElapsedMilliseconds;
    }
}