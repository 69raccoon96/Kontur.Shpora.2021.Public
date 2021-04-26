using System;

namespace ClusterClient
{
    public struct RequestResult
    {
        public string Result { get; }
        public TimeSpan Time { get; }
        public string ServerName { get; set; }
        public bool IsSuccess { get; }


        public RequestResult(string serverName, bool isSuccess = false, string result = null, TimeSpan time = default)
        {
            Time = time;
            Result = result;
            ServerName = serverName;
            IsSuccess = isSuccess;
        }
    }
}