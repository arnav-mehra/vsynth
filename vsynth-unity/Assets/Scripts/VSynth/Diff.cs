using UnityEngine;

public abstract class Derivative {
    // float derivatives 
    public class FF : Derivative {
        public float v;

        public FF(float v) => this.v = v;

        public static FF Mag(EnvType et, AST v_ast, AST wrt) {
            Vector3 dv = ((FV) v_ast.D(et, wrt)).v;
            Vector3 v = (Vector3)v_ast.vals[et];
            // d/dv sqrt(x(v)^2 + y(v)^2 + z(v)^2) -> wolfram alpha
            // (x(v)*x'(v) + y(v)*y'(v) + z(v)*z'(v)) / sqrt(x(v)^2 + y(v)^2 + z(v)^2)
            return new FF(Vector3.Dot(v * dv) / Vector3.Magnitude(v));
        }

        public static FF Dot(EnvType et, AST u_ast, AST v_ast, AST wrt) {
            Vector3 du = ((FV) u_ast.D(et, wrt)).v;
            Vector3 dv = ((FV) v_ast.D(et, wrt)).v;
            Vector3 u = (Vector3)u_ast.v[et];
            Vector3 v = (Vector3)v_ast.v[et];
            // d/dv (x1(v) * x2(v) + y1(v) * y2(v) + z1(v) * z2(v)) -> wolfram alpha
            // x1'(v) * x2(v) + x1(v) * x2'(v) + y1'(v) * y2(v) + y1(v) * y2'(v) + z1'(v) * z2(v) + z1(v) * z2'(v)
            return new FF(Vector3.Dot(du, u) + Vector3.Dot(dv, v));
        }

        public static FF FlA(EnvType et, AST u_ast, AST v_ast, AST wrt) {
            float du = ((FF) u_ast.D(et, wrt)).v;
            float dv = ((FF) v_ast.D(et, wrt)).v;
            return new FF(du + dv);
        }

        public static FF FlS(EnvType et, AST u_ast, AST v_ast, AST wrt) {
            float du = ((FF) u_ast.D(et, wrt)).v;
            float dv = ((FF) v_ast.D(et, wrt)).v;
            return new FF(du - dv);
        }

        public static FF FlM(EnvType et, AST u_ast, AST v_ast, AST wrt) {
            float du = ((FF) u_ast.D(et, wrt)).v;
            float dv = ((FF) v_ast.D(et, wrt)).v;
            float u = (float)u_ast.v[et];
            float v = (float)v_ast.v[et];
            return new FF(du * v + dv * u);
        }

        public static FF FlD(EnvType et, AST u_ast, AST v_ast, AST wrt) {
            float du = ((FF) u_ast.D(et, wrt)).v;
            float dv = ((FF) v_ast.D(et, wrt)).v;
            float u = (float)u_ast.v[et];
            float v = (float)v_ast.v[et];
            return new FF((v * du - u * dv) / (v * v));
        }
    }

    public class FV : Derivative {
        public Vector3 v;

        public FV(Vector3 v) => this.v = v;

        public static FV Add(EnvType et, AST u_ast, AST v_ast, AST wrt) {
            Vector3 du = ((FV) u_ast.D(et, wrt)).v;
            Vector3 dv = ((FV) v_ast.D(et, wrt)).v;
            return new FV(du + dv);
        }

        public static FV Sub(EnvType et, AST u_ast, AST v_ast, AST wrt) {
            Vector3 du = ((FV) u_ast.D(et, wrt)).v;
            Vector3 dv = ((FV) v_ast.D(et, wrt)).v;
            return new FV(du - dv);
        }

        public static FV ScD(EnvType et, AST c_ast, AST v_ast, AST wrt) {
            float dc = ((FF) u_ast.D(et, wrt)).v;
            Vector3 dv = ((FV) v_ast.D(et, wrt)).v;
            float c = (float)c_ast.v[et];
            Vector3 v = (Vector3)v_ast.v[et];
            // d/dv <v_x/c, v_y/c, v_z/c> = <c v_x' - c' v_x / c^2, ...> = cv' - c'v / c^2
            return new FV((c * dv - dc * v) / (c * c));
        }

        public static FV ScM(EnvType et, AST c_ast, AST v_ast, AST wrt) {
            float dc = ((FF) u_ast.D(et, wrt)).v;
            Vector3 dv = ((FV) v_ast.D(et, wrt)).v;
            float c = (float)c_ast.v[et];
            Vector3 v = (Vector3)v_ast.v[et];
            return new FV(dc * v + c * dv);
        }

        public static FV Crs(EnvType et, AST u_ast, AST v_ast, AST wrt) {
            Vector3 du = ((FV) u_ast.D(et, wrt)).v;
            Vector3 dv = ((FV) v_ast.D(et, wrt)).v;
            Vector3 u = (Vector3)u_ast.v[et];
            Vector3 v = (Vector3)v_ast.v[et];
            // d/dw <uy * vz - uz * vy, - ux * vz + uz * vx, ux * vy - uy * vx>
            // x =   uy' * vz + uy * vz' - uz' * vy - uz * vy'
            // y = - ux' * vz - ux * vz' + uz' * vx + uz * vx'
            // z =   ux' * vy + ux * vy' - uy' * vx - uy * vx'
            return new FV(Vector3.Cross(du, v) + Vector3.Cross(u, dv));
        }

        // NO.
        // public static FV Rot(EnvType et, AST u_ast, AST v_ast, AST c_ast, AST wrt) {
        //     Vector3 du = ((FV) u_ast.D(et, wrt)).v;
        //     Vector3 dv = ((FV) v_ast.D(et, wrt)).v;
        //     Vector3 u = (Vector3)u_ast.v[et];
        //     Vector3 v = (Vector3)v_ast.v[et];
        //     // d/dw v(w) - 2 proj_u(w) v(w) = d/dw v(w) - 2 (u(w) * v(w))/(v(w) * v(w)) * v(w)
        //     // v'(w) - 2 * (v(w) * (u(w) * v(w)))' * 1 / (v(w) * v(w)) - 2 * v(w) * (u(w) * v(w)) * (1 / (v(w) * v(w)))'
            
        //     // (v * (u . v))' = v' * (u . v) + v * (u . v)' = v' * (u . v) + v * (u . v)'
        //     return new FV(Vector3.Cross(du, v) + Vector3.Cross(u, dv));
        // }
    }
}