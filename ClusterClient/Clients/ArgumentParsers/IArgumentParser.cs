namespace ClusterClient.Clients.ArgumentParsers
{
    public interface IArgumentParser
    {
        public bool TryGetReplicaAddresses(string[] args, out string[] replicaAddresses);
    }
}