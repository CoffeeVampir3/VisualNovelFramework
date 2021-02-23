using VisualNovelFramework.GraphFramework.GraphRuntime;

namespace VisualNovelFramework.GraphFramework.GraphExperimentalEditor.NodeIO
{
    /// <summary>
    /// Simple class that conducts the key exchange for 
    /// </summary>
    [System.Serializable]
    public class Connection
    {
        private readonly ValuePort localPort;
        private readonly ValuePort connectedPort;
        private readonly RuntimeNode connectedNode;
        public Connection(ValuePort local, RuntimeNode connectedNode, ValuePort remote)
        {
            localPort = local;
            connectedPort = remote;
            this.connectedNode = connectedNode;
        }
        public void BindConnection()
        {
            localPort.valueKey = connectedPort;
            connectedPort.valueKey = localPort;
        }
    }
}