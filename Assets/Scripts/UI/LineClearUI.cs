using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LineClearUI : MonoBehaviour
{
    int val;

    [SerializeField]
    TextMeshProUGUI valueText_;

    private void Awake()
    {
        valueText_ = GetComponent<TextMeshProUGUI>();
    }

    public int Value_
    {
        get
        {
            return val;
        }
        set
        {
            val = value;
            if(valueText_ != null)
            {
                valueText_.text = val.ToString();
            }
        }
    }
}
