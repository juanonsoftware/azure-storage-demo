using AzureStorageDemo.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AzureStorageDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly CloudBlobClient _blobClient;
        private readonly CloudQueueClient _queueClient;

        public HomeController()
        {
            var account = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureConnection"].ConnectionString);
            _blobClient = account.CreateCloudBlobClient();
            _queueClient = account.CreateCloudQueueClient();
        }

        //
        // GET: /Home/

        public ActionResult Index()
        {
            var container = _blobClient.GetContainerReference(DateTime.Today.ToString("yyyy-MM"));
            container.CreateIfNotExists();

            var containers = _blobClient.ListContainers(detailsIncluded: ContainerListingDetails.Metadata);
            var containersDict = containers.ToDictionary(x => x.Uri, x => x.Name);

            var queues = _queueClient.ListQueues(queueListingDetails: QueueListingDetails.Metadata);
            var queuesDict = queues.ToDictionary(x => x.Uri, x => x.Name);

            var vm = new HomeViewModel()
            {
                Containers = containersDict,
                Queues = queuesDict
            };

            return View(vm);
        }

        public ActionResult Delete(string uri)
        {
            var blob = _blobClient.GetBlobReferenceFromServer(new Uri(uri));
            blob.Delete();

            return RedirectToAction("Index");
        }

        public ActionResult Details(string uri)
        {
            var container = _blobClient.GetContainerReference(uri);
            var blobs = container.ListBlobs(useFlatBlobListing: true);

            var blobsList = new List<BlobViewModel>();
            foreach (var blob in blobs)
            {
                switch (((CloudBlob)blob).BlobType)
                {
                    case BlobType.BlockBlob:
                        var blockBlob = (CloudBlockBlob)blob;
                        blobsList.Add(new BlobViewModel()
                        {
                            Name = blockBlob.Name,
                            Length = blockBlob.Properties.Length,
                            ContentType = blockBlob.Properties.ContentType,
                            Uri = blockBlob.Uri
                        });
                        break;

                    case BlobType.PageBlob:
                        var pageBlob = (CloudPageBlob)blob;
                        blobsList.Add(new BlobViewModel()
                        {
                            Name = pageBlob.Name,
                            Length = pageBlob.Properties.Length,
                            ContentType = pageBlob.Properties.ContentType,
                            Uri = pageBlob.Uri
                        });
                        break;
                }
            }

            return View(blobsList);
        }

        [HttpGet]
        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var container = _blobClient.GetContainerReference(DateTime.Today.ToString("yyyy-MM"));
                var blob = container.GetBlockBlobReference(string.Format("{0}_{1}", DateTime.Now.ToFileTime(), fileName));
                blob.UploadFromStream(file.InputStream);
            }

            return RedirectToAction("Index");
        }
    }
}
