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
        public const int QA = 255;
        public const int QB = 64;

        public double[][] HlWeightMatrix;
        public double[] HlBiasVector;
        public double[] OutputWeightVectorThem;

        public double[] OutputWeightVectorUs;
        public double OutputBias;
        public NeuralNetwork(int inputSize, int hlSize)
        {
            HlWeightMatrix = new double[inputSize][];
            for (int i = 0; i < inputSize; i++)
            {
                HlWeightMatrix[i] = new double[hlSize];
            }
            HlBiasVector = new double[hlSize];

            OutputWeightVectorThem = new double[hlSize];
            OutputWeightVectorUs = new double[hlSize];
            
            this.inputSize = inputSize;
            this.hlSize = hlSize;
        }

        public static double ActivationFunction(double x)
        {
            return Math.Pow(Math.Clamp(x, 0, QA), 2);
            return Math.Max(x, 0.01 * x);
            return 1 / (1 + Math.Exp(-x));
        }

        public double GetOutput(double[] input, bool WhiteToMove)
        {
            double[] accumulator = MatrixHelper.MatrixVectorMultiplication(HlWeightMatrix, input);
            MatrixHelper.VectorAddition(accumulator, HlBiasVector);
            MatrixHelper.ApplyActivationFunction(accumulator);


            double output = MatrixHelper.OutputMatrixVectorMultiplication(OutputWeightVectorUs, accumulator);
            output += MatrixHelper.OutputMatrixVectorMultiplication(OutputWeightVectorThem, accumulator);
            output += OutputBias;
            return output;
        }

        public void LoadFromFile(string filePath, int hlSize)
        {
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist.");
            }

            byte[] bytes = File.ReadAllBytes(filePath);

            int floatCount = bytes.Length / 2;
            short[] weights = new short[floatCount];
            // Load the weights and biases
            for (int i = 0; i < floatCount; i++)
            {
                weights[i] = BitConverter.ToInt16(bytes, i * 2);
            }
            for (int i = 0; i < inputSize; i++)
            {
                for (int j = 0; j < hlSize; j++) 
                {
                    HlWeightMatrix[i][j] = (double)weights[hlSize * i + j];
                }
            }
            for (int i = 0; i < hlSize; i++)
            {
                HlBiasVector[i] = (double)weights[inputSize * hlSize + i];
            }
            for (int i = 0; i < hlSize; i++)
            {
                OutputWeightVectorUs[i] = (double)weights[(inputSize + 1) * hlSize + i];
            }
            for (int i = 0; i < hlSize; i++)
            {
                OutputWeightVectorThem[i] = (double)weights[(inputSize + 2) * hlSize + i];
            }
            OutputBias = (double)weights[(inputSize + 3) * hlSize];
        }
    }
}