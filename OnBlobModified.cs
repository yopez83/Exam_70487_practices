using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace AzurePractices.Function
{
    public static class OnBlobModified
    {
        [FunctionName("OnBlobModified")]
        public static void Run([BlobTrigger("blob-container-example/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            dynamic messageDTO = new 
            {
                fromEmail = "yopez83@hotmail.com",
                toEmail = "e310548@miamidade.gov",
                subject = "Blob Modified",
                message = $"A blob has been modified.\n Name: {name}\n Size: {myBlob.Length} Bytes.",
                isImportant = true
            };

            var jsonMessageDTO = JsonConvert.SerializeObject(messageDTO);

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));

            // Create the queue client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue.
            CloudQueue queue = queueClient.GetQueueReference("myqueue-items");

            // Create the queue if it doesn't already exist.
            queue.CreateIfNotExistsAsync();

            // Create a message and add it to the queue.
            CloudQueueMessage message = new CloudQueueMessage(jsonMessageDTO);
            queue.AddMessageAsync(message);
        }
    }
}
