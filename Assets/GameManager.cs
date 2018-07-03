using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public InputField apiKeyField;

    [Header("API Settings")]
    public string API_KEY;
    public const string URL = "https://api.tipeeestream.com/v1.0/";
    public int limit;
    public Text foreverText, responseText;
    public float UpdateRate = 1;
    float TimeHolder;
    [Header("Info Fields")]
    public Text Follower;
    public RootObject requestdata;


    private void Start()
    {
        SetApiKey();
    }

    public void SetApiKey()
    {
        API_KEY = apiKeyField.text;
        Request();
    }

    public void ManualRefresh()
    {
        Request();
    }

    private void Update()
    {
        TimeHolder += Time.deltaTime;
        if (TimeHolder >= UpdateRate)
        {
            Request();
            TimeHolder = 0;
        }
    }

    public void Request()
    {
        //WWW request = new WWW(URL + "events.json?apiKey=" + API_KEY + "&type[]=follow&type[]=donation&type[]=subscription&limit" + limit);
        WWW foreverRequest = new WWW(URL + "events/forever.json?apiKey=" + API_KEY);
        StartCoroutine(OnResponse(foreverRequest, foreverText));
        //StartCoroutine(OnResponse(request, responseText));
    }

    private IEnumerator OnResponse(WWW req, Text text)
    {
        yield return req;
        text.text = req.text;
        requestdata = JsonUtility.FromJson<RootObject>(req.text);
        //Debug.Log(requestdata.datas.details.twitch.followers);
    }

    public class Hitbox
    {
        public string followers { get; set; }
    }

    public class Twitch
    {
        public int followers { get; set; }
    }

    public class Youtube
    {
        public int followers { get; set; }
    }

    public class Details
    {
        public Hitbox hitbox { get; set; }
        public Twitch twitch { get; set; }
        public Youtube youtube { get; set; }
    }

    public class Datas
    {
        public int subscribers { get; set; }
        public int followers { get; set; }
        public double donations { get; set; }
        public int cheers { get; set; }
        public Details details { get; set; }
    }

    public class RootObject
    {
        public string message { get; set; }
        public Datas datas { get; set; }
    }

}
