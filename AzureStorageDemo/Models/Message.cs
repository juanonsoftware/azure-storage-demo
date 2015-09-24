using System;

namespace AzureStorageDemo.Models
{
    public class Message
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string Content { get; set; }
    }
}