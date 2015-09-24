using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureStorageDemo.Models
{
    public class HomeViewModel
    {
        public IDictionary<Uri, string> Containers { get; set; }
        public IDictionary<Uri, string> Queues { get; set; }
    }
}