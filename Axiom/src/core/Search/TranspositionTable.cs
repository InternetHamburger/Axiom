namespace Axiom.src.core.Search
{
    public struct TTEntry
    {
        public int Score;
        public ushort Flag;  // Stores both depth and move flag
        public ushort BestMove; // Store the best move, In case the value can't be used we can search the best move from previous iteration first
        public ulong ZobristHash;

        // Constants for bit masking
        public const ushort DepthMask = 0b1111111111110000;  // 12 bits for depth (up to 4096)
        public const ushort FlagMask = 0b0000000000001111;   // 4 bits for flag

        // Flag types
        public const ushort NullFlag = 0b0000;
        public const ushort ExactFlag = 0b0001;
        public const ushort UpperBoundFlag = 0b0010;
        public const ushort LowerBoundFlag = 0b0011;
        public const ushort KillerMoveFlag = 0b0100;

        // Get depth from the Flag byte
        public readonly int Depth => (Flag & DepthMask) >> 4;

        // Check if entry is null
        public readonly bool IsNull => (Flag & FlagMask) == NullFlag;
        public readonly bool IsExact => (Flag & FlagMask) == ExactFlag;
        public readonly bool IsUpperBound => (Flag & FlagMask) == UpperBoundFlag;
        public readonly bool IsLowerBound => (Flag & FlagMask) == LowerBoundFlag;
        public readonly bool ContainsMove => BestMove != 0;

        // Constructor with score, depth, and flag
        public TTEntry(int score, int depth, ushort flag, ulong ZobristHash)
        {
            Score = score;
            BestMove = 0;
            // Combine depth (shifted) and flag into the Flag byte
            Flag = (ushort)((depth << 4) | (flag & FlagMask));
            this.ZobristHash = ZobristHash;
        }

        public TTEntry(int score, int depth, ushort flag, ushort move, ulong ZobristHash)
        {
            Score = score;
            BestMove = move;
            // Combine depth (shifted) and flag into the Flag byte
            Flag = (ushort)((depth << 4) | (flag & FlagMask));
            this.ZobristHash = ZobristHash;
        }

        // Constructor for initializing a null entry
        public TTEntry(int score, ushort flag, ushort move)
        {
            Score = score;
            BestMove = move;
            Flag = flag;
        }
    }
}
