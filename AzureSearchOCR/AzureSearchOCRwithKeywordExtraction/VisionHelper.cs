using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSearchOCRwithKeywordExtraction
{
    //
    // Copyright (c) Microsoft. All rights reserved.
    // Licensed under the MIT license.
    //
    // Project Oxford: http://ProjectOxford.ai
    //
    // ProjectOxford SDK Github:
    // https://github.com/Microsoft/ProjectOxfordSDK-Windows
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

    using System;
    using System.IO;
    using System.Text;

    using Microsoft.ProjectOxford.Vision;
    using Microsoft.ProjectOxford.Vision.Contract;

    namespace AzureSearchOCR
    {

        /// <summary>
        /// The class is used to access vision APIs.
        /// </summary>
        public class VisionHelper
        {
            /// <summary>
            /// The vision service client.
            /// </summary>
            private readonly IVisionServiceClient visionClient;

            /// <summary>
            /// Initializes a new instance of the <see cref="VisionHelper"/> class.
            /// </summary>
            /// <param name="subscriptionKey">The subscription key.</param>
            public VisionHelper(string subscriptionKey)
            {
                this.visionClient = new VisionServiceClient(subscriptionKey);
            }

            /// <summary>
            /// Recognize text from given image.
            /// </summary>
            /// <param name="imagePathOrUrl">The image path or url.</param>
            /// <param name="detectOrientation">if set to <c>true</c> [detect orientation].</param>
            /// <param name="languageCode">The language code.</param>
            public OcrResults RecognizeText(string imagePathOrUrl, bool detectOrientation = true, string languageCode = LanguageCodes.AutoDetect)
            {
                this.ShowInfo("Recognizing");
                OcrResults ocrResult = null;
                string resultStr = string.Empty;

                try
                {
                    if (File.Exists(imagePathOrUrl))
                    {
                        using (FileStream stream = File.Open(imagePathOrUrl, FileMode.Open))
                        {
                            ocrResult = this.visionClient.RecognizeTextAsync(stream, languageCode, detectOrientation).Result;
                        }
                    }
                    else
                    {
                        this.ShowError("Invalid image path or Url");
                    }
                }
                catch (ClientException e)
                {
                    if (e.Error != null)
                    {
                        this.ShowError(e.Error.Message);
                    }
                    else
                    {
                        this.ShowError(e.Message);
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    this.ShowError("Error: " + ex.Message.ToString());
                    return null;
                }

                return ocrResult;
            }


            /// <summary>
            /// Retrieve text from the given OCR results object.
            /// </summary>
            /// <param name="results">The OCR results.</param>
            /// <returns>Return the text.</returns>
            public string GetRetrieveText(OcrResults results)
            {
                StringBuilder stringBuilder = new StringBuilder();

                if (results != null && results.Regions != null)
                {
                    stringBuilder.Append("Text: ");
                    stringBuilder.AppendLine();
                    foreach (var item in results.Regions)
                    {
                        foreach (var line in item.Lines)
                        {
                            foreach (var word in line.Words)
                            {
                                stringBuilder.Append(word.Text);
                                stringBuilder.Append(" ");
                            }

                            stringBuilder.AppendLine();
                        }

                        stringBuilder.AppendLine();
                    }
                }

                return stringBuilder.ToString();
            }

            /// <summary>
            /// Show the working item.
            /// </summary>
            /// <param name="workStr">The work item's string.</param>
            private void ShowInfo(string workStr)
            {
                Console.WriteLine(string.Format("{0}......", workStr));
                Console.ResetColor();
            }

            /// <summary>
            /// Show error message.
            /// </summary>
            /// <param name="errorMessage">The error message.</param>
            private void ShowError(string errorMessage)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(errorMessage);
                Console.ResetColor();
            }

            /// <summary>
            /// Show result message.
            /// </summary>
            /// <param name="resultMessage">The result message.</param>
            private void ShowResult(string resultMessage)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(resultMessage);
                Console.ResetColor();
            }

        }
    }
}
