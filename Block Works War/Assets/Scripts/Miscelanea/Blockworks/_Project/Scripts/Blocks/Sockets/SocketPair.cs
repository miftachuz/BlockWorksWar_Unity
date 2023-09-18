namespace Blocks.Sockets
{
    public struct SocketPair
    {
        public Socket This;
        public Socket Other;

        public void Deconstruct(out Socket thisSocket, out Socket otherSocket)
        {
            thisSocket = This;
            otherSocket = Other;
        }
    }
}