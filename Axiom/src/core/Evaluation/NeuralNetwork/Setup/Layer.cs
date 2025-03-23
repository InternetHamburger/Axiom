
namespace Nerual_Network.Setup
{
    struct Layer
    {
        public double[][] WeightMatrix;
        public double[] BiasVector;

        public double[] Inputs;
        public int layerSize;
        public int inputSize;

        public Layer(int inputSize, int layerSize, bool isHiddenLayer)
        {
            this.layerSize = layerSize;
            this.inputSize = inputSize;
            WeightMatrix = new double[inputSize][];
            
            
            for (int i = 0; i < inputSize; i++)
            {
                WeightMatrix[i] = new double[layerSize];
            }

            Inputs = new double[inputSize];
            BiasVector = new double[layerSize * (isHiddenLayer ? 2 : 1)];
        }
    }
}
