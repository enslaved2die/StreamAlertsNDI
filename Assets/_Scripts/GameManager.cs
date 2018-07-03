using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public TMP_InputField apiKeyField;

    [Header("API Settings")]
    public string API_KEY;
    public const string URL = "https://api.tipeeestream.com/v1.0/";
    public int limit;
    public float UpdateRate = 1;
    float TimeHolder;
    [Header("Info Fields")]
    public TextMeshProUGUI Follower, Subscriptions, Donations, Cheers, YTSubs;

    //void Awake()
    //{
    //    if (PlayerPrefs.GetString("API_KEY") != null)
    //    {
    //        API_KEY = PlayerPrefs.GetString("API_KEY");
    //    }
    //}

    private void Start()
    {
        if (API_KEY != null)
        {
            Request();
        }
    }
    public void SetApiKey()
    {
        API_KEY = apiKeyField.text;
        //if(PlayerPrefs.GetString("API_KEY") == null)
        //{
        //    PlayerPrefs.SetString("API_KEY", API_KEY);
        //    PlayerPrefs.Save();
        //}
        Request();
    }

    public void ManualRefresh()
    {
        Request();
    }

    private void Update()
    {
        if (API_KEY != null)
        {
            TimeHolder += Time.deltaTime;
            if (TimeHolder >= UpdateRate)
            {
                Request();
                TimeHolder = 0;
            }
        }

    }

    public void Request()
    {
        //WWW request = new WWW(URL + "events.json?apiKey=" + API_KEY + "&type[]=follow&type[]=donation&type[]=subscription&limit" + limit);
        WWW foreverRequest = new WWW(URL + "events/forever.json?apiKey=" + API_KEY);
        StartCoroutine(OnResponse(foreverRequest));
        //StartCoroutine(OnResponse(request, responseText));
    }

    private IEnumerator OnResponse(WWW req)
    {
        yield return req;
        requestdata = JsonUtility.FromJson<RootObject>(req.text);
        SetInfo();
    }

    public RootObject requestdata;

    public void SetInfo()
    {
        Follower.text = requestdata.datas.details.twitch.followers.ToString();
        Subscriptions.text = requestdata.datas.subscribers.ToString();
        Donations.text = requestdata.datas.donations.ToString() + "€";
        Cheers.text = requestdata.datas.cheers.ToString() + " (" + requestdata.datas.cheers / 100 + "€)";
        YTSubs.text = requestdata.datas.details.youtube.followers.ToString();
    }

    [System.Serializable]
    public struct Hitbox
    {
        public string followers;
    }
    [System.Serializable]
    public struct Twitch
    {
        public int followers;
    }
    [System.Serializable]
    public struct Youtube
    {
        public int followers;
    }
    [System.Serializable]
    public struct Details
    {
        public Hitbox hitbox;
        public Twitch twitch;
        public Youtube youtube;
    }
    [System.Serializable]
    public struct Datas
    {
        public int subscribers;
        public int followers;
        public double donations;
        public int cheers;
        public Details details;
    }
    [System.Serializable]
    public struct RootObject
    {
        public string message;
        public Datas datas;
    }
}
