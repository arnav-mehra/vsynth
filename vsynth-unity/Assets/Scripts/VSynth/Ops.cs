using UnityEngine;

namespace System.Runtime.CompilerServices { // this fixes a bug w/ 2019 vs and records
    internal static class IsExternalInit {}
}

namespace Assets.Scripts.Ops {
	public class Ops {
        public enum Op {
            None, // returns some literal
            Add, Sub, Cro, Rot, ScM, ScD, // returns vector
            Dst, Dot, Mag, FlM, FlD, FlA, FlS // returns float
        }

        public static Vector3 Add(Vector3 a, Vector3 b) => a + b;
        public static Vector3 Sub(Vector3 a, Vector3 b) => a - b;
        public static Vector3 Cro(Vector3 a, Vector3 b) => Vector3.Cross(a, b);
        public static Vector3 Rot(Vector3 a, Vector3 b, float c) => Quaternion.AngleAxis(c, b) * a;
        public static Vector3 ScM(Vector3 b, float a) => a * b;
        public static Vector3 ScD(Vector3 b, float a) => (1.0f / a) * b;

        public static float Dst(Vector3 a, Vector3 b) => Vector3.Distance(a, b);
        public static float Dot(Vector3 a, Vector3 b) => Vector3.Dot(a, b);
        public static float Mag(Vector3 a) => Vector3.Magnitude(a);
        public static float FlM(float a, float b) => a * b;
        public static float FlD(float a, float b) => a / b;
        public static float FlA(float a, float b) => a + b;
        public static float FlS(float a, float b) => a - b;
    }

    public static class ComplexityExt {
        readonly static int[] COMPLEXITIES = {
            0, // None,
            1, 1, 2, 3, 1, 1, // Add, Sub, Cro, Rot, ScM, ScD,
            2, 1, 1, 1, 1, 1, 1 // Dst, Dot, Mag, FlM, FlD, FlA, FlS
        };
        public const int MIN_OP_C = 1;
        public const int MAX_OP_C = 3;

        public static int Complexity(this Ops.Op op) {
            return COMPLEXITIES[(int)op];
        }
    }
}