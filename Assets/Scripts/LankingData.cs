using UnityEngine;
using System.IO;

public static class RankingData
{
    private static readonly string filePath = Path.Combine(Application.persistentDataPath, "ranking.json");

    public static void Save(ScoreList data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
        Debug.Log(filePath + "에 저장되었습니다.");
    }

    public static ScoreList Load()
    {
        if (!File.Exists(filePath))
            return new ScoreList();

        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<ScoreList>(json);
    }

    public static void AddScore(string studentName, string studentPhoneNum, int score)
    {
        ScoreList data = Load();
        data.scores.Add(new ScoreData { studentName = studentName, studentPhoneNum = studentPhoneNum, score = score });

        data.scores.Sort((a, b) => b.score.CompareTo(a.score));

        if (data.scores.Count > 100)
            data.scores = data.scores.GetRange(0, 100);

        Save(data);
    }

    public static bool DeleteFile()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }
        return false;
    }

    public static string GetFilePath()
    {
        return filePath;
    }
}
