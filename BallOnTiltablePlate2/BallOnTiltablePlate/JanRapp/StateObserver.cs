using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace BallOnTiltablePlate.JanRapp
{
    class StateObserver
    {
        public DenseVector xh;

        public DenseMatrix A;
        public DenseMatrix B;
        public DenseMatrix C;

        public DenseMatrix L;

        public StateObserver(double[,] A, double[,] B, double[,] C, double[,] L, double[] xInitial)
            : this(new DenseMatrix(A), new DenseMatrix(B), new DenseMatrix(C),
               new DenseMatrix(L), new DenseVector(xInitial))
        { }

        public StateObserver(DenseMatrix A, DenseMatrix B, DenseMatrix C,
            DenseMatrix L, DenseVector xInitial)
        {
            this.A = A;
            this.B = B;
            this.C = C;
            this.L = L;
            this.xh = xInitial;
        }

        DenseVector cache1 = new DenseVector(1);
        DenseVector cache2 = new DenseVector(1);
        public void NextStep(double y, double u, double deltaTime)
        {
            cache1[0] = y;
            cache2[0] = u;
            NextStep(cache1, cache2, deltaTime);
        }

        public void NextStep(double[] y, double[] u, double deltaTime)
        {
            NextStep(new DenseVector(y), new DenseVector(u), deltaTime);
        }

        public void NextStep(DenseVector y, DenseVector u, double deltaTime)
        {
            xh = (A*xh + B*u - L*(C*xh - y)) * deltaTime + xh;
        }

        DenseMatrix Sigma
        {
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
