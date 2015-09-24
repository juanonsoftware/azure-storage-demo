using System;

namespace AzureStorageDemo.Models
{
    public class BlobViewModel
    {
        public string Name { get; set; }
        public long Length { get; set; }
        public string ContentType { get; set; }
        public Uri Uri { get; set; }
    }
}