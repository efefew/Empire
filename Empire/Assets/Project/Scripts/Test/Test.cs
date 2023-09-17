using System;
using System.Diagnostics;

using AdvancedEditorTools.Attributes;

using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform tr1;
    public Rigidbody2D rb2d;
    public float speed = 1.0f;
    [Min(1)]
    public int minCoun = 1;
    [Min(0)]
    public long minMilliseconds = 100;
    #region Methods
    public void Test1() => tr1.position += tr1.right * speed;
    public void Test2() => rb2d.AddForce(tr1.right * speed);

    [Button("Test", 15)]
    public void TestTime() => TimeCheck(new Action[] { Test1, Test2 }, minCoun, minMilliseconds);

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