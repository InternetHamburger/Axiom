
namespace Nerual_Network.Setup
{
    struct Layer
    {
        public double[][] WeightMatrix;
        public double[] BiasVector;

        public double[] Inputs;
        public int layerSize;
        public int inputSize;

        public Layer(int inputSize, int layerSize)
        {
            this.layerSize = layerSize;
            this.inputSize = inputSize;
            WeightMatrix = new double[inputSize][];
            
            
            for (int i = 0; i < inputSize; i++)
            {
                WeightMatrix[i] = new double[layerSize];
            }

            Inputs = new double[inputSize];
            BiasVector = new double[layerSize];
        }

        

        public void Randomize()
        {
            var r = new Random(96);
            for (int i = 0; i < layerSize; i++)
            {
                for (int j = 0; j < inputSize; j++)
                {
                    WeightMatrix[j][i] = r.NextDouble() * 4 - 2;
                }
                BiasVector[i] = r.NextDouble() * 4 - 2;
            }
        }
    }
}
