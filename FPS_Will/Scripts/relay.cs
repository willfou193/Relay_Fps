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

    public async Task SendRequest(string token)
    {
        // Set the authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", token);
        //Set the Ips for the request
        var objectToSerialize = new RootObject
        {
            users = new List<Users>
                          {
                             new Users { ip = "10.10.10.11"},
                             new Users { ip = "10.10.10.10"}
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

            Debug.Log($"We are ready to enter the Session ID and User ID!!");
            print("Here is your Session_ID : " + data.session_id);
            print("Here is your Token to enter : " + data.authorization_token);
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
            await Task.Delay(3000); // Wait 5 seconds between each iteration
            var response = await client.GetAsync("https://api.edgegap.com/v1/relays/sessions/" + sessionId);
            var responseContent = await response.Content.ReadAsStringAsync();
            print("Response from client -----------" + responseContent);
            content = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
            print("Is the game ready : "+content.ready);
        }
        

        // The "ready" property is now true, output a message
        Debug.Log("Data is now ready!");
    }
    private async void DeleteSessionAsync(string sessionId)
    {
        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.DeleteAsync($"https://myapi.com/session/{sessionId}");

            if (response.IsSuccessStatusCode)
            {
                Debug.Log("Session deleted successfully.");
            }
            else
            {
                Debug.LogError($"Failed to delete session. Status code: {response.StatusCode}");
            }
        }
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
    public int? port { get; set; }
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
    public long? authorization_token { get; set; }
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
    public int? port { get; set; }
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
