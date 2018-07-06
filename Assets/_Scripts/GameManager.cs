using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#region JSONHelper
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
#endregion

public class GameManager : MonoBehaviour
{
    #region VARIABLES
    public TMP_InputField apiKeyField;

    [Header("API Settings")]
    public string API_KEY;
    public const string URL = "https://api.tipeeestream.com/v1.0/";
    public int limit = 4;
    public float UpdateRate = 1;
    float TimeHolder;

    [Header("Info Fields")]
    public TextMeshProUGUI FollowerCount;
    public TextMeshProUGUI SubscriptionCount, DonationAmount, CheerAmount, YTSubAmount;
    public TextMeshProUGUI responseText, lastEventIDField;
    [Space]
    public Toggle followToggle;
    public Toggle subsToggle, donationToggle, hostsToggle;
    public bool subsActive, followsActive, donationsActive, hostsActive;
    private bool initialized = false;

    [Header("Alerts")]
    public Follower follower;
    public Subscription subscription;
    public Donation donation;
    public Host host;
    [Space]
    public GameObject saveArea;
    private TextMeshProUGUI ErrorText;
    [Header("Last Events")]
    public GameObject lastEventsPrefab;
    public RectTransform EventsLayoutGroup;
    [Space]
    public List<LastEvent> lastEvents;

    [Header("Requested Data")]
    public RootObject foreverData;
    public RootObject eventData;
    private int lastEventID = 0;

    [System.Serializable]
    public struct LastEvent
    {
        public GameObject Holder;
        public TextMeshProUGUI User;
        public TextMeshProUGUI Type;
        public TextMeshProUGUI Amount;
        public TextMeshProUGUI Message;
    }
    [System.Serializable]
    public struct Follower
    {
        public GameObject Root;
        public TextMeshProUGUI User;
        public GameObject _3DLayer;
    };
    [System.Serializable]
    public struct Subscription
    {
        public GameObject Root;
        public TextMeshProUGUI User;
        public TextMeshProUGUI Message;
        public TextMeshProUGUI ResubCount;
        public GameObject _3DLayer;
    };
    [System.Serializable]
    public struct Donation
    {
        public GameObject Root;
        public TextMeshProUGUI User;
        public TextMeshProUGUI Message;
        public TextMeshProUGUI Amount;
        public GameObject _3DLayer;
    };
    [System.Serializable]
    public struct Host
    {
        public GameObject Root;
        public TextMeshProUGUI User;
        public GameObject _3DLayer;
    };
    #endregion

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
    [ContextMenu("Clear Alert Bools")]
    public void ClearAlertsBools()
    {
        PlayerPrefs.DeleteKey("subsBool");
        PlayerPrefs.DeleteKey("donationsBool");
        PlayerPrefs.DeleteKey("followsBool");
        PlayerPrefs.DeleteKey("hostsBool");
    }
    void Awake()
    {
        ErrorText = saveArea.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        API_KEY = PlayerPrefs.GetString("API_KEY");
        lastEventID = PlayerPrefs.GetInt("lastEventID");
        apiKeyField.text = API_KEY;
        lastEventIDField.text = lastEventID.ToString();

        #region SET TOGGGLES

        if (PlayerPrefs.GetInt("subsBool") == 1)
            subsActive = true;
        else
            subsActive = false;

        if (PlayerPrefs.GetInt("donationsBool") == 1)
            donationsActive = true;
        else
            donationsActive = false;

        if (PlayerPrefs.GetInt("followsBool") == 1)
            followsActive = true;
        else
            followsActive = false;

        if (PlayerPrefs.GetInt("hostsBool") == 1)
            hostsActive = true;
        else
            hostsActive = false;

        subsToggle.isOn = subsActive;
        followToggle.isOn = followsActive;
        donationToggle.isOn = donationsActive;
        hostsToggle.isOn = hostsActive;

        initialized = true;

        #endregion

    }
    private void Start()
    {

        if (!string.IsNullOrEmpty(API_KEY))
        {
            Request();
        }

        else
        {
            Debug.LogWarning("NO API_KEY SET");
            ErrorText.text = "NO API_KEY SET";
        }

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
        saveArea.SetActive(false);
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



            if (eventData.datas.items[0].type.Equals("subscription") && subsActive == true)
            {
                ActivateAlert(alertState.subs);

                if (lastEvents.Count > 4)
                {
                    Destroy(lastEvents.First<LastEvent>().Holder);
                    lastEvents.RemoveAt(0);
                }

                LastEvent last = new LastEvent();

                last.Holder = Instantiate(lastEventsPrefab, EventsLayoutGroup);
                last.Holder.GetComponent<RectTransform>().SetAsFirstSibling();

                last.User = last.Holder.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                last.User.text = eventData.datas.items[0].parameters.username;
                last.Type = last.Holder.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                last.Type.text = eventData.datas.items[0].type.ToUpper();
                last.Amount = last.Holder.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                last.Amount.text = eventData.datas.items[0].parameters.amount.ToString() + eventData.datas.items[0].parameters.currency;
                last.Message = last.Holder.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
                last.Message.text = eventData.datas.items[0].parameters.message;

                lastEvents.Add(last);

            }

            if (eventData.datas.items[0].type.Equals("follow") && followsActive == true)
            {
                ActivateAlert(alertState.follow);

                if (lastEvents.Count > 4)
                {
                    Destroy(lastEvents.First<LastEvent>().Holder);
                    lastEvents.RemoveAt(0);
                }

                LastEvent last = new LastEvent();

                last.Holder = Instantiate(lastEventsPrefab, EventsLayoutGroup);
                last.Holder.GetComponent<RectTransform>().SetAsFirstSibling();

                last.User = last.Holder.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                last.User.text = eventData.datas.items[0].parameters.username;
                last.Type = last.Holder.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                last.Type.text = eventData.datas.items[0].type.ToUpper();
                last.Amount = last.Holder.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                last.Amount.text = eventData.datas.items[0].parameters.amount.ToString() + eventData.datas.items[0].parameters.currency;
                last.Message = last.Holder.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
                last.Message.text = eventData.datas.items[0].parameters.message;

                lastEvents.Add(last);
            }

            if (eventData.datas.items[0].type.Equals("donation") && donationsActive == true)
            {
                ActivateAlert(alertState.donation);

                if (lastEvents.Count > 4)
                {
                    Destroy(lastEvents.First<LastEvent>().Holder);
                    lastEvents.RemoveAt(0);
                }

                LastEvent last = new LastEvent();

                last.Holder = Instantiate(lastEventsPrefab, EventsLayoutGroup);
                last.Holder.GetComponent<RectTransform>().SetAsFirstSibling();

                last.User = last.Holder.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                last.User.text = eventData.datas.items[0].parameters.username;
                last.Type = last.Holder.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                last.Type.text = eventData.datas.items[0].type.ToUpper();
                last.Amount = last.Holder.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                last.Amount.text = eventData.datas.items[0].parameters.amount.ToString() + eventData.datas.items[0].parameters.currency;
                last.Message = last.Holder.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
                last.Message.text = eventData.datas.items[0].parameters.message;

                lastEvents.Add(last);
            }

            if (eventData.datas.items[0].type.Equals("hosting") && hostsActive == true)
            {
                ActivateAlert(alertState.host);

                if (lastEvents.Count > 4)
                {
                    Destroy(lastEvents.First<LastEvent>().Holder);
                    lastEvents.RemoveAt(0);
                }

                LastEvent last = new LastEvent();

                last.Holder = Instantiate(lastEventsPrefab, EventsLayoutGroup);
                last.Holder.GetComponent<RectTransform>().SetAsFirstSibling();

                last.User = last.Holder.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                last.User.text = eventData.datas.items[0].parameters.username;
                last.Type = last.Holder.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                last.Type.text = eventData.datas.items[0].type.ToUpper();
                last.Amount = last.Holder.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                last.Amount.text = eventData.datas.items[0].parameters.amount.ToString() + eventData.datas.items[0].parameters.currency;
                last.Message = last.Holder.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
                last.Message.text = eventData.datas.items[0].parameters.message;

                lastEvents.Add(last);
            }
        }

    }
    #endregion

    #region AlertStates
    public enum alertState { subs, follow, donation, host }

    void ActivateAlert(alertState alertState)
    {
        switch (alertState)
        {
            case alertState.subs:

                subscription.Root.SetActive(true);
                follower.Root.SetActive(false);
                donation.Root.SetActive(false);
                host.Root.SetActive(false);
                subscription._3DLayer.SetActive(true);
                follower._3DLayer.SetActive(false);
                donation._3DLayer.SetActive(false);
                host._3DLayer.SetActive(false);

                subscription.User.text = eventData.datas.items[0].parameters.username;
                subscription.Message.text = eventData.datas.items[0].parameters.message;
                subscription.ResubCount.text = "Resub " + eventData.datas.items[0].parameters.resub + "x";
                Debug.Log("Subscription");
                break;
            case alertState.follow:

                subscription.Root.SetActive(false);
                follower.Root.SetActive(true);
                donation.Root.SetActive(false);
                host.Root.SetActive(false);
                subscription._3DLayer.SetActive(false);
                follower._3DLayer.SetActive(true);
                donation._3DLayer.SetActive(false);
                host._3DLayer.SetActive(false);

                follower.User.text = eventData.datas.items[0].parameters.username;
                Debug.Log("Follower");
                break;
            case alertState.donation:

                subscription.Root.SetActive(false);
                follower.Root.SetActive(false);
                donation.Root.SetActive(true);
                host.Root.SetActive(false);
                subscription._3DLayer.SetActive(false);
                follower._3DLayer.SetActive(false);
                donation._3DLayer.SetActive(true);
                host._3DLayer.SetActive(false);

                donation.User.text = eventData.datas.items[0].parameters.username;
                donation.Amount.text = eventData.datas.items[0].parameters.amount + eventData.datas.items[0].parameters.currency;
                donation.Message.text = eventData.datas.items[0].parameters.message;
                Debug.Log("Donation");
                break;
            case alertState.host:

                subscription.Root.SetActive(false);
                follower.Root.SetActive(false);
                donation.Root.SetActive(false);
                host.Root.SetActive(true);
                subscription._3DLayer.SetActive(false);
                follower._3DLayer.SetActive(false);
                donation._3DLayer.SetActive(false);
                host._3DLayer.SetActive(true);

                host.User.text = eventData.datas.items[0].parameters.username;
                Debug.Log("Host");
                break;
            default:
                break;
        }
    }

    public void SwitchSubsToggle(bool newValue)
    {
        subsActive = newValue;
        PlayerPrefs.SetInt("subsBool", subsActive ? 1 : 0);
    }

    public void SwitchFollowToggle(bool newValue)
    {
        followsActive = newValue;
        PlayerPrefs.SetInt("followsBool", followsActive ? 1 : 0);
    }

    public void SwitchDonationsToggle(bool newValue)
    {
        donationsActive = newValue;
        PlayerPrefs.SetInt("donationsBool", donationsActive ? 1 : 0);
    }

    public void SwitchHostsToggle(bool newValue)
    {
        hostsActive = newValue;
        PlayerPrefs.SetInt("hostsBool", hostsActive ? 1 : 0);
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
