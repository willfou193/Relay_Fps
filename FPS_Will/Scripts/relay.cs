using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Text.Json;
public class relay : MonoBehaviour
{
    private readonly HttpClient _httpClient = new HttpClient();
    static bool QuitGame = false;
    public int? authorization_token;
    public long[] usersID;
    public long? userID;
    public string relayAddress;
    public int relayGameServerPort;
    public int relayGameClientPort;
    public async Task SendRequest(string token)
    {
        // Set the authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", token);
        //Set the Ips for the request
        var objectToSerialize = new RootObject
        {
            users = new List<Users>
                          {
                             new Users { ip = "74.15.154.180"},
                             new Users { ip = "74.15.154.181"}
                          }
        };
        // Serialize the IP array to JSON
        var jsonContent = new StringContent(JsonConvert.SerializeObject(objectToSerialize), Encoding.UTF8, "application/json");
        print(jsonContent.ReadAsStringAsync().Result);
        // Send the POST request and get the response
        var response = await _httpClient.PostAsync("https://api.edgegap.com/v1/relays/sessions", jsonContent);


        var responseContent = await response.Content.ReadAsStringAsync();
        print(responseContent);

        //Sends a loop to wait for a positive responce
        ApiResponse content = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
        await PollDataAsync(_httpClient,content,content.session_id);
        //Reinitialize our content
        var newResponse = await _httpClient.GetAsync("https://api.edgegap.com/v1/relays/sessions/" + content.session_id);
        var newResponseContent = await newResponse.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<ApiResponse>(newResponseContent);
        
        if (data.ready)
        {
            authorization_token = data.authorization_token;

            userID = data.session_users[0].authorization_token;
            relayAddress = data.relay.ip;
            relayGameServerPort = data.relay.ports.server.port;
            relayGameClientPort = data.relay.ports.client.port;
            Debug.Log($"We are ready to enter the Session ID and User ID!!");
            print("Here is your Authorization token : " + data.authorization_token);
            print("Here is the authorization token : " + data.session_users[0].authorization_token + " and " + data.session_users[1].authorization_token);
            print("Relay IP : " + data.relay.ip);
            print("Relay Server ports : " + data.relay.ports.server.port);
            print("Relay Client ports : " + data.relay.ports.client.port);
            await WaitingForDisconnect(_httpClient, content, content.session_id);
        }
        else
        {
            Debug.LogError($"Error: {response.RequestMessage} - {response.ReasonPhrase}");
            Debug.LogError($"Error: Couldn't found a session relay");

        }
    }
    public static async Task PollDataAsync(HttpClient client,ApiResponse content,string sessionId)
    {
        //TODO say something when waiting for too long
        while (!content.ready)
        {
            Debug.Log("Waiting for data to be ready...");
            await Task.Delay(3000); // Wait 3 seconds between each iteration
            var response = await client.GetAsync("https://api.edgegap.com/v1/relays/sessions/" + sessionId);
            var responseContent = await response.Content.ReadAsStringAsync();
            print("Response from client -----------" + responseContent);
            content = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
            print("Is the game ready : "+content.ready);
        }
        

        // The "ready" property is now true, output a message
        Debug.Log("Data is now ready!");
    }
    public static async Task WaitingForDisconnect(HttpClient client, ApiResponse content, string sessionId)
    {
        while (!QuitGame)
        {
            await Task.Delay(5000);
        }
        
        var response = await client.DeleteAsync($"https://api.edgegap.com/v1/relays/sessions/" + sessionId);

        if (response.IsSuccessStatusCode)
        {
            Debug.Log("Session deleted successfully.");
        }
        else
        {
            Debug.LogError($"Failed to delete session. Status code: {response.StatusCode}");
        }
    }
    private void OnApplicationQuit()
    {
        QuitGame = true;
    }
}
public class Users
{
    public string ip { get; set; }
}

public class RootObject
{
    public List<Users> users { get; set; }
}
public class Client
{
    public int port { get; set; }
    public string protocol { get; set; }
    public string link { get; set; }
}

public class Ports
{
    public Server server { get; set; }
    public Client client { get; set; }
}

public class Relay
{
    public string ip { get; set; }
    public string host { get; set; }
    public Ports ports { get; set; }
}

public class ApiResponse
{
    public string session_id { get; set; }
    public int? authorization_token { get; set; }
    public string status { get; set; }
    public bool ready { get; set; }
    public bool linked { get; set; }
    public object? error { get; set; }
    public List<SessionUser>? session_users { get; set; }
    public Relay relay { get; set; }
    public object? webhook_url { get; set; }
}

public class Server
{
    public int port { get; set; }
    public string protocol { get; set; }
    public string link { get; set; }
}

public class SessionUser
{
    public string ip_address { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }
    public long? authorization_token { get; set; }
}
