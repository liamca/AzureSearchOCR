// Sample Code used based from the PDF Image extraction sample provided by 
// https://psycodedeveloper.wordpress.com/2013/01/10/how-to-extract-images-from-pdf-files-using-c-and-itextsharp/
// PDF Extraction done using iTextSharp

using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.IO;

namespace AzureSearchOCRwithKeywordExtraction
{
    /// <summary>Helper class to extract images from a PDF file. Works with the most 
    /// common image types embedded in PDF files, as far as I can tell.</summary>
    /// <example>
    /// Usage example:
    /// <code>
    /// foreach (var filename in Directory.GetFiles(searchPath, "*.pdf", SearchOption.TopDirectoryOnly))
    /// {
    ///     var images = ImageExtractor.ExtractImages(filename);
    ///     var directory = Path.GetDirectoryName(filename);
    /// 
    ///     foreach (var name in images.Keys)
    ///     {
    ///         images[name].Save(Path.Combine(directory, name));
    ///     }
    /// }
    /// </code></example>
    public static class PdfImageExtractor
    {
        #region Methods

        #region Public Methods

        /// <summary>Checks whether a specified page of a PDF file contains images.</summary>
        /// <returns>True if the page contains at least one image; false otherwise.</returns>
        public static bool PageContainsImages(string filename, int pageNumber)
        {
            using (var reader = new PdfReader(filename))
            {
                var parser = new PdfReaderContentParser(reader);
                ImageRenderListener listener = null;
                parser.ProcessContent(pageNumber, (listener = new ImageRenderListener()));
                return listener.Images.Count > 0;
            }
        }

        /// <summary>Extracts all images (of types that iTextSharp knows how to decode) from a PDF file.</summary>
        public static Dictionary<string, System.Drawing.Image> ExtractImages(string filename)
        {
            var images = new Dictionary<string, System.Drawing.Image>();

            using (var reader = new PdfReader(filename))
            {
                var parser = new PdfReaderContentParser(reader);
                ImageRenderListener listener = null;

                for (var i = 1; i <= reader.NumberOfPages; i++)
                {
                    parser.ProcessContent(i, (listener = new ImageRenderListener()));
                    var index = 1;

                    if (listener.Images.Count > 0)
                    {
                        Console.WriteLine("Found {0} images on page {1}.", listener.Images.Count, i);

                        foreach (var pair in listener.Images)
                        {
                            images.Add(string.Format("{0}_Page_{1}_Image_{2}{3}",
                                System.IO.Path.GetFileNameWithoutExtension(filename), i.ToString("D4"), index.ToString("D4"), pair.Value), pair.Key);
                            index++;
                        }
                    }
                }
                return images;
            }
        }

        /// <summary>Extracts all images (of types that iTextSharp knows how to decode) 
        /// from a specified page of a PDF file.</summary>
        /// <returns>Returns a generic <see cref="Dictionary&lt;string, System.Drawing.Image&gt;"/>, 
        /// where the key is a suggested file name, in the format: PDF filename without extension, 
        /// page number and image index in the page.</returns>
        public static Dictionary<string, System.Drawing.Image> ExtractImages(string filename, int pageNumber)
        {
            Dictionary<string, System.Drawing.Image> images = new Dictionary<string, System.Drawing.Image>();
            PdfReader reader = new PdfReader(filename);
            PdfReaderContentParser parser = new PdfReaderContentParser(reader);
            ImageRenderListener listener = null;

            parser.ProcessContent(pageNumber, (listener = new ImageRenderListener()));
            int index = 1;

            if (listener.Images.Count > 0)
            {
                Console.WriteLine("Found {0} images on page {1}.", listener.Images.Count, pageNumber);

                foreach (KeyValuePair<System.Drawing.Image, string> pair in listener.Images)
                {
                    images.Add(string.Format("{0}_Page_{1}_Image_{2}{3}",
                        System.IO.Path.GetFileNameWithoutExtension(filename), pageNumber.ToString("D4"), index.ToString("D4"), pair.Value), pair.Key);
                    index++;
                }
            }
            return images;
        }

        #endregion Public Methods

        #endregion Methods
    }

    internal class ImageRenderListener : IRenderListener
    {
        #region Fields

        Dictionary<System.Drawing.Image, string> images = new Dictionary<System.Drawing.Image, string>();

        #endregion Fields

        #region Properties

        public Dictionary<System.Drawing.Image, string> Images
        {
            get { return images; }
        }

        #endregion Properties

        #region Methods

        #region Public Methods

        public void BeginTextBlock() { }

        public void EndTextBlock() { }

        public void RenderImage(ImageRenderInfo renderInfo)
        {
            PdfImageObject image = renderInfo.GetImage();
            PdfName filter = (PdfName)image.Get(PdfName.FILTER);

            //int width = Convert.ToInt32(image.Get(PdfName.WIDTH).ToString());
            //int bitsPerComponent = Convert.ToInt32(image.Get(PdfName.BITSPERCOMPONENT).ToString());
            //string subtype = image.Get(PdfName.SUBTYPE).ToString();
            //int height = Convert.ToInt32(image.Get(PdfName.HEIGHT).ToString());
            //int length = Convert.ToInt32(image.Get(PdfName.LENGTH).ToString());
            //string colorSpace = image.Get(PdfName.COLORSPACE).ToString();

            /* It appears to be safe to assume that when filter == null, PdfImageObject 
             * does not know how to decode the image to a System.Drawing.Image.
             * 
             * Uncomment the code above to verify, but when I've seen this happen, 
             * width, height and bits per component all equal zero as well. */
            if (filter != null)
            {
                System.Drawing.Image drawingImage = image.GetDrawingImage();

                string extension = ".";

                if (filter == PdfName.DCTDECODE)
                {
                    extension += PdfImageObject.ImageBytesType.JPG.FileExtension;
                }
                else if (filter == PdfName.JPXDECODE)
                {
                    extension += PdfImageObject.ImageBytesType.JP2.FileExtension;
                }
                else if (filter == PdfName.FLATEDECODE)
                {
                    extension += PdfImageObject.ImageBytesType.PNG.FileExtension;
                }
                else if (filter == PdfName.LZWDECODE)
                {
                    extension += PdfImageObject.ImageBytesType.CCITT.FileExtension;
                }

                /* Rather than struggle with the image stream and try to figure out how to handle 
                 * BitMapData scan lines in various formats (like virtually every sample I've found 
                 * online), use the PdfImageObject.GetDrawingImage() method, which does the work for us. */
                this.Images.Add(drawingImage, extension);
            }
        }

        public void RenderText(TextRenderInfo renderInfo) { }

        #endregion Public Methods

        #endregion Methods
    }
}
