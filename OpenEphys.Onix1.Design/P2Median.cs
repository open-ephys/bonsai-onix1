using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Estimates the running median using the P² algorithm, which maintains five markers at the 0th, 25th,
    /// 50th, 75th, and 100th percentiles. Each call to <see cref="Update"/> runs in O(1) time and requires
    /// O(1) storage regardless of how many observations have been added.
    /// </summary>
    /// <remarks>
    /// Jain, R., and Chlamtac, I. (1985). "The P² algorithm for dynamic calculation of quantiles and
    /// histograms without storing observations." <i>CACM</i> 28(10): 1076–1085.
    /// </remarks>
    internal sealed class P2Median
    {
        const int M = 5;
        readonly float[] q = new float[M]; // quantiles
        readonly float[] dn = { 0f, 0.25f, 0.5f, 0.75f, 1f };
        readonly int[] n = new int[M]; // markers
        int count;

        /// <summary>
        /// Incorporates a new observation into the median estimate.
        /// </summary>
        /// <param name="x">The observed value.</param>
        public void Update(float x)
        {
            if (count < M)
            {
                q[count++] = x;
                if (count == M) 
                { 
                    Array.Sort(q); 
                    for (int i = 0; i < M; i++) n[i] = i + 1; 
                }
                return;
            }
            count++;

            int k;
            if      (x < q[0]) { q[0] = x; k = 0; }
            else if (x < q[1]) k = 0;
            else if (x < q[2]) k = 1;
            else if (x < q[3]) k = 2;
            else if (x <= q[4]) k = 3;
            else               { q[4] = x; k = 3; }

            for (int i = k + 1; i < M; i++) n[i]++;
            for (int i = 1; i <= 3; i++) Adjust(i);
        }

        /// <summary>
        /// Gets the current estimated median. Returns 0 if no observations have been added. Falls back to the
        /// true median of the first five observations until the algorithm is initialized.
        /// </summary>
        public float Median
        {
            get
            {
                if (count == 0) return 0f;
                if (count < M)
                {
                    var tmp = new float[count];
                    Array.Copy(q, tmp, count);
                    Array.Sort(tmp);
                    return tmp[count / 2];
                }
                return q[2];
            }
        }

        void Adjust(int i)
        {
            float desired = 1f + dn[i] * (count - 1);
            float d = desired - n[i];
            if ((d >= 1f && n[i + 1] - n[i] > 1) || (d <= -1f && n[i - 1] - n[i] < -1))
            {
                int sign = d > 0 ? 1 : -1;
                float qp = Parabolic(i, sign);
                // NB: Dont adjust so much that you are going beyond adjacent markers. If that happens revert the linear slope
                q[i] = (q[i - 1] < qp && qp < q[i + 1]) ? qp
                    : q[i] + sign * (q[i + sign] - q[i]) / (n[i + sign] - n[i]); 
                n[i] += sign;
            }
        }

        /// <summary>
        /// Piecewise-parabolic prediction (P²) to update marker height
        /// </summary>
        /// <param name="i">Marker index</param>
        /// <param name="d">Adjustment direction (is the index going to increase or decrease?)</param>
        /// <returns>Updated marker height</returns>
        /// <remarks>
        /// The P² formula assumes that the curve passing through (n[i-1], q[i-1]), (n[i], q[i]), and (n[i+1],
        /// q[i+1]) is a parabola.
        /// </remarks>
        float Parabolic(int i, int d) =>
            q[i] + (float)d / (n[i + 1] - n[i - 1]) * (
                (n[i] - n[i - 1] + d) * (q[i + 1] - q[i]) / (n[i + 1] - n[i]) +
                (n[i + 1] - n[i] - d) * (q[i] - q[i - 1]) / (n[i] - n[i - 1]));
    }
}
