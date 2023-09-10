using System;
using System.Diagnostics;

using AdvancedEditorTools.Attributes;

using UnityEngine;

public class Test : MonoBehaviour
{
    #region Methods
    public void Test1() => _ = string.Format("{0:0.##}", 15.3521324);
    public void Test2() => _ = Math.Round(15.3521324, 2).ToString();
    public void Test3() => _ = $"{Math.Round(15.3521324, 2)}";

    [Button("Test", 15)]
    public void TestTime() => TimeCheck(new Action[] { Test1, Test2, Test3 });

    public void TimeCheck(Action[] actions, int minCount = 1, long minMilliseconds = 100)
    {
        Stopwatch stopWatch = new();
        for (int id = 0; id < actions.Length; id++)
        {
            stopWatch.Start();
            int countID = 0;
            while (countID < minCount || stopWatch.ElapsedMilliseconds < minMilliseconds)
            {
                actions[id].Invoke();
                countID++;
            }

            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;
            string time = string.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            UnityEngine.Debug.Log($"<color=#22AA22> Тест {id + 1}: время = {time}, количество {countID} </color>");

            stopWatch.Reset();
        }
    }

    #endregion Methods
}