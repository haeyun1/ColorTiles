using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private string studentName;
    [SerializeField] private string studentPhoneNum;
    [SerializeField] private int score;
    [SerializeField] private float maxTime = 120f;
    [SerializeField] private float curTime;

    private Coroutine coroutine;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // --------------------- Timer ---------------------
    public void StartTimer()
    {
        StopAllCoroutines();
        curTime = maxTime;
        coroutine = StartCoroutine(OrderTimer());
    }

    public void StopTimer()
    {
        if (coroutine == null) return;
        StopCoroutine(coroutine);
    }

    IEnumerator OrderTimer()
    {
        curTime = maxTime;

        while (curTime > 0)
        {
            curTime -= Time.deltaTime;
            yield return null;
        }

        curTime = 0;
        SaveForRank();
        UIManager.instance.SetState(UIManager.State.End);
    }

    public float GetTime()
    {
        return curTime;
    }

    public void SetTime(float time)
    {
        curTime += time;
    }

    public float GetMaxTime()
    {
        return maxTime;
    }
    // -------------------------------------------------

    // --------------------- Score ---------------------
    public string GetScore()
    {
        return score.ToString();
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    public void ResetScore()
    {
        score = 0;
    }

    public void SaveForRank()
    {
        RankingData.AddScore(studentName, studentPhoneNum, score);
    }
    // -------------------------------------------------
    public void SetInformation(string studentName, string studentPhoneNum = "")
    {
        this.studentName = studentName;
        if (string.IsNullOrWhiteSpace(studentPhoneNum)) return;
        this.studentPhoneNum = studentPhoneNum;
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
