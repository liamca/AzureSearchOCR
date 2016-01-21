using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSearchOCR
{
    [SerializePropertyNamesAsCamelCase]
    public class OCRTextIndex
    {
        public string fileId { get; set; }

        public string fileName { get; set; }

        public string ocrText { get; set; }

    }
}