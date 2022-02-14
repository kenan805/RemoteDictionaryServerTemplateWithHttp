using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;

namespace ClientApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string url = null;
            HttpClient client = null;
            HttpResponseMessage response = null;
            try
            {
                Console.WriteLine(">>> Console client app <<<");
                client = new HttpClient();

                while (true)
                {
                    url = "http://localhost:45001/";
                    Console.Write("Search username: ");
                    var searchUsername = Console.ReadLine();
                    url += $"?searchUser={searchUsername}";
                    response = client.GetAsync(url).Result;
                    var responseFrom = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(responseFrom);

                    if (!response.IsSuccessStatusCode) Console.WriteLine($"Error {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
