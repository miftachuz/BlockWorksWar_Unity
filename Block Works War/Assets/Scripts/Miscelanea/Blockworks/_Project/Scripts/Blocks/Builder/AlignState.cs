namespace Blocks.Builder
{
    public enum AlignState
    {
        NoConnections,
        BlockingWithItself,
        SocketsTooFar,
        OneSocketAligned,
        Aligned
    }
}