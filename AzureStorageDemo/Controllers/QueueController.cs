using AzureStorageDemo.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Rabbit.SerializationMaster;
using System;
using System.Configuration;
using System.Web.Mvc;

namespace AzureStorageDemo.Controllers
{
    public class QueueController : Controller
    {
        private readonly CloudBlobClient _blobClient;
        private readonly CloudQueueClient _queueClient;

        public QueueController()
        {
            var account = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureConnection"].ConnectionString);
            _blobClient = account.CreateCloudBlobClient();
            _queueClient = account.CreateCloudQueueClient();
        }

        public ActionResult ProcessQueue(string name)
        {
            var cloudQueue = _queueClient.GetQueueReference(name);
            cloudQueue.CreateIfNotExists();

            var container = _blobClient.GetContainerReference(DateTime.Now.ToString("yyyy-MM"));
            container.CreateIfNotExists();

            while (true)
            {
                var queueMessage = cloudQueue.PeekMessage();
                if (queueMessage == null)
                {
                    break;
                }

                var blob = container.GetAppendBlobReference("Messages");
                if (!blob.Exists())
                {
                    blob.CreateOrReplace();
                }

                var message = queueMessage.AsString.Deserialize<Message>();
                message.ProcessedAt = DateTime.Now;

                blob.AppendText(message.Serialize());

                queueMessage = cloudQueue.GetMessage();
                cloudQueue.DeleteMessage(queueMessage);
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Queue(string uri)
        {
            var cloudQueue = _queueClient.GetQueueReference(uri);
            var vm = new QueueViewModel()
            {
                Uri = cloudQueue.Uri,
                Name = cloudQueue.Name,
                Messages = cloudQueue.ApproximateMessageCount ?? 0
            };
            return View(vm);
        }

        public ActionResult NewMessage()
        {
            var cloudQueue = _queueClient.GetQueueReference(DateTime.Now.ToString("yyyy-MM"));
            cloudQueue.CreateIfNotExists();

            var newMessage = new Message()
            {
                CreatedAt = DateTime.Now,
                Content = "A sample message"
            };

            cloudQueue.AddMessage(new CloudQueueMessage(newMessage.Serialize()));

            return RedirectToAction("Index", "Home");
        }
    }
}