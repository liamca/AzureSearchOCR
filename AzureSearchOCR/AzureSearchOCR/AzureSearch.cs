using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSearchOCR
{
    public class AzureSearch
    {
        public static void CreateIndex(SearchServiceClient serviceClient, string indexName)
        {

            if (serviceClient.Indexes.Exists(indexName))
            {
                serviceClient.Indexes.Delete(indexName);
            }

            var definition = new Index()
            {
                Name = indexName,
                Fields = new[]
                {
                    new Field("fileId", DataType.String)                       { IsKey = true },
                    new Field("fileName", DataType.String)                     { IsSearchable = true, IsFilterable = false, IsSortable = false, IsFacetable = false },
                    new Field("ocrText", DataType.String)                      { IsSearchable = true, IsFilterable = false, IsSortable = false, IsFacetable = false }
                }
            };

            serviceClient.Indexes.Create(definition);
        }

        public static void UploadDocuments(SearchIndexClient indexClient, string fileId, string fileName, string ocrText)
        {
            var documents =
                new OCRTextIndex[]
                {
                    new OCRTextIndex()
                    { 
                        fileId = fileId, 
                        fileName = fileName, 
                        ocrText = ocrText
                    }
                };

            try
            {
                indexClient.Documents.Index(IndexBatch.Create(documents.Select(doc => IndexAction.Create(doc))));
            }
            catch (IndexBatchException e)
            {
                // Sometimes when your Search service is under load, indexing will fail for some of the documents in
                // the batch. Depending on your application, you can take compensating actions like delaying and
                // retrying. For this simple demo, we just log the failed document keys and continue.
                Console.WriteLine(
                    "Failed to index some of the documents: {0}",
                    String.Join(", ", e.IndexResponse.Results.Where(r => !r.Succeeded).Select(r => r.Key)));
            }

        }

    }
}
