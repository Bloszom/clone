using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace pcea.Helpers
{
    public class AuditTrail
    {
        private readonly IWebHostEnvironment environment;

        public AuditTrail(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        public void LogToFile(LogItem logItem)
        {
            var path = Path.Combine(environment.WebRootPath, "log");
            var filename = $"{logItem.USER_TYPE}_{logItem.NAME}_{logItem.USER_ID}.json";
            var filePath = Path.Combine(path, filename);

            if (!File.Exists(filePath))
            {
                FileStream fs = File.Create(filePath);
                fs.Close();
            }
            /*// Read existing json data
            var jsonData = File.ReadAllText(filePath);
            // De-serialize to object or create new list
            var logItems = JsonConvert.DeserializeObject<List<LogItem>>(jsonData) ?? new List<LogItem>();

            // Add any new employees
            logItems.Add(logItem);*/

            // Update json data string
            var jsonData = JsonConvert.SerializeObject(logItem);
            System.IO.File.AppendAllTextAsync(filePath, jsonData + ",");
        }

        /*public List<LogItem> ReadLogFromFile(int day, int month, int year)
        {
            var path = Path.Combine(environment.WebRootPath, "log");
            var filename = $"{logItem.NAME}_{logItem.USER_TYPE}_{logItem.USER_ID}.json";
            var filePath = Path.Combine(path, filename);
     
            if (File.Exists(filePath))
            {
                // Read existing json data
                var jsonData = System.IO.File.ReadAllText(filePath);
                // De-serialize to object or create new list
                var logItems = JsonConvert.DeserializeObject<List<LogItem>>(jsonData) ?? new List<LogItem>();
                return logItems;
            }
            return new List<LogItem>();
        }*/

        public List<LogItem> ReadLogFile(string filename)
        {
            var path = Path.Combine(environment.WebRootPath, "log");
            var filePath = Path.Combine(path, filename);

            if (File.Exists(filePath))
            {
                // Read existing json data
                var jsonData = System.IO.File.ReadAllText(filePath);
                if(jsonData.EndsWith(","))
                {
                    jsonData = jsonData.Substring(0, jsonData.Length - 1);
                    jsonData = "[" + jsonData + "]";
                }
                
                // De-serialize to object or create new list
                var logItems = JsonConvert.DeserializeObject<List<LogItem>>(jsonData) ?? new List<LogItem>();
                return logItems.OrderByDescending(x => x.ACTIVITY_TIME).ToList();
            }
            return new List<LogItem>();
        }
    }

    public class LogItem
    {
        public int ID { get; set; }
        public string USER_ID { get; set; }
        public string NAME { get; set; }
        public string OPERATORNAME { get; set; }
        public string ACCESSED_MODULE { get; set; }
        public string OLD_VALUE { get; set; }
        public string NEW_VALUE { get; set; }
        public string ACTIVITY_DETAIL { get; set; }
        public DateTime ACTIVITY_TIME { get; set; }
        public string USER_TYPE { get; set; }
    }

}
