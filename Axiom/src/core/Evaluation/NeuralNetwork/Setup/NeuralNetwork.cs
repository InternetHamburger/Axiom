using Axiom.src.core.Move_Generation;
using Axiom.src.core.Utility;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Axiom.src.core.Evaluation.NeuralNetwork.Setup
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

        public static void ActivationFunction(int[] accUs, int[] accThem, int[] fromUs, int[] fromThem)
        {
            int simdWidth = Vector<int>.Count;

            Vector<int> zero = Vector<int>.Zero;
            Vector<int> qaVec = new Vector<int>(QA);

            int i = 0;
            for (; i <= fromUs.Length - simdWidth; i += simdWidth)
            {
                // Load vectors
                var vUs = new Vector<int>(fromUs, i);
                var vThem = new Vector<int>(fromThem, i);

                // Clamp to [0, QA]
                vUs = Vector.Min(Vector.Max(vUs, zero), qaVec);
                vThem = Vector.Min(Vector.Max(vThem, zero), qaVec);

                // Square the clamped values
                var vUsSq = vUs * vUs;
                var vThemSq = vThem * vThem;

                // Store back
                vUsSq.CopyTo(accUs, i);
                vThemSq.CopyTo(accThem, i);
            }

            // Handle remaining elements
            for (; i < fromUs.Length; i++)
            {
                int us = Math.Clamp(fromUs[i], 0, QA);
                int them = Math.Clamp(fromThem[i], 0, QA);
                accUs[i] = us * us;
                accThem[i] = them * them;
            }
        }

        public int GetOutput(bool WhiteToMove)
        {
            int[] accUs = new int[hlSize];
            int[] accThem = new int[hlSize];

            int[] fromUs = WhiteToMove ? StmAccumulator : NstmAccumulator;
            int[] fromThem = WhiteToMove ? NstmAccumulator : StmAccumulator;

            ActivationFunction(accUs, accThem, fromUs, fromThem);

            int output = MatrixHelper.OutputMatrixVectorMultiplication(OutputWeightVectorUs, accUs)
                       + MatrixHelper.OutputMatrixVectorMultiplication(OutputWeightVectorThem, accThem);

            output = (output / QA) + OutputBias;

            return output * EvalScale / (QA * QB);


        }


        public void AddFeature(int piece, int square)
        {
            if (piece == 0) return;
            square = BoardUtility.FlipSquare(square);

            int idx0 = PreComputedMoveData.NNInputIndicies[0, piece, square];
            int idx1 = PreComputedMoveData.NNInputIndicies[1, piece, square];

            ApplyFeature(HlWeightMatrix[idx0], HlWeightMatrix[idx1], StmAccumulator, NstmAccumulator, true);
        }

        public void RemoveFeature(int piece, int square)
        {
            if (piece == 0) return;
            square = BoardUtility.FlipSquare(square);

            int idx0 = PreComputedMoveData.NNInputIndicies[0, piece, square];
            int idx1 = PreComputedMoveData.NNInputIndicies[1, piece, square];

            ApplyFeature(HlWeightMatrix[idx0], HlWeightMatrix[idx1], StmAccumulator, NstmAccumulator, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ApplyFeature(short[] weights0, short[] weights1, int[] acc0, int[] acc1, bool add)
        {
            int simdWidth = Vector<short>.Count;

            ref short w0Ref = ref MemoryMarshal.GetArrayDataReference(weights0);
            ref short w1Ref = ref MemoryMarshal.GetArrayDataReference(weights1);
            ref int acc0Ref = ref MemoryMarshal.GetArrayDataReference(acc0);
            ref int acc1Ref = ref MemoryMarshal.GetArrayDataReference(acc1);

            for (int i = 0; i < weights0.Length; i += simdWidth)
            {
                // Load short vectors
                var w0Short = Unsafe.ReadUnaligned<Vector<short>>(ref Unsafe.As<short, byte>(ref Unsafe.Add(ref w0Ref, i)));
                var w1Short = Unsafe.ReadUnaligned<Vector<short>>(ref Unsafe.As<short, byte>(ref Unsafe.Add(ref w1Ref, i)));

                // Widen to int vectors
                Vector.Widen(w0Short, out var w0Lo, out var w0Hi);
                Vector.Widen(w1Short, out var w1Lo, out var w1Hi);

                // Load accumulators
                var acc0Lo = Unsafe.ReadUnaligned<Vector<int>>(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref acc0Ref, i)));
                var acc0Hi = Unsafe.ReadUnaligned<Vector<int>>(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref acc0Ref, i + simdWidth / 2)));
                var acc1Lo = Unsafe.ReadUnaligned<Vector<int>>(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref acc1Ref, i)));
                var acc1Hi = Unsafe.ReadUnaligned<Vector<int>>(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref acc1Ref, i + simdWidth / 2)));

                // Add or subtract
                if (add)
                {
                    acc0Lo += w0Lo;
                    acc0Hi += w0Hi;
                    acc1Lo += w1Lo;
                    acc1Hi += w1Hi;
                }
                else
                {
                    acc0Lo -= w0Lo;
                    acc0Hi -= w0Hi;
                    acc1Lo -= w1Lo;
                    acc1Hi -= w1Hi;
                }

                // Store back
                Unsafe.WriteUnaligned(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref acc0Ref, i)), acc0Lo);
                Unsafe.WriteUnaligned(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref acc0Ref, i + simdWidth / 2)), acc0Hi);
                Unsafe.WriteUnaligned(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref acc1Ref, i)), acc1Lo);
                Unsafe.WriteUnaligned(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref acc1Ref, i + simdWidth / 2)), acc1Hi);
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
                        HlWeightMatrix[i][j] = weights[(hlSize * i) + j];
                    }
                }
                for (int i = 0; i < hlSize; i++)
                {
                    HlBiasVector[i] = weights[(inputSize * hlSize) + i];
                    StmAccumulator[i] = HlBiasVector[i];
                    NstmAccumulator[i] = HlBiasVector[i];
                }
                for (int i = 0; i < hlSize; i++)
                {
                    OutputWeightVectorUs[i] = weights[((inputSize + 1) * hlSize) + i];
                }
                for (int i = 0; i < hlSize; i++)
                {
                    OutputWeightVectorThem[i] = weights[((inputSize + 2) * hlSize) + i];
                }
                OutputBias = weights[(inputSize + 3) * hlSize];
            }
        }
    }
}