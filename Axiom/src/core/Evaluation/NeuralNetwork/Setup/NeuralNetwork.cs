using Axiom.src.core.Board;
using Axiom.src.core.Move_Generation;
using Axiom.src.core.Utility;
using Nerual_Network.Chess;
using Newtonsoft.Json;
using System;
using System.IO;
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

        public int[] StmAccumulator;
        public int[] NstmAccumulator;

        public short[] OutputWeightVectorUs;
        public short OutputBias;
        public NeuralNetwork(int inputSize, int hlSize)
        {
            StmAccumulator = new int[hlSize];
            NstmAccumulator = new int[hlSize];

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

        public static int ActivationFunction(int x)
        {
            int clamped = Math.Clamp(x, 0, QA);
            return clamped * clamped;
        }

        public int GetOutput(bool WhiteToMove)
        {
            int[] accUs = new int[hlSize];
            int[] accThem = new int[hlSize];

            int[] fromUs = WhiteToMove ? StmAccumulator : NstmAccumulator;
            int[] fromThem = WhiteToMove ? NstmAccumulator : StmAccumulator;

            for (int i = 0; i < hlSize; i++)
            {
                accUs[i] = ActivationFunction(fromUs[i]);
                accThem[i] = ActivationFunction(fromThem[i]);
            }

            int output = MatrixHelper.OutputMatrixVectorMultiplication(OutputWeightVectorUs, accUs)
                       + MatrixHelper.OutputMatrixVectorMultiplication(OutputWeightVectorThem, accThem);

            output = output / QA + OutputBias;

            return output * EvalScale / (QA * QB);
        }


        public void AddFeature(int piece, int square)
        {
            if (piece == 0) return;
            square = BoardUtility.FlipSquare(square);

            int idx0 = PreComputedMoveData.NNInputIndicies[0, piece, square];
            int idx1 = PreComputedMoveData.NNInputIndicies[1, piece, square];

            short[] weights0 = HlWeightMatrix[idx0];
            short[] weights1 = HlWeightMatrix[idx1];

            for (int i = 0; i < hlSize; i++)
            {
                StmAccumulator[i] += weights0[i];
                NstmAccumulator[i] += weights1[i];
            }
        }

        public void RemoveFeature(int piece, int square)
        {
            if (piece == 0) return;
            square = BoardUtility.FlipSquare(square);

            int idx0 = PreComputedMoveData.NNInputIndicies[0, piece, square];
            int idx1 = PreComputedMoveData.NNInputIndicies[1, piece, square];

            short[] weights0 = HlWeightMatrix[idx0];
            short[] weights1 = HlWeightMatrix[idx1];

            for (int i = 0; i < hlSize; i++)
            {
                StmAccumulator[i] -= weights0[i];
                NstmAccumulator[i] -= weights1[i];
            }
        }


        public void LoadFromFile(string filePath, int hlSize)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(filePath))
            {

                using BinaryReader reader = new BinaryReader(stream);
                byte[] bytes = reader.ReadBytes((int)stream.Length);

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
}