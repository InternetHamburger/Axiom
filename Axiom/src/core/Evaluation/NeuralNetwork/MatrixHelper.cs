using Nerual_Network.Setup;
using System.Numerics;

namespace Nerual_Network
{
    static class MatrixHelper
    {
        public static double[] InputMatrixVectorMultiplication(double[][] matrix, double[] vector)
        {
            double[] vectorProduct = new double[matrix[0].Length];
            int count = matrix.Count();
            for (int i = 0; i < count; i++)
            {
                if (vector[i] == 0)
                {
                    continue;
                }
                else
                {
                    VectorAddition(vectorProduct, matrix[i]);
                }
            }
            return vectorProduct;
        }
        //public static double[] InputMatrixVectorMultiplication(double[][] matrix, double[] vector)
        //{
        //    int rows = matrix.Length;
        //    int cols = matrix[0].Length;


        //    double[] result = new double[rows];

        //    for (int j = 0; j < rows; j++)
        //    {
        //        if (vector[j] == 1)
        //        {
        //            VectorAddition(result, matrix[j]);
        //        }
        //    }
        //    return result;
        //}
        public static double[] MatrixVectorMultiplication(double[][] matrix, double[] vector)
        {

            double[] vectorProduct = new double[matrix[0].Length];
            for (int i = 0; i < matrix.Count(); i++)
            {
                double[] T = VectorScaling(matrix[i], vector[i]);
                VectorAddition(vectorProduct, T);
            }
            return vectorProduct;
        }

        public static int OutputMatrixVectorMultiplication(short[] matrix, short[] vector)
        {
            int product = 0;
            int count = matrix.Count();

            for (int i = 0; i < count; i++)
            {
                product += matrix[i] * vector[i];
            }
            
            return product;
        }

        public static double[] VectorScaling(double[] vector, double scalar)
        {
            int length = vector.Length;
            double[] scaledVector = new double[length]; 
            // When passing in a jagged array (my matrices) it takes a reference instead of a copy

            for (int i = 0; i < length; i++)
            {
                scaledVector[i] = vector[i] * scalar;
            }

            return scaledVector;
        }

        public static double[] InputVectorScaling(double[] vector, double scalar)
        {
            int length = vector.Length;
            double[] scaledVector = new double[length];
            // When passing in a jagged array (my matrices) it takes a reference instead of a copy

            for (int i = 0; i < length; i++)
            {
                scaledVector[i] = vector[i] * scalar;
            }

            return scaledVector;
        }

        public static void VectorAddition(double[] vector, double[] vector2)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] += vector2[i];
            }
        }
    }
}
