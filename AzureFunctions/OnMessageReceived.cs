using System;
using System.Net.Mail;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace AzureFunctions
{
    public class NotificationMessage: TableEntity
    {
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsImportant { get; set; }
        public DateTime NotifiedAt { get; set; }
        public string Comments { get; set; }
    }

    public static class OnMessageReceived
    {
        [FunctionName("OnMessageReceived")]
        [return: Table("blobNotifications", Connection = "AzureWebJobsStorage")]
        public static NotificationMessage Run([QueueTrigger("myqueue-items", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log)
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

            var notification = new NotificationMessage
            {
                PartitionKey = "EmailNotifications",
                RowKey = Guid.NewGuid().ToString(),
                FromEmail = data.fromEmail.ToString(),
                ToEmail = data.toEmail.ToString(),
                Subject = data.subject.ToString(),
                Body = data.message.ToString(),
                IsImportant = bool.Parse(data.isImportant.ToString())
            };

            try 
            {
                client.Send(mail);
                notification.NotifiedAt = DateTime.UtcNow;
                log.LogInformation($"Email sent to {data.toEmail} at {notification.NotifiedAt}.");
            }
            catch (Exception ex) 
            {
                notification.Comments = ex.ToString();
                log.LogError(ex.ToString());                
            }

            return notification;
        }
    }
}
