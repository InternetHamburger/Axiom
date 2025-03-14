using Nerual_Network.Setup;

namespace Nerual_Network
{
    static class MatrixHelper
    {
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

        public static void VectorAddition(double[] vector, double[] vector2)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] += vector2[i];
            }
        }

        public static void ApplyActivationFunction(double[] vector)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = NeuralNetwork.ActivationFunction(vector[i]);
            }
        }
    }
}
