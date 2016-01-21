---
services: search
platforms: dotnet
author: liamca
---

# Azure Search Optical Character Recognition Sample (OCR)

This is a sample of how to leverage Optical Character Recognition (OCR) to extract text from images to enable Full Text Search in Azure Search.  In this sample, we take a PDF that has an embedded image, extract any of the images within the PDF using iTextSharp, apply OCR to extract the text using Project Orford's Vision API, and then upload the resulting text to an Azure Search index.

Once the text is uploaded to Azure Search, we can then do full text search over the text in the images.
