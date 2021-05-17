using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace BestBuyProductAvailabilityNotifier
{
    class Program
    {
        static void Main(string[] args)
        {
            //OLED77G1PUA
            //OLED77GXPUA

            if (args == null || !args.Any())
            {
                Console.WriteLine("No model number provided.");
                return;
            }

            try
            {
                string modelNumber = WebUtility.HtmlEncode(args[0]);
                Console.WriteLine($"Querying model number {modelNumber}...");
                var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                          .AddJsonFile("appsettings.json", optional: false)
                                                          .Build();

                string url = BuildUrl(configuration["AppSettings:ApiKey"], modelNumber);

                Console.WriteLine("Url: " + url);

                Console.WriteLine("Sending request...");
                string response = SendRequest(url);

                Console.WriteLine("Response received.");
                Console.WriteLine(response);

                var model = System.Text.Json.JsonSerializer.Deserialize<ProductSearchResultModel>(response);

                Console.WriteLine();
                Console.WriteLine(model.ToString());

                if (model.products != null && model.products.Any())
                {
                    Console.WriteLine("Product found!");
                    var product = model.products.First();
                    string fileName = $"{product.modelNumber}.txt";
                    if (File.Exists(fileName))
                    {
                        Console.WriteLine("File exists. Notification already sent.");
                    }
                    else
                    {
                        var smtpConfiguration = configuration.GetSection("Smtp");

                        Console.WriteLine("Sending email...");
                        using (var client = new SmtpClient(smtpConfiguration["Host"], int.Parse(smtpConfiguration["Port"])))
                        {
                            client.Credentials = new NetworkCredential(smtpConfiguration["Username"], smtpConfiguration["Password"]);
                            client.EnableSsl = true;

                            var notifyConfiguration = configuration.GetSection("Notify");

                            var message = new MailMessage(
                                notifyConfiguration["From"],
                                notifyConfiguration["To"],
                                $"Product {product.modelNumber} Found",
                                product.ToString());
                            
                            client.Send(message);
                        }

                        Console.WriteLine("Email sent.");
                        Console.WriteLine("Writing file...");
                        File.WriteAllText(fileName, product.ToString());
                    }
                }
                else
                {
                    Console.WriteLine("Product not found.");
                }

#if DEBUG
                Console.ReadLine();
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.ToString());
#if DEBUG
                Console.ReadLine();
#else
                throw;
#endif
            }
        }

        private static string BuildUrl(string apiKey, string modelNumber)
        {
            return $"https://api.bestbuy.com/v1/products(modelNumber={modelNumber}&(categoryPath.id=abcat0101000))?format=json&apiKey={apiKey}&show=name,modelNumber,onlineAvailability,inStoreAvailability";
        }

        private static string SendRequest(string url)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Accept = "application/json";

            string responseFromServer;
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var dataStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(dataStream))
                    {
                        responseFromServer = reader.ReadToEnd();
                        reader.Close();
                    }
                }

                response.Close();
            }

            return responseFromServer;
        }
    }
}
