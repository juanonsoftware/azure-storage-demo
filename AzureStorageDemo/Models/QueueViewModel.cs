using System;

namespace AzureStorageDemo.Models
{
    public class QueueViewModel
    {
        public Uri Uri { get; set; }
        public string Name { get; set; }
        public int Messages { get; set; }
    }
}