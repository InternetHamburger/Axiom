using Axiom.src.core.Board;
using Axiom.src.core.Move_Generation;
using Axiom.src.core.Utility;
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

        public short[][] HlWeightMatrix;
        public short[] HlBiasVector;
        public short[] OutputWeightVectorThem;

        public short[] StmAccumulator;
        public short[] NstmAccumulator;

        public short[] OutputWeightVectorUs;
        public short OutputBias;
        public NeuralNetwork(int inputSize, int hlSize)
        {
            StmAccumulator = new short[hlSize];
            NstmAccumulator = new short[hlSize];

            HlWeightMatrix = new short[inputSize][];
            for (int i = 0; i < inputSize; i++)
            {
                HlWeightMatrix[i] = new short[hlSize];
            }
            HlBiasVector = new short[hlSize];

            OutputWeightVectorThem = new short[hlSize];
            OutputWeightVectorUs = new short[hlSize];
            
            this.inputSize = inputSize;
            this.hlSize = hlSize;
        }

        public static short ActivationFunction(short x)
        {
            double a = x;
            return (short)Math.Pow(Math.Clamp(a, 0, QA), 2);
        }

        public int GetOutput()
        {
            short[] accumulatorUs = StmAccumulator;
            short[] accumulatorThem = NstmAccumulator;
            for (int i = 0; i < hlSize; i++)
            {
                accumulatorUs[i] = (short)(ActivationFunction(accumulatorUs[i]));
                accumulatorThem[i] = (short)(ActivationFunction(accumulatorThem[i]));
            }
            for (int i = 0; i < 16; i++)
            {
                Console.WriteLine(accumulatorThem[i] + " | " + NstmAccumulator[i]);
            }

            int output = MatrixHelper.OutputMatrixVectorMultiplication(OutputWeightVectorUs, accumulatorUs);
            output += MatrixHelper.OutputMatrixVectorMultiplication(OutputWeightVectorThem, accumulatorThem);
            
            Console.WriteLine(output);
            output /= QA;
            output += OutputBias;
            return output * EvalScale / (QA * QB);
        }

        public void AddFeature(int piece, int square)
        {
            square = BoardUtility.FlipSquare(square);
            for (int i = 0; i < hlSize; i++)
            {
                
                StmAccumulator[i] += HlWeightMatrix[PreComputedMoveData.NNInputIndicies[0, piece, square]][i];
                NstmAccumulator[i] += HlWeightMatrix[PreComputedMoveData.NNInputIndicies[1, piece, square]][i];
            }
            Console.WriteLine(PreComputedMoveData.NNInputIndicies[1, piece, square]);
        }

        public void RemoveFeature(int piece, int square)
        {
            for (int i = 0; i < hlSize; i++)
            {
                StmAccumulator[i] -= HlWeightMatrix[PreComputedMoveData.NNInputIndicies[0, piece, square]][i];
                NstmAccumulator[i] -= HlWeightMatrix[PreComputedMoveData.NNInputIndicies[1, piece, square]][i];
            }
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
                    HlWeightMatrix[i][j] = (short)weights[hlSize * i + j];
                }
            }
            for (int i = 0; i < hlSize; i++)
            {
                HlBiasVector[i] = (short)weights[inputSize * hlSize + i];
                StmAccumulator[i] = HlBiasVector[i];
                NstmAccumulator[i] = HlBiasVector[i];
            }
            for (int i = 0; i < hlSize; i++)
            {
                OutputWeightVectorUs[i] = (short)weights[(inputSize + 1) * hlSize + i];
            }
            for (int i = 0; i < hlSize; i++)
            {
                OutputWeightVectorThem[i] = (short)weights[(inputSize + 2) * hlSize + i];
            }
            OutputBias = (short)weights[(inputSize + 3) * hlSize];
        }
    }
}