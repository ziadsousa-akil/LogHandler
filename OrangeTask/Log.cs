using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LogHandler
{
    public enum TransportProtocolType
    {
        None = 0,
        TCP,
        UDP
    }
    public enum ActionType
    {
        None = 0,
        Allow,
        Deny,
        Bypass,
        LogOnly
    }

    public struct Log
    {
        public DateTime DateTime { get; set; }
        public string SourceIP { get; set; }
        public string DestinationIP { get; set; }
        public int Port { get; set; }
        public TransportProtocolType TransportProtocol { get; set; }
        public string Username { get; set; }
        public ActionType Action { get; set; }
        public override string ToString() => $"{DateTime.ToString("yyyy-MM-dd")} {DateTime.ToString("HH-mm-ss")} {SourceIP} {DestinationIP} {Port} {TransportProtocol} {Username} {Action}";
        public static Log Parse(params string[] Parameters)
        {
            Log log = new Log();
            if (Parameters.Length < 2 || System.DateTime.TryParseExact(Parameters[0] + " " + Parameters[1], "yyyy-MM-dd HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime DateTime) == false) log.DateTime = new DateTime();
            else log.DateTime = DateTime;

            if (Parameters.Length < 3 || IPAddress.TryParse(Parameters[2], out IPAddress SourceIP) == false) log.SourceIP = IPAddress.Any.ToString();
            else log.SourceIP = SourceIP.ToString();

            if (Parameters.Length < 4 || IPAddress.TryParse(Parameters[3], out IPAddress DestinationIP) == false) log.DestinationIP = IPAddress.Any.ToString();
            else log.DestinationIP = DestinationIP.ToString();

            if (Parameters.Length < 5 || int.TryParse(Parameters[4], out int Port) == false) log.Port = 0;
            else log.Port = Port;

            if (Parameters.Length < 6 || Enum.TryParse(typeof(TransportProtocolType), Parameters[5], out var TransportProtocolType) == false) log.TransportProtocol = LogHandler.TransportProtocolType.None;
            else log.TransportProtocol = (TransportProtocolType)TransportProtocolType;

            if (Parameters.Length < 7) log.Username = "";
            else log.Username = Parameters[6];

            if (Parameters.Length < 8 || Enum.TryParse(typeof(ActionType), Parameters[7], out var ActionType) == false) log.Action = LogHandler.ActionType.None;
            else log.Action = (ActionType)ActionType;

            return log;
        }
        public static Log Random(int Seed, DateTime dateTime, ActionType action, TransportProtocolType transportProtocol, List<KeyValuePair<string, string>> UsersPair)
        {
            var UserPair = UsersPair[LogHandler.Random.Int(0, UsersPair.Count)];
            return new Log()
            {
                DateTime = dateTime,
                Action = action,
                DestinationIP = LogHandler.Random.IP,
                Port = LogHandler.Random.Int(20, 1024),
                SourceIP = UserPair.Value,
                TransportProtocol = transportProtocol,
                Username = UserPair.Key
            };
        }
    }
    public class LogElement<T> 
    {
        private List<Queue<T>> keyValuePairs = new List<Queue<T>>();
        private Semaphore semaphore = new Semaphore(1, 1);
        public void Add(T elementType,Percentage percentage,long TotalAmount)
        {
            Queue<T> temp = new Queue<T>(Enumerable.Range(0, (int)Math.Ceiling(percentage.TotalValue(TotalAmount))).AsParallel().Select(x => elementType));
            keyValuePairs.Add(temp);
        }
        public void Add(dynamic Start, dynamic End,TimeRange elementType,Percentage percentage,long TotalAmount) 
        {
            if (typeof(T).Equals(typeof(DateTime)) == false) throw new Exception("Time range can only be used with DateTime Type");
            Queue<T> temp = new Queue<T>(Enumerable.Range(0, (int)Math.Ceiling(percentage.TotalValue(TotalAmount))).AsParallel().Select(x => (T)Random.Date(Start,End,elementType)));
            keyValuePairs.Add( temp);
        }
        public T GetNext()
        {
            semaphore.WaitOne();
            try
            {
                T elementType = default(T);
                if (keyValuePairs.Count == 0) return elementType;
                var index = Random.Int(0, keyValuePairs.Count - 1);
                while (keyValuePairs[index].Count == 0) index = Random.Int(0, keyValuePairs.Count - 1);
                elementType = keyValuePairs[index].Dequeue();

                keyValuePairs.RemoveAll(x => x.Count == 0);

                return elementType;
            }
            finally
            {
                semaphore.Release();
            }
          
        }
    }
}
