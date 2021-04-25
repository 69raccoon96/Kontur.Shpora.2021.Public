using System;

namespace ClusterClient
{
    public struct RequestResult
    {
        public string Result { get; }
        public TimeSpan Time { get; }
        
        public string ServerName { get; }
        

        public RequestResult(string serverName, string result = null, TimeSpan time = default)
        {
            Time = time;
            Result = result;
            ServerName = serverName;
        }
    }
}