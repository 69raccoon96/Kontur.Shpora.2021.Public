using System;
using System.Threading;

namespace ReaderWriterLock
{
    public class RwLock : IRwLock
    {
        private int readers;
        private int writers;
        private object locker;

        public RwLock()
        {
            readers = 0;
            writers = 0;
            locker = new object();
        }
        public void ReadLocked(Action action)
        {
            lock (locker)
            {
                while (writers > 0)
                    Monitor.Wait(locker);
                readers++;
            }
            action.Invoke();
            lock (locker)
            {
                readers--;
                if (readers == 0)
                    Monitor.PulseAll(locker);
            }
        }

        public void WriteLocked(Action action)
        {
            lock (locker)
            {
                while (readers > 0 && writers > 0)
                    Monitor.Wait(locker);
                writers++;
                
            }
            action.Invoke();
            lock (locker)
            {
                writers--;
                if(readers == 0 && writers == 0)
                    Monitor.PulseAll(locker);
            }
        }
    }
}