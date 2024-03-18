using System;
using System.Collections.Generic;
using UnityEngine;

// Vec+Float Locality-Sensitive Comparison for Dictionary usage.
// Idea: switch to an LSH, might give more reliable lookups.

public class LSC : IEqualityComparer<object> {
	public new bool Equals(object x1, object x2) => (x1, x2) switch {
		(Vector3 v1, Vector3 v2) => VecLSC.Equals(v1, v2),
		(float f1, float f2) => FltLSC.Equals(f1, f2),
		_ => x1.Equals(x2)
	};

	public int GetHashCode(object x) => x switch {
		Vector3 v => VecLSC.GetHashCode(v),
		float f => FltLSC.GetHashCode(f),
		_ => x.GetHashCode()
	};
}

static class RoundExt {
	public static int SIG_DIGITS = 5;

	public static float Round(this float f) => (float)Math.Round(f, SIG_DIGITS);
}

class VecLSC {
	public static bool Equals(Vector3 v1, Vector3 v2) {
		return v1.x.Round() == v2.x.Round()
			&& v1.y.Round() == v2.y.Round()
			&& v1.z.Round() == v2.z.Round();
	}

	public static int GetHashCode(Vector3 v) {
		int hash = 17;
		hash = hash * 23 + v.x.Round().GetHashCode();
		hash = hash * 23 + v.y.Round().GetHashCode();
		hash = hash * 23 + v.z.Round().GetHashCode();
		return hash;
	}
}

class FltLSC {
	public static bool Equals(float f1, float f2) => f1.Round() == f2.Round();

	public static int GetHashCode(float f) => f.Round().GetHashCode();
}