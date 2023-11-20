using UnityEngine;
using System.Collections.Generic;

public class CandleExtinguisher : MonoBehaviour
{
    [System.Serializable]
    public class CandleData
    {
        public GameObject candle;
        public int extinguishDay;
    }

    [SerializeField] private List<CandleData> candles = new List<CandleData>();

    private void Start()
    {
        foreach (var candleData in candles)
        {
            if (candleData.candle == null)
            {
                Debug.LogError("Candle GameObject is not assigned!");
            }
        }
    }

    public void ExtinguishCandles(int currentDay)
    {
        foreach (var candleData in candles)
        {
            if (candleData.candle != null && currentDay >= candleData.extinguishDay)
            {
                // Put your logic here to extinguish each candle
                candleData.candle.SetActive(false);
            }
        }
    }
}