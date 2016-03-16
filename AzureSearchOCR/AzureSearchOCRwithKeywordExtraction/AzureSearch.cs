using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSearchOCRwithKeywordExtraction
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
                    new Field("ocrText", DataType.String)                      { IsSearchable = true, IsFilterable = false, IsSortable = false, IsFacetable = false },
                    new Field("keyPhrases", DataType.Collection(DataType.String)) { IsSearchable = true, IsFilterable = true,  IsFacetable = true }
                }
            };

            serviceClient.Indexes.Create(definition);
        }

        public static void UploadDocuments(SearchIndexClient indexClient, string fileId, string fileName, string ocrText, KeyPhraseResult keyPhraseResult)
        {
            List<IndexAction> indexOperations = new List<IndexAction>();
            var doc = new Document();
            doc.Add("fileId", fileId);
            doc.Add("fileName", fileName);
            doc.Add("ocrText", ocrText);
            doc.Add("keyPhrases", keyPhraseResult.KeyPhrases.ToList());
            indexOperations.Add(IndexAction.Upload(doc));

            try
            {
                indexClient.Documents.Index(new IndexBatch(indexOperations));
            }
            catch (IndexBatchException e)
            {
                // Sometimes when your Search service is under load, indexing will fail for some of the documents in
                // the batch. Depending on your application, you can take compensating actions like delaying and
                // retrying. For this simple demo, we just log the failed document keys and continue.
                Console.WriteLine(
                "Failed to index some of the documents: {0}",
                       String.Join(", ", e.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key)));
            }

        }


        public static void SearchDocuments(SearchIndexClient indexClient, string searchText)
        {
            // Search using the supplied searchText and output documents that match 
            try
            {
                var sp = new SearchParameters();

                DocumentSearchResult<OCRTextIndex> response = indexClient.Documents.Search<OCRTextIndex>(searchText, sp);
                foreach (SearchResult<OCRTextIndex> result in response.Results)
                {
                    Console.WriteLine("File ID: {0}", result.Document.fileId);
                    Console.WriteLine("File Name: {0}", result.Document.fileName);
                    Console.WriteLine("Extracted Text: {0}", result.Document.ocrText);
                    Console.WriteLine("Key Phrases: {0}", string.Join(",", result.Document.keyPhrases));
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed search: {0}", e.Message.ToString());
            }

        }

    }
}
