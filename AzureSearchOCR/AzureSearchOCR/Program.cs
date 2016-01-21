// This sample will open a PDF file that includes an image where the image is then extracted,
// sent to Project Oxford Vision API for OCR analysis.  The resulting text is then sent to an
// Azure Search index.

// Sample Code used based from the PDF Image extraction sample provided by 
// https://psycodedeveloper.wordpress.com/2013/01/10/how-to-extract-images-from-pdf-files-using-c-and-itextsharp/
// PDF Extraction done using iTextSharp
// These libraries may have different licenses then those for this sample

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// Azure Search: http://azure.com
// Project Oxford: http://ProjectOxford.ai
//
// Copyright (c) Microsoft Corporation
// All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using AzureSearchOCR.AzureSearchOCR;
using Microsoft.Azure.Search;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Configuration;
using System.IO;

namespace AzureSearchOCR
{
    class Program
    {
        static string oxfordSubscriptionKey = ConfigurationManager.AppSettings["oxfordSubscriptionKey"];
        static string searchServiceName = ConfigurationManager.AppSettings["searchServiceName"];
        static string searchServiceAPIKey = ConfigurationManager.AppSettings["searchServiceAPIKey"];
        static string indexName = "ocrtest";
        static SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(searchServiceAPIKey));
        static SearchIndexClient indexClient = serviceClient.Indexes.GetClient(indexName);
        static VisionHelper vision = new VisionHelper(oxfordSubscriptionKey);

        static void Main(string[] args)
        {
            var searchPath = "pdf";
            var outPath = "image";

            // Note, this will create a new Azure Search Index for the OCR text
            Console.WriteLine("Creating Azure Search index...");
            AzureSearch.CreateIndex(serviceClient, indexName);

            // Creating an image directory
            if (Directory.Exists(outPath) == false)
                Directory.CreateDirectory(outPath);

            foreach (var filename in Directory.GetFiles(searchPath, "*.pdf", SearchOption.TopDirectoryOnly))
            {
                Console.WriteLine("Extracting images from {0}", System.IO.Path.GetFileName(filename));
                var images = PdfImageExtractor.ExtractImages(filename);
                Console.WriteLine("{0} images found.", images.Count);
                Console.WriteLine();

                var directory = System.IO.Path.GetDirectoryName(filename);
                foreach (var name in images.Keys)
                {
                    //if there is a filetype save the file
                    if (name.LastIndexOf(".") + 1 != name.Length)
                        images[name].Save(System.IO.Path.Combine(outPath, name));
                }

                // Read in all the images and convert to text creating one big text string
                string ocrText = string.Empty;
                Console.WriteLine("Extracting text from image...");
                foreach (var imagefilename in Directory.GetFiles(outPath))
                {
                    OcrResults ocr = vision.RecognizeText(imagefilename);
                    ocrText += vision.GetRetrieveText(ocr);
                    File.Delete(imagefilename);
                }

                // Take the resulting orcText and upload to a new Azure Search Index
                // It is highly recommended that you upload documents in batches rather 
                // individually like is done here
                if (ocrText.Length > 0)
                {
                    Console.WriteLine("Uploading extracted text to Azure Search...");
                    string fileNameOnly = System.IO.Path.GetFileName(filename);
                    string fileId = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(fileNameOnly));
                    AzureSearch.UploadDocuments(indexClient, fileId, fileNameOnly, ocrText);
                }

            }

            // Execute a test search 
            Console.WriteLine("Execute Search...");
            AzureSearch.SearchDocuments(indexClient, "Azure Search");

            Console.WriteLine("All done.  Press any key to continue.");
            Console.ReadLine();

        }
    }
}
