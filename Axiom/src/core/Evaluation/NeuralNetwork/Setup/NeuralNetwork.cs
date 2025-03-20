using Newtonsoft.Json;
using System.Text.Json;

namespace Nerual_Network.Setup
{
    public class NeuralNetwork
    {
        private Layer[] Layers;
        public int inputSize;
        public int outputSize;
        public NeuralNetwork(params int[] layerSizes)
        {
            Layers = new Layer[layerSizes.Length - 1];
            for (int i = 1; i < layerSizes.Length; i++)
            {
                Layers[i - 1] = new Layer(layerSizes[i - 1], layerSizes[i], i==1);
            }
            inputSize = layerSizes[0];
            outputSize = layerSizes[^1];
        }

        public static double ActivationFunction(double x)
        {
            return Math.Pow(Math.Clamp(x, 0, 1), 2);
            return Math.Max(x, 0.01 * x);
            return 1 / (1 + Math.Exp(-x));
        }

        public static double InverseSigmoid(double x)
        {
            return 400 * Math.Log10(x / (1 - x));
        }

        public double GetOutput(double[] input, bool WhiteToMove)
        {
            double[] layerFeed = input;

            for (int i = 0; i < Layers.Length; i++)
            {
                layerFeed = FeedSingleLayer(i, layerFeed, i == Layers.Length - 1);
            }
            return InverseSigmoid(Math.Clamp(layerFeed[0], 0, 1)) * (WhiteToMove ? 1 : -1);
        }


        private double[] FeedSingleLayer(int index, double[] inputs, bool isLastLayer)
        {
            Layers[index].Inputs = inputs;
            double[] output = new double[Layers[index].layerSize];
            if (!isLastLayer)
            {
                output = MatrixHelper.InputMatrixVectorMultiplication(Layers[index].WeightMatrix, inputs);
            }
            else
            {
                output = MatrixHelper.MatrixVectorMultiplication(Layers[index].WeightMatrix, inputs);
            }
            
            MatrixHelper.VectorAddition(output, Layers[index].BiasVector);
            if (!isLastLayer)
            {
                MatrixHelper.ApplyActivationFunction(output);
            }
            return output;
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
                    Layers[0].WeightMatrix[i][j] = (double)floats[hlSize * i + j];
                }
            }
            for (int i = 0; i < 2 * hlSize; i++)
            {
                Layers[0].BiasVector[i] = (double)floats[inputSize * hlSize + i];
            }
            for (int i = 0; i < hlSize; i++)
            {
                Layers[1].WeightMatrix[i][0] = (double)floats[(inputSize + 2) * hlSize + i];
            }
            Layers[1].BiasVector[0] = (double)floats[(inputSize + 3) * hlSize];
        }
    }
}