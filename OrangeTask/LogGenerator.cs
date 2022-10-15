using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OrangeTask
{ 
    public class LogGenerator
    {
      
        /// <summary>
        /// Random Users pool to be used while generating the logs
        /// </summary>
        private ConcurrentDictionary<string, string[]> Users { get; set; }
        /// <summary>
        /// Characters allowed while generating the username
        /// </summary>
        private string UsernameCharacters { get; set; }
        /// <summary>
        /// Initialize new Log Generator
        /// </summary>
        /// <param name="UsernameCharacters">Characters allowed while generating the username</param>
        public LogGenerator(string UsernameCharacters  )
        { 
            Users = new ConcurrentDictionary<string, string[]>();
            this.UsernameCharacters = UsernameCharacters;
        }
        /// <summary>
        /// Fill the users pool with random usernames and IPs
        /// </summary>
        /// <param name="NumberOfUsers">Max number of unique users</param>
        /// <param name="NumberOfIPs">Max number of unique IPs</param>
        /// <param name="MaxIPsPerUser">Max IPs per user</param>
        /// <param name="MinIPsPerUser">Min IPs per user</param>
        private void AddUsers(int NumberOfUsers, int NumberOfIPs, int MaxIPsPerUser,int MinIPsPerUser = 1)
        {
            if (MinIPsPerUser <= 0) throw new Exception("Minimum IPs per users cannot be less than 1");
            //aX+bY = W, bX+bY= bZ where a= Max IPs per User, b= Minimum IPs per User, X= Number of users with Maximum IPs, Y= Number of users with Minimum IPs, W= Number of IPs, Z= Number of Users
            //By subtracting equations (a-b)X = W - bZ but W Must be Bigger than or equal bZ, X = (W-bZ)/(a-b), Y= (W-aX)/b

            if (NumberOfIPs < MinIPsPerUser * NumberOfUsers) throw new Exception("Given number of Users and number of IPs are invalid Number of IPs must be bigger than or equal Number of users multiplied by Minimum IPs per user");



            var X = (NumberOfIPs - (MinIPsPerUser * NumberOfUsers)) / (MaxIPsPerUser - MinIPsPerUser);
            var Y = (NumberOfIPs - (MaxIPsPerUser * X)) / MinIPsPerUser;

            for (int i = 0; i < X; i++) AddUser(  MaxIPsPerUser);
            for (int i = 0; i < Y; i++) AddUser(   MinIPsPerUser);
        }
        /// <summary>
        /// Add Random User to Pool
        /// </summary>
        /// <param name="NumberOfIPs">IPs per User</param>
        public void AddUser( int NumberOfIPs) => Users.TryAdd(Random.Text(8,new Regex("[a-zA-Z][a-zA-Z0-9]+"),UsernameCharacters), Enumerable.Range(0,NumberOfIPs).Select(x=> Random.IP).ToArray() );
        /// <summary>
        /// Generate logs based on the passed parameters 
        /// </summary>
        /// <param name="TotalNumberOfLogs">Total number of logs to generate</param>
        /// <param name="StartDate">Start date of the logs</param>
        /// <param name="EndDate">End date of the logs</param>
        /// <param name="NumberOfUsers">Maximum number of unique users throughout the logs</param>
        /// <param name="NumberOfIPs">Maximum number of unique Source IPs throughout the logs</param>
        /// <param name="MaxIPsPerUser">Maximum number of IPs per User</param>
        /// <param name="TransportProtocolsPercentages">Percentage for each transport protocol available such that each protocol will appear in the logs with volume exactly equal to the precentage. e.g., TCP 40%, All logs will have only 40% TCP traffic</param>
        /// <param name="ActionsPercentages">Percentage for each action available such that each action will appear in the logs with volume exactly equal to the precentage. e.g., Allow 40%, All logs will have only 40% Allow traffic</param>
        /// <param name="TimeOfDayPercentages">Percentage for day time logs and night time logs. e.g., Daytime 40%, All logs will have only 40% Daytime traffic. Daytime= 6AM to 6PM, Nighttime= 6PM to 6AM</param>
        /// <returns>Logs generated</returns>
        /// <exception cref="Exception"></exception>
        public List<Log> Generate(long TotalNumberOfLogs,DateTime StartDate, DateTime EndDate,int NumberOfUsers,int NumberOfIPs,int MaxIPsPerUser,
            Dictionary<TransportProtocolType,Percentage> TransportProtocolsPercentages, Dictionary<ActionType, Percentage> ActionsPercentages,Dictionary<TimeRange,Percentage> TimeOfDayPercentages )
        {
            #region Validation
            if (NumberOfUsers > NumberOfIPs) throw new Exception("Number of IPs cannot be less than the number of Users. Each user must have at least one IP");
            if (TransportProtocolsPercentages.Sum(x => x.Value) != 100) throw new Exception("Transport protocol percentage is not 100% in total");
            if (ActionsPercentages.Sum(x => x.Value) != 100) throw new Exception("Actions percentage is not 100% in total");
            #endregion

            EndDate = EndDate.AddHours(-12); //Randomization of date will always exceed the last day by 12 hours. **Design limitation to avoid over complication for the task

            #region Fill Randomization Pools
            AddUsers(NumberOfUsers, NumberOfIPs, MaxIPsPerUser);
            
            LogElement<ActionType> actionLogElements = new LogElement<ActionType>();
            LogElement<TransportProtocolType> transportLogElements = new LogElement<TransportProtocolType>();
            LogElement<DateTime> dateTimeLogElements = new LogElement<DateTime>();

            Parallel.ForEach(ActionsPercentages, element => actionLogElements.Add(element.Key, element.Value, TotalNumberOfLogs));
            Parallel.ForEach(TransportProtocolsPercentages, element => transportLogElements.Add(element.Key, element.Value, TotalNumberOfLogs));
            Parallel.ForEach(TimeOfDayPercentages, element => dateTimeLogElements.Add(StartDate,EndDate,element.Key, element.Value, TotalNumberOfLogs));

            var Users = this.Users.AsParallel().SelectMany(x => x.Value.Select(y=> new KeyValuePair<string,string>(x.Key,y)).ToList()).ToList();
            #endregion

            #region Generate
            List<Log> Logs = new List<Log>();
            for (int i = 0; i < TotalNumberOfLogs; i++)
            {
                var log = Log.Random(i, dateTimeLogElements.GetNext(), actionLogElements.GetNext(), transportLogElements.GetNext(), Users);
                Logs.Add(log); 
            }
            #endregion
            return Logs;
        }
        /// <summary>
        /// Call to initiate the generate logs wizard in Console
        /// </summary>
        public static void RunGenerateWizard()
        {
            #region Variables 

            long TotalNumberOfLogs;
            int TotalNumberOfUsers, TotalNumberOfIPs, MaxIPsPerUser;
            DateTime StartDate, EndDate;
            Dictionary<TransportProtocolType, Percentage> TransportPercentages = new Dictionary<TransportProtocolType, Percentage>();
            Dictionary<ActionType, Percentage> ActionsPercentages = new Dictionary<ActionType, Percentage>();
            Dictionary<TimeRange, Percentage> TimeOfDayPercentages = new Dictionary<TimeRange, Percentage>();
            Percentage DayTimePercentage = new Percentage(0), NighttimePercentage = new Percentage(0);
            #endregion

            Console.WriteLine("### Generate Logs Application ###");

            #region Gathering Information
            Console.WriteLine("> Please enter total number of logs:");
            while (long.TryParse(Console.ReadLine(), out TotalNumberOfLogs) == false) Console.WriteLine("Value must be in correct number format, Please enter total number of logs:");

            Console.WriteLine("> Please enter the start date of the logs in this format (dd/MM/yyyy HH:mm:ss):");
            while (DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out StartDate) == false) Console.WriteLine("Please enter the start date of the logs in this format (dd/MM/yyyy):");

            Console.WriteLine("> Please enter the end date of the logs in this format (dd/MM/yyyy HH:mm:ss):");
            while (DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out EndDate) == false) Console.WriteLine("Please enter the end date of the logs in this format (dd/MM/yyyy):");

            Console.WriteLine("> Please enter total number of unique users:");
            while (int.TryParse(Console.ReadLine(), out TotalNumberOfUsers) == false) Console.WriteLine("Value must be in correct number format, Please enter total number of unique users:");

            Console.WriteLine("> Please enter total number of unique IPs:");
            while (int.TryParse(Console.ReadLine(), out TotalNumberOfIPs) == false) Console.WriteLine("Value must be in correct number format, Please enter total number of unique IPs:");

            Console.WriteLine("> Please enter the max IPs per user:");
            while (int.TryParse(Console.ReadLine(), out MaxIPsPerUser) == false) Console.WriteLine("Value must be in correct number format, Please enter the max IPs per user:");

            foreach (string actionType in Enum.GetNames(typeof(ActionType)))
            {
                ActionType type = (ActionType)Enum.Parse(typeof(ActionType), actionType);
                Console.WriteLine($"> What is the percentage of {actionType} action type in logs?");
                Percentage actionTypePercentage;
                while (Percentage.TryParse(Console.ReadLine(), out actionTypePercentage, out string Reason) == false) Console.WriteLine($"> {Reason} , What is the percentage of {actionType} action type in logs?");
                ActionsPercentages.Add(type, actionTypePercentage);
            }

            foreach (string transportType in Enum.GetNames(typeof(TransportProtocolType)))
            {
                TransportProtocolType type = (TransportProtocolType)Enum.Parse(typeof(TransportProtocolType), transportType);
                Console.WriteLine($"> What is the percentage of {transportType} transport type in logs?");
                Percentage transportTypePercentage;
                while (Percentage.TryParse(Console.ReadLine(), out transportTypePercentage, out string Reason) == false) Console.WriteLine($"> {Reason} , What is the percentage of {transportType} transport type in logs?");
                TransportPercentages.Add(type, transportTypePercentage);
            }

            while (DayTimePercentage.Value + NighttimePercentage != 100)
            {
                Console.WriteLine($"> What is the percentage of Daytime logs?");
                while (Percentage.TryParse(Console.ReadLine(), out DayTimePercentage, out string Reason) == false) Console.WriteLine($"> {Reason} , What is the percentage of Daytime logs?");

                Console.WriteLine($"> What is the percentage of Nighttime logs?");
                while (Percentage.TryParse(Console.ReadLine(), out NighttimePercentage, out string Reason) == false) Console.WriteLine($"> {Reason} , What is the percentage of Nighttime logs?");

                if (DayTimePercentage.Value + NighttimePercentage != 100) Console.WriteLine("> Daytime and Nighttime must be summed to 100%");
            }
            TimeOfDayPercentages.Add(new TimeRange(new TimeSpan(6, 0, 0), new TimeSpan(18, 0, 0)), DayTimePercentage);
            TimeOfDayPercentages.Add(new TimeRange(new TimeSpan(18, 0, 1), new TimeSpan(5, 59, 59)), NighttimePercentage);
            #endregion


            LogGenerator logGenerator;
            List<Log> Logs;
            try
            {
                logGenerator = new LogGenerator("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Generating Logs...");
                Logs = logGenerator.Generate(TotalNumberOfLogs, StartDate, EndDate, TotalNumberOfUsers, TotalNumberOfIPs, MaxIPsPerUser, TransportPercentages, ActionsPercentages, TimeOfDayPercentages);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            #region Validation
            Console.WriteLine("Logs Generated, Validating Results...");

            CommandLine.Write("Total number of logs generated is", Logs.Count.ToString(), (Logs.Count == TotalNumberOfLogs) ? CommandLine.Status.Pass : CommandLine.Status.Fail);

            var UniqueUsersCount = Logs.Select(x => x.Username).Distinct().Count();
            CommandLine.Write("Number of Unique users", UniqueUsersCount.ToString(), (UniqueUsersCount <= TotalNumberOfUsers) ? CommandLine.Status.Pass : CommandLine.Status.Fail);

            var UniqueIPsCount = Logs.Select(x => x.SourceIP).Distinct().Count();
            CommandLine.Write("Number of Unique IPs", UniqueIPsCount.ToString(), (UniqueIPsCount <= TotalNumberOfIPs) ? CommandLine.Status.Pass : CommandLine.Status.Fail);

            foreach (string actionType in Enum.GetNames(typeof(ActionType)))
            {
                ActionType type = (ActionType)Enum.Parse(typeof(ActionType), actionType);
                if (ActionsPercentages.ContainsKey(type) == false) continue;
                var Percentage = new Percentage(Logs.Where(x => x.Action == type).Count(), TotalNumberOfLogs);
                CommandLine.Write($"Percentage of {actionType} action type in logs is", Percentage.ToString(), (ActionsPercentages.TryGetValue(type, out var value) && value.ToString() == Percentage.ToString()) ? CommandLine.Status.Pass : CommandLine.Status.Fail);
            }



            var LowestDate = Logs.Min(x => x.DateTime);
            CommandLine.Write("The lowest date in logs is", LowestDate.ToString(), (StartDate <= LowestDate) ? CommandLine.Status.Pass : CommandLine.Status.Fail);

            var HighestDate = Logs.Max(x => x.DateTime);
            CommandLine.Write("The highest date in logs is", HighestDate.ToString(), (EndDate >= HighestDate) ? CommandLine.Status.Pass : CommandLine.Status.Fail);

            var MaxIPs = Logs.GroupBy(x => x.Username).ToDictionary(x => x.Key, x => x.Select(y => y.SourceIP).Distinct().Count()).Values.Max();
            CommandLine.Write("Maximum IPs per user is", MaxIPs.ToString(), (MaxIPs <= MaxIPsPerUser) ? CommandLine.Status.Pass : CommandLine.Status.Fail);

            var DayTimeLogs = new Percentage(Logs.Where(x => new TimeRange(new TimeSpan(6, 0, 0), new TimeSpan(18, 0, 0)).isWithinRange(x.DateTime.TimeOfDay)).Count(), TotalNumberOfLogs);
            CommandLine.Write("Logs within Daytime from 6 AM to 6 PM", DayTimeLogs.ToString(), (TimeOfDayPercentages.First().Value.ToString() == DayTimeLogs.ToString()) ? CommandLine.Status.Pass : CommandLine.Status.Fail);

            var NightTimeLogs = new Percentage(Logs.Where(x => new TimeRange(new TimeSpan(18, 0, 1), new TimeSpan(5, 59, 59)).isWithinRange(x.DateTime.TimeOfDay)).Count(), TotalNumberOfLogs);
            CommandLine.Write("Logs within Nighttime from 6 PM to 6 AM", NightTimeLogs.ToString(), (TimeOfDayPercentages.Last().Value.ToString() == NightTimeLogs.ToString()) ? CommandLine.Status.Pass : CommandLine.Status.Fail);
            #endregion

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Please enter the destination path:");
            var DestinationFile = "";

            while (true)
            {
                try
                {
                    DestinationFile = Console.ReadLine();
                    if (DestinationFile == "" || DestinationFile == null)
                    {
                        DestinationFile = Path.Combine(Environment.CurrentDirectory, "Log.txt");
                        Console.WriteLine("Destination path is empty. Setting to default: " + DestinationFile);
                    }
                    if (System.IO.File.Exists(DestinationFile)) System.IO.File.Delete(DestinationFile);

                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Path provided is invalid");
                }

            }
            try
            {
                Console.WriteLine("Writing Logs...");
                System.IO.File.WriteAllLines(DestinationFile, Logs.OrderBy(x => x.DateTime).Select(x => x.ToString()));
                Console.WriteLine("OK!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        /// <summary>
        /// Call to initiate the read logs wizard in Console. Only the requirements in the OBS task has been covered.
        /// </summary>
        public static void RunReadWizard()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Please enter the destination path:");
            var DestinationFile = "";

            while (true)
            {
                try
                {
                    DestinationFile = Console.ReadLine();
                    if (System.IO.File.Exists(DestinationFile) == false) Console.WriteLine("File path provided does not exist");

                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Path provided is invalid");
                }

            }
            Console.WriteLine("Parsing files...");
            var Logs = System.IO.File.ReadAllLines(DestinationFile).Select(x => Log.Parse(x.Split(" "))).ToList();

            Console.WriteLine("> Top 10 Allowed IPs (Latest)");
            foreach (var log in Logs.Where(x => x.Action == ActionType.Allow).OrderByDescending(x => x.DateTime).Take(10)) CommandLine.Write("Source IP:", log.SourceIP, CommandLine.Status.Pass, false);

            Console.WriteLine("> Top 10 Denied Users (Latest)");
            foreach (var log in Logs.Where(x => x.Action == ActionType.Deny).GroupBy(x => x.Username).OrderByDescending(x => x.Count()).Take(10))
            {
                CommandLine.Write("Username:", log.Key, CommandLine.Status.Pass, false);
                Console.WriteLine("> Top 5 Destination for " + log.Key);
                foreach (var userLog in Logs.Where(x => x.Username == log.Key).OrderByDescending(x => x.DateTime).Take(5)) CommandLine.Write("", userLog.DestinationIP, CommandLine.Status.Pass, false);
            }

            var Percentage = new Percentage(Logs.Where(x => x.Action == ActionType.Bypass).Count(), Logs.Count);
            CommandLine.Write($"Percentage of ByPass action type in logs is", Percentage.ToString(), CommandLine.Status.Pass, false);
            Console.WriteLine("Top 5 TCP services");
            var TCPServices = Logs.Where(x => x.Action == ActionType.Bypass && x.TransportProtocol == TransportProtocolType.TCP).OrderByDescending(x => x.DateTime).Take(5);
            foreach (var log in TCPServices) CommandLine.Write("TCP Service:", log.Port.ToString(), CommandLine.Status.Pass, false);
            if (TCPServices.Count() == 0) CommandLine.Write("No TCP Services with", "ByPass action", CommandLine.Status.Fail, false);

            Console.WriteLine("> Top 5 UDP services");
            var UDPServices = Logs.Where(x => x.Action == ActionType.Bypass && x.TransportProtocol == TransportProtocolType.UDP).OrderByDescending(x => x.DateTime).Take(5);
            foreach (var log in UDPServices) CommandLine.Write("UDP Service:", log.Port.ToString(), CommandLine.Status.Pass, false);
            if (UDPServices.Count() == 0) CommandLine.Write("No UDP Services with", "ByPass action", CommandLine.Status.Fail,false);  


            Console.WriteLine("> Top 5 hours within log file duration in terms of unique user count");
            Dictionary<KeyValuePair<DateTime, DateTime>, int> keyValuePairs = new Dictionary<KeyValuePair<DateTime, DateTime>, int>();
            int index = 0;
            while (index + 1 < Logs.Count)
            {
                var log = Logs[index];
                var fiveHourLog = Logs.Skip(index+1).TakeWhile(x => x.DateTime.Subtract(log.DateTime).TotalHours < 5);
                index += fiveHourLog.Count()+1;
                keyValuePairs.Add(new KeyValuePair<DateTime, DateTime>(log.DateTime, fiveHourLog.Max(x => x.DateTime)),fiveHourLog.Select(x=>x.Username).Distinct().Count());
            }
            var maxUsers = keyValuePairs.OrderByDescending(x => x.Value).First();
            CommandLine.Write("Between " + maxUsers.Key.Key.ToString("dd/MM/yyyy HH:mm:ss") + ", " + maxUsers.Key.Value.ToString("dd/MM/yyyy HH:mm:ss") + ". Number of Users:", maxUsers.Value.ToString(), CommandLine.Status.Pass, false);  


            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
