using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Text.Json;

namespace Nerual_Network.Setup
{
    public class NeuralNetwork
    {

        public int inputSize;
        public int hlSize;
        public const int outputSize = 1;
        public const int EvalScale = 400;

        public double[][] HlWeightMatrix;
        public double[] HlBiasVector;
        public double[] OutputWeightVectorThem;

        public double[] OutputWeightVectorUs;
        public double OutputBias;
        public NeuralNetwork(int inputSize, int hlSize)
        {
            HlWeightMatrix = new double[hlSize][];
            for (int i = 0; i < hlSize; i++)
            {
                HlWeightMatrix[i] = new double[inputSize];
            }
            HlBiasVector = new double[hlSize];

            OutputWeightVectorThem = new double[hlSize];
            OutputWeightVectorUs = new double[hlSize];
            
            this.inputSize = inputSize;
            this.hlSize = hlSize;
        }

        public static double ActivationFunction(double x)
        {
            return Math.Pow(Math.Clamp(x, 0, 1), 1);
            return Math.Max(x, 0.01 * x);
            return 1 / (1 + Math.Exp(-x));
        }

        public double GetOutput(double[] input, bool WhiteToMove)
        {

            double[] usAccumulator = MatrixHelper.InputMatrixVectorMultiplication(HlWeightMatrix, input);
            double[] themAccumulator = MatrixHelper.InputMatrixVectorMultiplication(HlWeightMatrix, input);
            MatrixHelper.VectorAddition(usAccumulator, HlBiasVector);
            MatrixHelper.VectorAddition(themAccumulator, HlBiasVector);
            MatrixHelper.ApplyActivationFunction(usAccumulator);
            MatrixHelper.ApplyActivationFunction(themAccumulator);

            double output = MatrixHelper.OutputMatrixVectorMultiplication(OutputWeightVectorUs, usAccumulator);
            output += MatrixHelper.OutputMatrixVectorMultiplication(OutputWeightVectorThem, themAccumulator);
            output += OutputBias;
            return (WhiteToMove ? 1 : -1) * output * EvalScale;
        }

        public void LoadFromFile(string filePath, int hlSize)
        {
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist.");
            }

            byte[] bytes = File.ReadAllBytes(filePath);

            int floatCount = bytes.Length / 4;
            float[] floats = new float[floatCount];
            
            // Load the weights and biases
            for (int i = 0; i < floatCount; i++)
            {
                floats[i] = BitConverter.ToSingle(bytes, i * 4);
            }
            for (int i = 0; i < inputSize; i++)
            {
                for (int j = 0; j < hlSize; j++) 
                {
                    HlWeightMatrix[j][i] = (double)floats[hlSize * i + j];
                }
            }
            for (int i = 0; i < hlSize; i++)
            {
                HlBiasVector[i] = (double)floats[inputSize * hlSize + i];
            }
            for (int i = 0; i < hlSize; i++)
            {
                OutputWeightVectorUs[i] = (double)floats[(inputSize + 1) * hlSize + i];
            }
            for (int i = 0; i < hlSize; i++)
            {
                OutputWeightVectorThem[i] = (double)floats[(inputSize + 2) * hlSize + i];
            }
            OutputBias = (double)floats[(inputSize + 3) * hlSize];
        }
    }
}