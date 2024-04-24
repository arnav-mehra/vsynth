using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GradientDescent {
    const float LEARNING_RATE = 0.1f;
    const int MAX_ITERS = 50;
    
    /* Run gradient descent on each input of ast to minimize io error terms.
     * 
     * Cost Function:
     *      C = delta_out . delta_out + sum_i(delta_vi . delta_vi)
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
    public static float Run(EnvType et, AST ast, Vector3 target, List<AST> var_asts) {
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

                var fp_w_x = ((Derivative.FV) ast.Diff(EnvType.User, ast_w, 0)).v;     // f'(w_x + delta_w_x)
                var fp_w_y = ((Derivative.FV) ast.Diff(EnvType.User, ast_w, 1)).v;     // f'(w_y + delta_y_x)
                var fp_w_z = ((Derivative.FV) ast.Diff(EnvType.User, ast_w, 2)).v;     // f'(w_z + delta_z_x)

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

    public static float GetError(EnvType et, AST ast, Vector3 target, List<Vector3> deltas) {
        var eval = (Vector3) ast.vals[et];
        var out_err_sq = Vector3.SqrMagnitude(target - eval);
        var in_err_sq = deltas.Aggregate(0.0f, (acc, delta) => acc + Vector3.SqrMagnitude(delta));
        var C = out_err_sq + in_err_sq;
        //Debug.Log(errs.Aggregate((int)(C * 10.0f)/10.0f + ": ", (acc, v) => acc + v + " ") + "-> " + (target - f));
        return C;
    }
}