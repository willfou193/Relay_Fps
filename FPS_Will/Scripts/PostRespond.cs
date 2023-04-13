using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class reponse
{
    public string session_id { get; set; }
    public object authorization_token { get; set; }
    public string status { get; set; }
    public bool ready { get; set; }
    public bool linked { get; set; }
    public object error { get; set; }
    public List<object> session_users { get; set; }
    public object relay { get; set; }
    public string webhook_url { get; set; }
}

