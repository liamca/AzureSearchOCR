using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using System.Configuration; // get it from http://www.newtonsoft.com/json
namespace AzureSearchOCRwithKeywordExtraction
{
    /// <summary>
    /// This is a sample program that shows how to use the Azure ML Text Analytics app (https://datamarket.azure.com/dataset/amla/text-analytics)
    /// </summary>
    public class TextExtraction
    {
        private const string ServiceBaseUri = "https://api.datamarket.azure.com/";
        public static KeyPhraseResult  ProcessText(string inputText)
        {
            string accountKey = ConfigurationManager.AppSettings["textExtractionAccountKey"];
            KeyPhraseResult keyPhraseResult = new KeyPhraseResult();
            using (var httpClient = new HttpClient())
            {
                string inputTextEncoded = HttpUtility.UrlEncode(inputText);
                httpClient.BaseAddress = new Uri(ServiceBaseUri);
                string creds = "AccountKey:" + accountKey;
                string authorizationHeader = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(creds));
                httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeader);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // get key phrases
                string keyPhrasesRequest = "data.ashx/amla/text-analytics/v1/GetKeyPhrases?Text=" + inputTextEncoded;
                Task<HttpResponseMessage> responseTask = httpClient.GetAsync(keyPhrasesRequest);
                responseTask.Wait();
                HttpResponseMessage response = responseTask.Result;
                Task<string> contentTask = response.Content.ReadAsStringAsync();
                contentTask.Wait();
                string content = contentTask.Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Call to get key phrases failed with HTTP status code: " +
                                        response.StatusCode + " and contents: " + content);
                }
                keyPhraseResult = JsonConvert.DeserializeObject<KeyPhraseResult>(content);
                Console.WriteLine("Key phrases: {0} \r\n", string.Join(",", keyPhraseResult.KeyPhrases));

                // Uncomment the following if you want to retrieve additional details on this text 

                //// get sentiment
                //string sentimentRequest = "data.ashx/amla/text-analytics/v1/GetSentiment?Text=" + inputTextEncoded;
                //responseTask = httpClient.GetAsync(sentimentRequest);
                //responseTask.Wait();
                //response = responseTask.Result;
                //contentTask = response.Content.ReadAsStringAsync();
                //contentTask.Wait();
                //content = contentTask.Result;
                //if (!response.IsSuccessStatusCode)
                //{
                //    throw new Exception("Call to get sentiment failed with HTTP status code: " +
                //                        response.StatusCode + " and contents: " + content);
                //}
                //SentimentResult sentimentResult = JsonConvert.DeserializeObject<SentimentResult>(content);
                //Console.WriteLine("Sentiment score: " + sentimentResult.Score);
                
                //// get the language in text
                //string languageRequest = "data.ashx/amla/text-analytics/v1/GetLanguage?Text=" + inputTextEncoded;
                //responseTask = httpClient.GetAsync(languageRequest);
                //responseTask.Wait();
                //response = responseTask.Result;
                //contentTask = response.Content.ReadAsStringAsync();
                //contentTask.Wait();
                //content = contentTask.Result;
                //if (!response.IsSuccessStatusCode)
                //{
                //    throw new Exception("Call to get language failed with HTTP status code: " +
                //                        response.StatusCode + " and contents: " + content);
                //}
                //LanguageResult languageResult = JsonConvert.DeserializeObject<LanguageResult>(content);
                //Console.WriteLine("Detected Languages: " + string.Join(",", languageResult.DetectedLanguages.Select(language => language.Name).ToArray()));

            }
            return keyPhraseResult;
        }
    }
    /// <summary>
    /// Class to hold result of Key Phrases call
    /// </summary>
    public class KeyPhraseResult
    {
        public List<string> KeyPhrases { get; set; }
    }
    /// <summary>
    /// Class to hold result of Sentiment call
    /// </summary>
    public class SentimentResult
    {
        public double Score { get; set; }
    }
    
    /// <summary>
    /// Class to hold result of Language detection call
    /// </summary>
    public class LanguageResult
    {
        public bool UnknownLanguage { get; set; }
        public IList<DetectedLanguage> DetectedLanguages { get; set; }
    }
    /// <summary>
    /// Class to hold information about a single detected language
    /// </summary>
    public class DetectedLanguage
    {
        public string Name { get; set; }
                        
        /// <summary>
        /// This is the short ISO 639-1 standard form of representing
        /// all languages. The short form is a 2 letter representation of the language.
        /// en = English, fr = French for example
        /// </summary>
        public string Iso6391Name { get; set; }
        public double Score { get; set; }
    }
}
    