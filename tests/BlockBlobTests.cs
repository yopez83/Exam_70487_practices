using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class BlockBlobTests
    {
        private string StorageConnection { get; set; }

        [SetUp]
        public void TestSetup()
        {
            var appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            StorageConnection = appConfig.AppSettings.Settings["storageConnection"]?.Value;
        }

        [TestCase("This is a test document", "yiny.txt", "blob-container-example")]
        public async Task CreateBlockBlobFromConfig(string fileContent, string fileName, string containerName)
        {
            //ARRANGE
            CloudStorageAccount account = CloudStorageAccount.Parse(StorageConnection);
            CloudBlobClient client = account.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(containerName);

            //ACT
            await container.CreateIfNotExistsAsync();
            ICloudBlob blockBlob = container.GetBlockBlobReference(fileName);
            using(MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent)))
            {
                await blockBlob.UploadFromStreamAsync(stream);
            }

            //ASSERT
            Assert.That(blockBlob.Properties.Length, Is.EqualTo(fileContent.Length));
        }

        [TestCase("yiny.txt", "blob-container-example")]
        public async Task DeleteBlockBlobFromConfigAsync(string fileName, string containerName)
        {
            //ARRANGE
            CloudStorageAccount account = CloudStorageAccount.Parse(StorageConnection);
            CloudBlobClient client = account.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(containerName);

            //ACT
            await container.CreateIfNotExistsAsync();
            ICloudBlob blockBlob = container.GetBlockBlobReference(fileName);
            bool isDeleted = await blockBlob.DeleteIfExistsAsync();

            //ASSERT
            Assert.That(true, Is.True);
        }
    }
}