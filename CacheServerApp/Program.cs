using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;

namespace CacheServerApp
{
    internal class Program
    {
        static Dictionary<string, int> usersDict = new Dictionary<string, int>();
        static bool? isFoundUser = null;
        private static string responseToClient;

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine(">>> Cache server app <<<");
                Console.WriteLine("Listener...");
                var cacheListener = new HttpListener();
                cacheListener.Prefixes.Add("http://localhost:45678/");
                cacheListener.Start();


                while (true)
                {
                    var client = cacheListener.GetContext();
                    Console.WriteLine($"New client connected.");

                    var request = client.Request;
                    var response = client.Response;

                    var reader = new StreamReader(request.InputStream);
                    var writer = new StreamWriter(response.OutputStream);

                    Task.Run(() =>
                    {
                        switch (request.HttpMethod)
                        {
                            case "GET":
                                HttpGet(request, response, writer);
                                break;
                            case "POST":
                                HttpPost(request, response, writer, reader);
                                break;
                            default:
                                break;
                        }
                        response.Close();
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void HttpPost(HttpListenerRequest request, HttpListenerResponse response, StreamWriter writer, StreamReader reader)
        {
            string username = request.QueryString["searchUser"];
            var userLikes = reader.ReadToEnd();
            usersDict.Add(username, int.Parse(userLikes));
            string addDataToCacheMessage = $"~~~\"{username}\" cache dataya elave olundu!~~~";
            Console.WriteLine(addDataToCacheMessage);
            writer.Write(addDataToCacheMessage);
            writer.Flush();
            response.StatusCode = (int)HttpStatusCode.OK;
        }

        private static void HttpGet(HttpListenerRequest request, HttpListenerResponse response, StreamWriter writer)
        {
            string username = request.QueryString["searchUser"];
            if (usersDict.ContainsKey(username))
            {
                isFoundUser = true;
                responseToClient = $"{username} --> likes: {usersDict.GetValueOrDefault(username, 0)}";
                Console.WriteLine("Melumat cache serverde tapildi!");
                Console.WriteLine("Cliente melumat cache serverden ugurla gonderildi!");
            }
            else
            {
                isFoundUser = false;
                responseToClient = isFoundUser.ToString();
                Console.WriteLine("Melumat cache serverde tapilmadi!");
            }
            writer.Write(responseToClient);
            writer.Flush();
            response.StatusCode = (int)HttpStatusCode.OK;
        }
    }
}
