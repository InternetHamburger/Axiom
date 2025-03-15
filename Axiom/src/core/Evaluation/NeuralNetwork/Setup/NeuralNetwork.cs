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
                Layers[i - 1] = new Layer(layerSizes[i - 1], layerSizes[i]);
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
            output = MatrixHelper.MatrixVectorMultiplication(Layers[index].WeightMatrix, inputs);
            MatrixHelper.VectorAddition(output, Layers[index].BiasVector);
            if (!isLastLayer)
            {
                MatrixHelper.ApplyActivationFunction(output);
            }
            return output;
        }

        public void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist.");
            }

            string jsonString = File.ReadAllText(filePath);
            var networkData = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonString);

            if (networkData == null || networkData.Count != Layers.Length)
            {
                throw new Exception("Invalid network data format.");
            }

            for (int i = 0; i < Layers.Length; i++)
            {
                // Deserialize the weight matrix (jagged array)
                var weightList = JsonSerializer.Deserialize<List<List<double>>>(networkData[i]["Weights"].ToString());
                Layers[i].WeightMatrix = weightList.Select(row => row.ToArray()).ToArray();

                // Deserialize the bias vector (1D array)
                Layers[i].BiasVector = JsonSerializer.Deserialize<double[]>(networkData[i]["Biases"].ToString());
            }
        }
    }
}