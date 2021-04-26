using System;
using System.Threading.Tasks;

namespace ClusterClient
{
    public class ServerData
    {
        public string Name { get; }
        public int FailedTimes { get; private set; }

        public TimeSpan AverageTimeResponse { get; private set; }

        private int _successCallsCount;
        private TimeSpan _allTimeResponse;

        public ServerData(string name)
        {
            Name = name;
            FailedTimes = 0;
            AverageTimeResponse = new TimeSpan(0);
            _successCallsCount = 0;
            _allTimeResponse = new TimeSpan(0);
        }

        public void UpdateInfo(bool isSuccess, TimeSpan responseTime = default)
        {
            if (isSuccess)
            {
                _successCallsCount++;
                _allTimeResponse += responseTime;
                AverageTimeResponse = _allTimeResponse / _successCallsCount;
            }
            else
            {
                FailedTimes++;
            }
        }
        
    }
}