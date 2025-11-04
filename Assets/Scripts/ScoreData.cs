using System;
using System.Collections.Generic;

[Serializable]
public class ScoreData
{
    public string studentName;
    public string studentPhoneNum;
    public int score;
}

[Serializable]
public class ScoreList
{
    public List<ScoreData> scores = new();
}
