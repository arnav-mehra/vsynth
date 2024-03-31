using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Derivative {
    // float derivatives 
    public class FF : Derivative {
        public float v;

        public FF(float v) => this.v = v;

        public static FF Mag(EnvType et, List<AST> args, AST wrt, int coord) {
            AST v_ast = args[0];
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            Vector3 v = (Vector3) v_ast.vals[et];
            // d/dw (v_x(w)^2 + v_y(w)^2 + v_z(w)^2)^1/2
            // = 1/2 * (v_x^2 + v_y^2 + v_z^2)^-1/2 * (2 * v_x * v_x' + 2 * v_y * v_y' + 2 * v_z * v_z')
            // = (v . v') / |v|
            return new FF(Vector3.Dot(v, dv) / Vector3.Magnitude(v));
        }

        /*public static FF Dst(EnvType et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            Vector3 du = ((FV) u_ast.Diff(et, wrt, coord)).v;
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            Vector3 u = (Vector3) u_ast.vals[et];
            Vector3 v = (Vector3) v_ast.vals[et];
            // d/dw ((u_x - v_x)^2 + (u_y - v_y)^2 + (u_z - v_z)^2)^1/2
            // = 1/2 * ((u_x - v_x)^2 + (u_y - v_y)^2 + (u_z - v_z)^2)^-1/2 * (2(u_x - v_x)(u_x' - v_x') + 2(u_y - v_y)(u_y' - v_y') + 2(u_z - v_z)(u_z' - v_z'))
            // = (u - v) . (u' - v') / |u - v|
            return new FF(Vector3.Dot(u - v, du - dv) / Vector3.Magnitude(u - v));
        }*/

        public static FF Dot(EnvType et, List<AST> args, AST wrt, int coord) {
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
            float u = (float) u_ast.vals[et];
            float v = (float) v_ast.vals[et];
            return new FF(du * v + dv * u);
        }

        public static FF FlD(EnvType et, List<AST> args, AST wrt, int coord) {
            AST u_ast = args[0], v_ast = args[1];
            float du = ((FF) u_ast.Diff(et, wrt, coord)).v;
            float dv = ((FF) v_ast.Diff(et, wrt, coord)).v;
            float u = (float) u_ast.vals[et];
            float v = (float) v_ast.vals[et];
            return new FF((v * du - u * dv) / (v * v));
        }
    }

    // vector derivatives
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
            AST v_ast = args[0], c_ast = args[1];
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            float dc = ((FF) c_ast.Diff(et, wrt, coord)).v;
            Vector3 v = (Vector3) v_ast.vals[et];
            float c = (float) c_ast.vals[et];
            // d/dw (v / c) = <(v_x / c)', (v_y / c)', (v_z / c)'> = <(v_x / c)', (v_y / c)', (v_z / c)'>
            return new FV((c * dv - dc * v) / (c * c));
        }

        public static FV ScM(EnvType et, List<AST> args, AST wrt, int coord) {
            AST v_ast = args[0], c_ast = args[1];
            Vector3 dv = ((FV) v_ast.Diff(et, wrt, coord)).v;
            float dc = ((FF) c_ast.Diff(et, wrt, coord)).v;
            Vector3 v = (Vector3) v_ast.vals[et];
            float c = (float) c_ast.vals[et];
            return new FV(dc * v + c * dv);
        }

        public static FV Cro(EnvType et, List<AST> args, AST wrt, int coord) {
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

    public static float GetError(EnvType et, AST ast, Vector3 target, List<Vector3> deltas) {
        var eval = (Vector3) ast.vals[et];
        var out_err_sq = Vector3.SqrMagnitude(target - eval);
        var in_err_sq = deltas.Aggregate(0.0f, (acc, delta) => acc + Vector3.SqrMagnitude(delta));
        var C = out_err_sq + in_err_sq;
        //Debug.Log(errs.Aggregate((int)(C * 10.0f)/10.0f + ": ", (acc, v) => acc + v + " ") + "-> " + (target - f));
        return C;
    }

    const float LEARNING_RATE = 0.1f;
    const int MAX_ITERS = 50;
    
    /* GradientDescent:
     *      Run gradient descent on each input of ast to minimize io error terms.
     * 
     * Cost Function:
     *      C = delta_out . delta_out + sum(delta_vi . delta_vi)
     *        = |out - f(v1 + delta_v1, v2 + delta_v2, ...)|^2 + sum_i(|delta_vi|^2)
     *      C(delta_wx) = |out - f(wx + delta_wx)|^2 + delta_wx^2 + sum_rem
     *      C(delta_wy) = |out - f(wy + delta_wy)|^2 + delta_wy^2 + sum_rem
     *      C(delta_wz) = |out - f(wz + delta_wz)|^2 + delta_wz^2 + sum_rem
     * 
     * Gradient:
     *      dC/ddelta_wx = (|out - f(wx + delta_wx)|^2)' + (delta_wx^2)'
     *                 = 2 (out - f(wx + delta_wx))' . (out - f(wx + delta_wx)) + 2 delta_wx
     *                 = 2 (-f'(wx + delta_wx)) . (out - f(wx + delta_wx)) + 2 delta_wx
     *                 = 2 (delta_wx - f'(wx + delta_wx) . (out - f(wx + delta_wx)))
     *      dC/ddelta_wy = 2 (delta_wy - f'(wy + delta_wy) . (out - f(wy + delta_wy)))
     *      dC/ddelta_wz = 2 (delta_wz - f'(wz + delta_wz) . (out - f(wz + delta_wz)))
     */
    public static float GradientDescent(EnvType et, AST ast, Vector3 target, List<AST> var_asts) {
        var in_vals = var_asts.ConvertAll(a => (Vector3) a.vals[et]);       // original input vals.
        var in_deltas = var_asts.ConvertAll(_ => Vector3.zero);             // input delta terms.
        var rates = var_asts.ConvertAll(_ => LEARNING_RATE);                // gradient descent rates per input error term.
        //GetError(et, ast, target, in_deltas);

        // run gradient descent on each variable MAX_ITERS times.
        for (int iter = 1; iter <= MAX_ITERS; iter++) {
            // run gradient descent for each variable.
            for (int i_w = 0; i_w < var_asts.Count; i_w++) {
                var ast_w = var_asts[i_w];
                
                var w = in_vals[i_w];
                var delta_w = in_deltas[i_w];
                var err_w = Vector3.SqrMagnitude(delta_w);                  // |delta_w|^2
                var rate_w = rates[i_w];

                var eval = (Vector3) ast.vals[et];                          // f(w_z + delta_w_z)
                var delta_out = target - eval;                              // out - f(w_z + delta_w_z)
                var err_out = Vector3.SqrMagnitude(delta_out);              // |out - f(w_z + delta_w_z)|^2

                var fp_w_x = ((FV) ast.Diff(EnvType.User, ast_w, 0)).v;     // f'(w_x + delta_w_x)
                var fp_w_y = ((FV) ast.Diff(EnvType.User, ast_w, 1)).v;     // f'(w_y + delta_y_x)
                var fp_w_z = ((FV) ast.Diff(EnvType.User, ast_w, 2)).v;     // f'(w_z + delta_z_x)

                var dC_ddelta_w = new Vector3(
                    2.0f * (delta_w.x - Vector3.Dot(fp_w_x, delta_out)),    // dC / ddelta_w_x
                    2.0f * (delta_w.y - Vector3.Dot(fp_w_y, delta_out)),    // dC / ddelta_w_y
                    2.0f * (delta_w.z - Vector3.Dot(fp_w_z, delta_out))     // dC / ddelta_w_z
                );

                var new_delta_w = delta_w - rate_w * dC_ddelta_w;           // new_delta_w = gradient_descent(delta_w).
                var new_err_w = Vector3.SqrMagnitude(new_delta_w);          // |new_delta_w|^2

                ast_w.vals[et] = w + new_delta_w;                           // update w_ast using new_delta_w.
                ast.ReEval(et);                                             // re-eval ast to maintain cache consistency.

                var new_eval = (Vector3) ast.vals[et];                      // f(w_z + new_delta_w_z)
                var new_delta_out = target - new_eval;                      // out - f(w_z + new_delta_w_z)
                var new_err_out = Vector3.SqrMagnitude(new_delta_out);      // |out - f(w_z + new_delta_w_z)|^2

                var derr = (new_err_w + new_err_out) - (err_w + err_out);   // use change in error to determine update rules.
                if (derr > 0) {                                             // error increased:
                    ast_w.vals[et] = w + delta_w;                           //      - revert w_ast update
                    ast.ReEval(et);                                         //        and re-establish cache consistency.
                    rates[i_w] *= 0.2f;                                     //      - decrease learning rate.
                    //Debug.Log("Learning rate decreased to " + rates[i_w]);
                } else {                                                    // error decreased: 
                    in_deltas[i_w] = new_delta_w;                           //      - proceed with new delta.
                }
            }
            //GetError(et, ast, target, in_deltas);
        }

        // compute final hypothesized error
        var h_err = GetError(et, ast, target, in_deltas);

        // restore env and ast.
        for (int i = 0; i < var_asts.Count; i++) {
            var_asts[i].vals[et] = in_vals[i];
        }
        ast.ReEval(et);

        return h_err;
    }
}