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

        public void NextStep(double y, double u, double deltaTime)
        {
            NextStep(new DenseVector(1, y), new DenseVector(1, u), deltaTime);
        }

        public void NextStep(double[] y, double[] u, double deltaTime)
        {
            NextStep(new DenseVector(y), new DenseVector(u), deltaTime);
        }

        public void NextStep(DenseVector y, DenseVector u, double deltaTime)
        {
            xh = (A * xh - L * (C * xh - y)) * deltaTime + xh;
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
