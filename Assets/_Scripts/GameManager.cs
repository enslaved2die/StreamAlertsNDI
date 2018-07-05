using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

class JsonHelper
{
    private const string INDENT_STRING = "    ";
    public static string FormatJson(string str)
    {
        var indent = 0;
        var quoted = false;
        var sb = new StringBuilder();
        for (var i = 0; i < str.Length; i++)
        {
            var ch = str[i];
            switch (ch)
            {
                case '{':
                case '[':
                    sb.Append(ch);
                    if (!quoted)
                    {
                        sb.AppendLine();
                        Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
                    }
                    break;
                case '}':
                case ']':
                    if (!quoted)
                    {
                        sb.AppendLine();
                        Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
                    }
                    sb.Append(ch);
                    break;
                case '"':
                    sb.Append(ch);
                    bool escaped = false;
                    var index = i;
                    while (index > 0 && str[--index] == '\\')
                        escaped = !escaped;
                    if (!escaped)
                        quoted = !quoted;
                    break;
                case ',':
                    sb.Append(ch);
                    if (!quoted)
                    {
                        sb.AppendLine();
                        Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
                    }
                    break;
                case ':':
                    sb.Append(ch);
                    if (!quoted)
                        sb.Append(" ");
                    break;
                default:
                    sb.Append(ch);
                    break;
            }
        }
        return sb.ToString();
    }
}

static class Extensions
{
    public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
    {
        foreach (var i in ie)
        {
            action(i);
        }
    }
}

public class GameManager : MonoBehaviour
{
    private const string INDENT_STRING = "    ";

    static string FormatJson(string json)
    {

        int indentation = 0;
        int quoteCount = 0;
        var result =
            from ch in json
            let quotes = ch == '"' ? quoteCount++ : quoteCount
            let lineBreak = ch == ',' && quotes % 2 == 0 ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, indentation)) : null
            let openChar = ch == '{' || ch == '[' ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, ++indentation)) : ch.ToString()
            let closeChar = ch == '}' || ch == ']' ? Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, --indentation)) + ch : ch.ToString()
            select lineBreak == null
                        ? openChar.Length > 1
                            ? openChar
                            : closeChar
                        : lineBreak;

        return String.Concat(result);
    }

    public TMP_InputField apiKeyField;

    [Header("API Settings")]
    public string API_KEY;
    public const string URL = "https://api.tipeeestream.com/v1.0/";
    public int limit;
    public float UpdateRate = 1;
    float TimeHolder;

    [Header("Info Fields")]
    public TextMeshProUGUI FollowerCount;
    public TextMeshProUGUI SubscriptionCount, DonationAmount, CheerAmount, YTSubAmount;
    public TextMeshProUGUI responseText, lastEventIDField;

    [Header("Alerts")]
    public Follower follower;
    public Subscription subscription;
    public Donation donation;
    public Host host;

    [Header("Requested Data")]
    public RootObject foreverData;
    public RootObject eventData;
    private int lastEventID = 0;

    [System.Serializable]
    public struct Follower
    {
        public GameObject Root;
        public TextMeshPro User;
    };
    [System.Serializable]
    public struct Subscription
    {
        public GameObject Root;
        public TextMeshPro User;
        public TextMeshPro Message;
        public TextMeshPro Month;
    };
    [System.Serializable]
    public struct Donation
    {
        public GameObject Root;
        public TextMeshPro User;
        public TextMeshPro Message;
        public TextMeshPro Amount;
    };
    [System.Serializable]
    public struct Host
    {
        public GameObject Root;
        public TextMeshPro User;
    };


    #region General and Request
    [ContextMenu("Clear API Key")]
    public void ClearAPI_KEY()
    {
        PlayerPrefs.DeleteKey("API_KEY");
    }
    [ContextMenu("Clear Last Event ID")]
    public void ClearLastEventID()
    {
        PlayerPrefs.DeleteKey("lastEventID");
    }
    void Awake()
    {
        API_KEY = PlayerPrefs.GetString("API_KEY");
        lastEventID = PlayerPrefs.GetInt("lastEventID");
        apiKeyField.text = API_KEY;
        lastEventIDField.text = lastEventID.ToString();
    }
    private void Start()
    {
        if (!string.IsNullOrEmpty(API_KEY))
        {
            Request();
        }

        else
            Debug.LogWarning("NO API_KEY SET");
    }
    public void SetApiKey()
    {
        API_KEY = apiKeyField.text;
        PlayerPrefs.SetString("API_KEY", API_KEY);
        PlayerPrefs.Save();
        Request();
    }
    public void SetInfo()
    {
        FollowerCount.text = foreverData.datas.details.twitch.followers.ToString();
        SubscriptionCount.text = foreverData.datas.subscribers.ToString();
        DonationAmount.text = foreverData.datas.donations.ToString() + "€";
        CheerAmount.text = foreverData.datas.cheers.ToString() + " (" + foreverData.datas.cheers / 100 + "€)";
        YTSubAmount.text = foreverData.datas.details.youtube.followers.ToString();
    }
    public void ManualRefresh()
    {
        Request();
    }
    private void Update()
    {
        if (!string.IsNullOrEmpty(API_KEY))
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
        Debug.Log("Request");
        WWW eventRequest = new WWW(URL + "events.json?apiKey=" + API_KEY + "&type[]=follow&type[]=donation&type[]=subscription&limit=" + limit);
        WWW foreverRequest = new WWW(URL + "events/forever.json?apiKey=" + API_KEY);
        StartCoroutine(OnResponseForever(foreverRequest));
        StartCoroutine(OnResponseEvent(eventRequest, responseText));
    }
    private IEnumerator OnResponseForever(WWW req)
    {
        yield return req;
        foreverData = JsonUtility.FromJson<RootObject>(req.text);
        SetInfo();
    }
    private IEnumerator OnResponseEvent(WWW req, TextMeshProUGUI text)
    {
        yield return req;
        text.text = JsonHelper.FormatJson(req.text);
        eventData = JsonUtility.FromJson<RootObject>(req.text);
        if (eventData.datas.items[0].id != lastEventID)
        {
            lastEventID = eventData.datas.items[0].id;
            lastEventIDField.text = lastEventID.ToString();
            PlayerPrefs.SetInt("lastEventID", lastEventID);
            PlayerPrefs.Save();

            if (eventData.datas.items[0].type.Equals("subscription"))
            {
                SubscriptionAlert();
            }

            if (eventData.datas.items[0].type.Equals("follow"))
            {
                FollowerAlert();
            }

            if (eventData.datas.items[0].type.Equals("donation"))
            {
                DonationAlert();
            }

            if (eventData.datas.items[0].type.Equals("hosting"))
            {
                HostAlert();
            }
        }

    }
    #endregion

    #region AlertFunctions

    void SubscriptionAlert()
    {
        subscription.User.text = eventData.datas.items[0].parameters.username;
        subscription.Message.text = eventData.datas.items[0].parameters.message;

        Debug.Log("Subscription");
    }

    void DonationAlert()
    {
        donation.User.text = eventData.datas.items[0].parameters.username;
        donation.Message.text = eventData.datas.items[0].parameters.message;
        Debug.Log("Donation");
    }

    void FollowerAlert()
    {
        follower.User.text = eventData.datas.items[0].parameters.username;
        Debug.Log("Follower");
    }

    void HostAlert()
    {
        host.User.text = eventData.datas.items[0].parameters.username;
        Debug.Log("Host");
    }

    #endregion

    #region Json Structure
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
    public struct Currency
    {
        public string code;
        public string symbol;
        public string label;
        public bool available;
    }
    [System.Serializable]
    public struct Provider
    {
        public DateTime connectedAt;
        public string code;
        public string id;
        public string username;
        public bool master;
        public DateTime last_follow_update;
        public DateTime created_at;
    }
    [System.Serializable]
    public struct User
    {
        public string avatar;
        public DateTime hasPayment;
        public Currency currency;
        public string country;
        public string campaignActivation;
        public int id;
        public string username;
        public string pseudo;
        public List<Provider> providers;
        public DateTime created_at;
        public DateTime session_at;
    }
    [System.Serializable]
    public struct Parameters
    {
        public string twitch_channel_id;
        public object twitch_created_at;
        public int twitch_user_id;
        public string username;
        public double? amount;
        public int? campaignId;
        public string currency;
        public int? fees;
        public string identifier;
        public string formattedMessage;
        public string message;
        public int? plan;
        public int? resub;
        public string gifter;
        public int? user_id;
    }
    [System.Serializable]
    public struct Item
    {
        public int id;
        public string type;
        public User user;
        public string @ref;
        public string origin;
        public DateTime created_at;
        public DateTime inserted_at;
        public bool display;
        public Parameters parameters;
        //public double __invalid_name__parameters.amount;
        public string formattedAmount;
    }
    [System.Serializable]
    public struct Datas
    {
        public int subscribers;
        public int followers;
        public double donations;
        public int cheers;
        public Details details;
        public List<Item> items;
        public int total_count;
    }
    [System.Serializable]
    public struct RootObject
    {
        public string message;
        public Datas datas;
    }
    #endregion
}
