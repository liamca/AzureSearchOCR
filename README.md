---
services: search
platforms: dotnet
author: liamca
---

# Azure Search Optical Character Recognition Sample (OCR)

This is a sample of how to leverage Optical Character Recognition (OCR) to extract text from images to enable Full Text Search over it, from within Azure Search.  In this sample, we take the [following PDF](https://github.com/liamca/AzureSearchOCR/blob/master/AzureSearchOCR/AzureSearchOCR/pdf/sample.pdf) that has an embedded image, extract any of the images within the PDF using [iTextSharp](https://www.nuget.org/packages/iTextSharp/), apply OCR to extract the text using [Project Oxford's Vision API](https://www.projectoxford.ai/vision), and then upload the resulting text to an Azure Search index.

Once the text is uploaded to Azure Search, we can then do full text search over the text in the images.

![Demo Screen Shot](https://raw.githubusercontent.com/liamca/AzureSearchOCR/master/output.png)

## Requirements

Here are some things that you will need to run this sample:
* **Project Oxford Vision API** which you can get [from here](https://www.projectoxford.ai/vision)
* **Azure Search** Service and subscription.  You might want to simply create one of the [free Azure Search service](https://azure.microsoft.com/en-us/pricing/details/search/)

## Special Thanks
Special thanks to Jerome Viveiros who wrote a great sample on how to use iTextSharp on [his blog](https://psycodedeveloper.wordpress.com/2013/01/10/how-to-extract-images-from-pdf-files-using-c-and-itextsharp/) which formed a basis of much of what I used in my sample that extracts the images from the PDF file.
