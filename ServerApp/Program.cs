using ServerApp.DataAccess.Context;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ServerApp
{
    internal class Program
    {
        static Dictionary<string, int> searchUsersDict = new Dictionary<string, int>();
        static void Main(string[] args)
        {
            try
            {

                var ctx = new UsersRedisContext();
                Console.WriteLine(">>> Server app <<<");
                Console.WriteLine("Listener...");

                var listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:45001/");
                listener.Start();

                while (true)
                {
                    // Main server connected
                    var client = listener.GetContext();
                    Console.WriteLine($"New client connected.");

                    HttpListenerRequest request = client.Request;
                    HttpListenerResponse response = client.Response;

                    var reader = new StreamReader(request.InputStream);
                    var writer = new StreamWriter(response.OutputStream);

                    Task.Run(() =>
                    {
                        switch (request.HttpMethod)
                        {
                            case "GET":
                                HttpGet(request, response, writer, ctx);
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

        private static void HttpGet(HttpListenerRequest request, HttpListenerResponse response, StreamWriter writer, UsersRedisContext dbContext)
        {
            if (request.QueryString.HasKeys())
            {
                string username = request.QueryString["searchUser"];
                string responseToClient = null;

                // Cache server connected
                var urlCache = $"http://localhost:45678/?searchUser={username}";
                var cache = new HttpClient();
                var responseCache = cache.GetAsync(urlCache).Result;

                responseToClient = responseCache.Content.ReadAsStringAsync().Result;
                if (responseToClient == bool.FalseString)
                {
                    if (dbContext.Users.ToList().Exists(u => u.Username == username))
                    {
                        var searchUser = dbContext.Users.FirstOrDefault(u => u.Username == username);
                        responseToClient = $"{searchUser.Username} --> likes: {searchUser.Likes}";
                        Console.WriteLine($"Cliente melumat databazadan ugurla gonderildi!");

                        if (!searchUsersDict.ContainsKey(username))
                            searchUsersDict.Add(username, 1);
                        else
                        {
                            if (searchUsersDict[username] < 3)
                            {
                                searchUsersDict[username] += 1;
                                if (searchUsersDict[username] == 3)
                                {
                                    var content = new StringContent(searchUser.Likes.ToString());
                                    responseCache = cache.PostAsync(urlCache, content).Result;
                                    Console.WriteLine(responseCache.Content.ReadAsStringAsync().Result);
                                }
                            }
                        }
                    }
                    else
                    {
                        string notFoundUserMessage = "Melumat tapilmadi!";
                        responseToClient = notFoundUserMessage;
                        Console.WriteLine("Melumat databazada tapilmadi");
                    }
                }
                else
                {
                    Console.WriteLine($"Cliente melumat cache serverden ugurla gonderildi!");
                }

                writer.WriteLine(responseToClient);
                writer.Flush();
                response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                response.StatusCode = ((int)HttpStatusCode.BadRequest);
                Console.WriteLine($"Cliente melumatin gonderilmesi ugursuz oldu!");
            }
        }
    }
}
