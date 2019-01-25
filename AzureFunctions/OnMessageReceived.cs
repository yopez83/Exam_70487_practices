using System;
using System.Net.Mail;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctions
{
    public static class OnMessageReceived
    {
        [FunctionName("OnMessageReceived")]
        public static void Run([QueueTrigger("myqueue-items", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log)
        {
            log.LogInformation("New queue message received."); 

            dynamic data = JsonConvert.DeserializeObject(myQueueItem);    
            
            MailMessage mail = new MailMessage(data.fromEmail.ToString(), data.toEmail.ToString());
            mail.Subject = data.subject.ToString();    
            if (bool.Parse(data.isImportant.ToString())) 
            {
                mail.Priority = MailPriority.High;
            }    
            mail.Body = data.message.ToString();

            var hotmailAccountPassword = Environment.GetEnvironmentVariable("HOTMAIL_ACCOUNT_PASSWORD", EnvironmentVariableTarget.Process);
            var client = new SmtpClient
            {
                Host = "smtp.live.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential("yopez83@hotmail.com", hotmailAccountPassword),
                Timeout = 20000
            };

            try 
            {
                client.Send(mail);
                log.LogInformation($"Email sent to {data.toEmail}.");
            }
            catch (Exception ex) 
            {
                log.LogError(ex.ToString());
            }
        }
    }
}
