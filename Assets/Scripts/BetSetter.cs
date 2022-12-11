using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetSetter : MonoBehaviour
{
    public static int bet = 0;
    [Header ("Input Bet Area")]
    [SerializeField]
    private TMP_InputField _inputBet;
    [Header ("Set Bet Button")]
    [SerializeField]
    private Button _setBet;
    public event Action<int> SumBetChanged = default;

    public void OnSetBetButtonClick()
    {
        bet = int.Parse(_inputBet.text);
        if(FindObjectOfType<SumHandler>().Sum >= bet)
        {
            _setBet.GetComponent<Button>().interactable = false;
            if(SumBetChanged != null)
            {
                SumBetChanged(bet);
            }
        }
        else
        {
            bet = 0;
        }
    }
}
