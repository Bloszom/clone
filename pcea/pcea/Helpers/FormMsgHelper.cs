namespace pcea.Helpers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;

    public class FormMsgHelper
    {

        private JsonSerializer _serializer = new JsonSerializer();
        private IConfiguration _config;
        private HttpClient client;

        public FormMsgHelper(IConfiguration config)
        {
            _config = config;
            client = new HttpClient();
            client.BaseAddress = new Uri("https://messaging.supportdom.com/messagingV1/Mail/SendMail");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> SendMail(string sTo, string sSubject, string sHtmlBody, int sPriority)
        {
            try
            {
                client.DefaultRequestHeaders.Add("ApiKey", _config.GetValue<string>("AppSettings:MessagingApiKey"));
                client.DefaultRequestHeaders.Add("AppId", _config.GetValue<string>("AppSettings:MessagingAppId"));

                var request = new MailMessageRequest();

                request.ToMail = sTo;
                request.Subject = sSubject;
                request.Body = sHtmlBody;
                request.Priority = sPriority;

                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");


                var response = await client.PostAsync(client.BaseAddress, content);
                var str = await response.Content.ReadAsStringAsync();
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                using (var json = new JsonTextReader(reader))
                {
                    var jsoncontent = _serializer.Deserialize<MailResponseBase>(json);
                    if (jsoncontent.StatusCode == 200)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }



    public class MailMessageRequest
    {
        public string ToMail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int Priority { get; set; }
    }
    public class MailMessageResponse
    {
        public string OrderId { get; set; }
    }
    public class MailResponseBase
    {
        public MailMessageResponse Data { get; set; }
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}

