using System;
using System.Diagnostics;

using AdvancedEditorTools.Attributes;

using UnityEngine;

public class TestClass
{
    #region Fields

    public double value;

    #endregion Fields
}

public class Test : MonoBehaviour
{
    #region Fields

    private TestClass[] array = new TestClass[1000];

    [Min(1)]
    public int minCount = 1;

    [Min(0)]
    public long minMilliseconds = 100;

    #endregion Fields

    #region Methods

    public void Test1()
    {
        for (int i = 0; i < array.Length; i++)
            array[i].value = 1;
    }

    public void Test2()
    {
        foreach (TestClass element in array)
            element.value = 1;
    }

    [Button("JustTest", 15)]
    public void JustTest()
    {
    }

    [Button("Test", 15)]
    public void TestTime() => TimeCheck(new Action[] { Test1, Test2 }, minCount, minMilliseconds);

    public void TimeCheck(Action[] actions, int minCount = 1, long minMilliseconds = 100)
    {
        for (int id = 0; id < array.Length; id++)
            array[id] = new TestClass();

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
            UnityEngine.Debug.Log($"<color=#1CDE6F> Тест {id + 1}: время = {time}, количество {countID} </color>");

            stopWatch.Reset();
        }
    }

    #endregion Methods
}