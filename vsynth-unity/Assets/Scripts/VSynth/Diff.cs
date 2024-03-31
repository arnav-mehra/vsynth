using System.Collections.Generic;
using UnityEngine;

public abstract class Derivative {
    // float derivatives 
    public class FF : Derivative {
        public float v;

        public FF(float v) => this.v = v;

        public static FF Mag(EnvType et, List<AST> args, AST wrt, int coord) {
            AST v_ast = args[0];
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            Vector3 v = (Vector3)v_ast.vals[et];
            // d/dw (v_x(w)^2 + v_y(w)^2 + v_z(w)^2)^1/2
            // = 1/2 * (v_x^2 + v_y^2 + v_z^2)^-1/2 * (2 * v_x * v_x' + 2 * v_y * v_y' + 2 * v_z * v_z')
            // = (v . v') / |v|
            return new FF(Vector3.Dot(v, dv) / Vector3.Magnitude(v));
        }

        public static FF Dst(EnvType et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            Vector3 du = ((FV) u_ast.Diff(et, wrt, coord)).v;
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            Vector3 u = (Vector3)u_ast.vals[et];
            Vector3 v = (Vector3)v_ast.vals[et];
            // d/dw ((u_x - v_x)^2 + (u_y - v_y)^2 + (u_z - v_z)^2)^1/2
            // = 1/2 * ((u_x - v_x)^2 + (u_y - v_y)^2 + (u_z - v_z)^2)^-1/2 * (2(u_x - v_x)(u_x' - v_x') + 2(u_y - v_y)(u_y' - v_y') + 2(u_z - v_z)(u_z' - v_z'))
            // = (u - v) . (u' - v') / |u - v|
            return new FF(Vector3.Dot(u - v, du - dv) / Vector3.Magnitude(u - v));
        }

        public static FF Dot(EnvType et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            Vector3 du = ((FV) u_ast.Diff(et, wrt, coord)).v;
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            Vector3 u = (Vector3)u_ast.vals[et];
            Vector3 v = (Vector3)v_ast.vals[et];
            // d/dw (v(w) . u(w))
            // = v_x' * u_x + v_x * u_x' + v_y' * u_y + v_y * u_y' + v_z' * u_z + v_z * u_z'
            // = v' . u + v . u'
            return new FF(Vector3.Dot(du, u) + Vector3.Dot(dv, v));
        }

        public static FF FlA(EnvType et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            float du = ((FF) u_ast.Diff(et, wrt, coord)).v;
            float dv = ((FF) v_ast.Diff(et, wrt, coord)).v;
            return new FF(du + dv);
        }

        public static FF FlS(EnvType et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            float du = ((FF) u_ast.Diff(et, wrt, coord)).v;
            float dv = ((FF) v_ast.Diff(et, wrt, coord)).v;
            return new FF(du - dv);
        }

        public static FF FlM(EnvType et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            float du = ((FF) u_ast.Diff(et, wrt, coord)).v;
            float dv = ((FF) v_ast.Diff(et, wrt, coord)).v;
            float u = (float)u_ast.vals[et];
            float v = (float)v_ast.vals[et];
            return new FF(du * v + dv * u);
        }

        public static FF FlD(EnvType et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            float du = ((FF) u_ast.Diff(et, wrt, coord)).v;
            float dv = ((FF) v_ast.Diff(et, wrt, coord)).v;
            float u = (float)u_ast.vals[et];
            float v = (float)v_ast.vals[et];
            return new FF((v * du - u * dv) / (v * v));
        }
    }

    public class FV : Derivative {
        public Vector3 v;

        public FV(Vector3 v) => this.v = v;

        public static FV Add(EnvType et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            Vector3 du = ((FV) u_ast.Diff(et, wrt, coord)).v;
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            return new FV(du + dv);
        }

        public static FV Sub(EnvType et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            Vector3 du = ((FV) u_ast.Diff(et, wrt, coord)).v;
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            return new FV(du - dv);
        }

        public static FV ScD(EnvType et, List<AST> args, AST wrt, int coord) {
            AST c_ast = args[0], v_ast = args[1];
            float dc = ((FF) c_ast.Diff(et, wrt, coord)).v;
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            float c = (float)c_ast.vals[et];
            Vector3 v = (Vector3)v_ast.vals[et];
            // d/dw (v / c) = <(v_x / c)', (v_y / c)', (v_z / c)'> = (cv' - c'v) / c^2
            return new FV((c * dv - dc * v) / (c * c));
        }

        public static FV ScM(EnvType et, List<AST> args, AST wrt, int coord) {
            AST c_ast = args[0], v_ast = args[1];
            float dc = ((FF) c_ast.Diff(et, wrt, coord)).v;
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            float c = (float)c_ast.vals[et];
            Vector3 v = (Vector3)v_ast.vals[et];
            return new FV(dc * v + c * dv);
        }

        public static FV Cro(EnvType et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            Vector3 du = ((FV) u_ast.Diff(et, wrt, coord)).v;
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            Vector3 u = (Vector3)u_ast.vals[et];
            Vector3 v = (Vector3)v_ast.vals[et];
            // d/dw <u_y * v_z - u_z * v_y, - u_x * v_z + u_z * v_x, u_x * v_y - u_y * v_x>
            // x =   u_y' * v_z + u_y * v_z' - u_z' * v_y - u_z * v_y'
            // y = - u_x' * v_z - u_x * v_z' + u_z' * v_x + u_z * v_x'
            // z =   u_x' * v_y + u_x * v_y' - u_y' * v_x - u_y * v_x'
            return new FV(Vector3.Cross(du, v) + Vector3.Cross(u, dv));
        }

        // NO. I ain't paid enough to do this shit.
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