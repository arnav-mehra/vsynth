using System.Collections.Generic;
using UnityEngine;

public abstract class Derivative {
    // float derivatives 
    public class FF : Derivative {
        public float v;

        public FF(float v) => this.v = v;

        public static FF Mag(int et, List<AST> args, AST wrt, int coord) {
            AST v_ast = args[0];
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            Vector3 v = (Vector3) v_ast.vals[et];
            // d/dw (v_x(w)^2 + v_y(w)^2 + v_z(w)^2)^1/2
            // = 1/2 * (v_x^2 + v_y^2 + v_z^2)^-1/2 * (2 * v_x * v_x' + 2 * v_y * v_y' + 2 * v_z * v_z')
            // = (v . v') / |v|
            return new FF(Vector3.Dot(v, dv) / Vector3.Magnitude(v));
        }

        public static FF Dot(int et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            Vector3 du = ((FV) u_ast.Diff(et, wrt, coord)).v;
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            Vector3 u = (Vector3) u_ast.vals[et];
            Vector3 v = (Vector3) v_ast.vals[et];
            // d/dw (v(w) . u(w))
            // = v_x' * u_x + v_x * u_x' + v_y' * u_y + v_y * u_y' + v_z' * u_z + v_z * u_z'
            // = v' . u + v . u'
            return new FF(Vector3.Dot(dv, u) + Vector3.Dot(v, du));
        }

        public static FF FlA(int et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            float du = ((FF) u_ast.Diff(et, wrt, coord)).v;
            float dv = ((FF) v_ast.Diff(et, wrt, coord)).v;
            return new FF(du + dv);
        }

        public static FF FlS(int et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            float du = ((FF) u_ast.Diff(et, wrt, coord)).v;
            float dv = ((FF) v_ast.Diff(et, wrt, coord)).v;
            return new FF(du - dv);
        }

        public static FF FlM(int et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            float du = ((FF) u_ast.Diff(et, wrt, coord)).v;
            float dv = ((FF) v_ast.Diff(et, wrt, coord)).v;
            float u = (float) u_ast.vals[et];
            float v = (float) v_ast.vals[et];
            return new FF(du * v + dv * u);
        }

        public static FF FlD(int et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            float du = ((FF) u_ast.Diff(et, wrt, coord)).v;
            float dv = ((FF) v_ast.Diff(et, wrt, coord)).v;
            float u = (float) u_ast.vals[et];
            float v = (float) v_ast.vals[et];
            return new FF((v * du - u * dv) / (v * v));
        }

        public static FF FlN(int et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0];
            float du = ((FF) u_ast.Diff(et, wrt, coord)).v;
            return new FF(-du);
        }

        public static FF FlI(int et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0];
            float du = ((FF) u_ast.Diff(et, wrt, coord)).v;
            float u = (float) u_ast.vals[et];
            return new FF(-du / (u * u));
        }
    }

    // vector derivatives
    public class FV : Derivative {
        public Vector3 v;

        public FV(Vector3 v) => this.v = v;

        public static FV Neg(int et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0];
            Vector3 du = ((FV) u_ast.Diff(et, wrt, coord)).v;
            return new FV(-du);
        }

        public static FV Add(int et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            Vector3 du = ((FV) u_ast.Diff(et, wrt, coord)).v;
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            return new FV(du + dv);
        }

        public static FV Sub(int et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            Vector3 du = ((FV) u_ast.Diff(et, wrt, coord)).v;
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            return new FV(du - dv);
        }

        public static FV ScD(int et, List<AST> args, AST wrt, int coord) {
            AST v_ast = args[0], c_ast = args[1];
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            float dc = ((FF) c_ast.Diff(et, wrt, coord)).v;
            Vector3 v = (Vector3) v_ast.vals[et];
            float c = (float) c_ast.vals[et];
            // d/dw (v / c) = <(v_x / c)', (v_y / c)', (v_z / c)'> = <(v_x / c)', (v_y / c)', (v_z / c)'>
            return new FV((c * dv - dc * v) / (c * c));
        }

        public static FV ScM(int et, List<AST> args, AST wrt, int coord) {
            AST v_ast = args[0], c_ast = args[1];
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            float dc = ((FF) c_ast.Diff(et, wrt, coord)).v;
            Vector3 v = (Vector3) v_ast.vals[et];
            float c = (float) c_ast.vals[et];
            return new FV(dc * v + c * dv);
        }

        public static FV Cro(int et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            Vector3 du = ((FV) u_ast.Diff(et, wrt, coord)).v;
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            Vector3 u = (Vector3) u_ast.vals[et];
            Vector3 v = (Vector3) v_ast.vals[et];
            // d/dw <u_y * v_z - u_z * v_y, - u_x * v_z + u_z * v_x, u_x * v_y - u_y * v_x>
            // x =   u_y' * v_z + u_y * v_z' - u_z' * v_y - u_z * v_y'
            // y = - u_x' * v_z - u_x * v_z' + u_z' * v_x + u_z * v_x'
            // z =   u_x' * v_y + u_x * v_y' - u_y' * v_x - u_y * v_x'
            return new FV(Vector3.Cross(du, v) + Vector3.Cross(u, dv));
        }
    }
}