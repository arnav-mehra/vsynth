using UnityEngine;

public abstract class Derivative {
    // float derivatives 
    public class FF : Derivative {
        public float v;

        public FF(float v) => this.v = v;

        public static FF Mag(EnvType et, AST v, AST wrt) {
            FV dv = (FV) v.D(et, wrt);
            Vector3 dvec = dv.v;
            // d/dv sqrt(x(v)^2 + y(v)^2 + z(v)^2) -> wolfram alpha
            // (x(v)*x'(v) + y(v)*y'(v) + z(v)*z'(v)) / sqrt(x(v)^2 + y(v)^2 + z(v)^2)

            float xp = dvec.x;
            float yp = dvec.y;
            float zp = dvec.z;
            float x = ((Vector3)v.vals[et]).x;
            float y = ((Vector3)v.vals[et]).y;
            float z = ((Vector3)v.vals[et]).z;
            return new FF((x * xp + y * yp + z * zp) / Mathf.Sqrt(x * x + y * y + z * z));
        }
    }

    public class FV : Derivative {
        public Vector3 v;

        public FV(Vector3 v) => this.v = v;

        public static FV Add(EnvType et, AST a, AST b, AST wrt) {
            FV d0 = (FV) a.D(et, wrt);
            FV d1 = (FV) b.D(et, wrt);
            return new FV(d0.v + d1.v);
        }

        public static FV ScM(EnvType et, AST a, AST b, AST wrt) {
            FV d0 = (FV) a.D(et, wrt);
            FF d1 = (FF) b.D(et, wrt);
            return new FV(d0.v * d1.v);
        }
    }
}