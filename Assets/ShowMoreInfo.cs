using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowMoreInfo : MonoBehaviour
{

    private GameObject MoreInfo;
    public TextMeshProUGUI user, type, amount, message;
    private TextMeshProUGUI userMoreInfo, typeMoreInfo, amountMoreInfo, messageMoreInfo;

    private void OnEnable()
    {
        MoreInfo = GameObject.FindGameObjectWithTag("MoreInfo");

        Transform child;
        child = MoreInfo.transform.GetChild(0).GetChild(1).gameObject.transform;

        userMoreInfo = child.GetChild(0).GetComponent<TextMeshProUGUI>();
        typeMoreInfo = child.GetChild(1).GetComponent<TextMeshProUGUI>();
        amountMoreInfo = child.GetChild(2).GetComponent<TextMeshProUGUI>();
        messageMoreInfo = child.GetChild(3).GetComponent<TextMeshProUGUI>();
    }

    public void SetMoreInfo()
    {
        userMoreInfo.text = user.text;
        typeMoreInfo.text = type.text;
        amountMoreInfo.text = amount.text;
        messageMoreInfo.text = message.text;

        if (string.IsNullOrEmpty(message.text))
        {
            messageMoreInfo.gameObject.SetActive(false);
        }
        else
        {
            messageMoreInfo.gameObject.SetActive(true);
        }

        if (string.IsNullOrEmpty(amount.text))
        {
            amountMoreInfo.gameObject.SetActive(false);
        }
        else
        {
            amountMoreInfo.gameObject.SetActive(true);
        }

        MoreInfo.GetComponent<CanvasGroup>().alpha = 1;
        MoreInfo.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

}
