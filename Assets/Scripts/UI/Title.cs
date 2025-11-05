using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UIElements;

public class Title : MonoBehaviour, IUIState
{
    private UIDocument uI;
    List<Button> btns = new List<Button>();
    VisualElement overlay;
    List<VisualElement> popUps;
    List<TextField> textFields;
    public VisualTreeAsset rankingItemTemplate;
    private ScrollView scrollView;
    Label topScore;
    void Awake()
    {
        uI = gameObject.GetComponent<UIDocument>();
    }

    void OnEnable()
    {
        UIManager.instance.ApplyPlatformStyles(uI);
    }

    public void Enter()
    {
        gameObject.SetActive(true);

        var root = uI.rootVisualElement;
        btns.AddRange(root.Q<VisualElement>().Query<Button>().ToList());
        topScore = root.Q<Label>("TopScore");
        overlay = root.Q<VisualElement>("Overlay");
        popUps = overlay.Query<VisualElement>(className: "pop-up").ToList();
        scrollView = popUps[1].Q<ScrollView>();
        textFields = popUps[2].Query<TextField>().ToList();
        textFields.Add(popUps[4].Q<VisualElement>("PWContainer").Q<TextField>());
        btns.Add(overlay.Q<VisualElement>().Q<Button>());
        btns.Add(popUps[4].Q<VisualElement>("PWContainer").Q<Button>());
        UIManager.instance.Hide(overlay);
        UIManager.instance.ShowTile(false);
        foreach (var btn in btns)
        {
            btn.clicked += () => OnClick(btn.name);
        }
        RefreshRanking();
    }

    void OnClick(string btnName)
    {
        switch (btnName)
        {
            case "PlayBtn":
                SelectPopUp(2);
                break;
            case "StartBtn":
                if (string.IsNullOrWhiteSpace(textFields[0].value))
                {
                    SelectPopUp(3);
                }
                else
                {
                    GameManager.instance.SetInformation(textFields[0].value, textFields[1].value);
                    UIManager.instance.SetState(UIManager.State.InGame);
                    GameManager.instance.SetState(true);
                }
                break;
            case "HelpBtn":
                SelectPopUp(0);
                break;
            case "RankBtn":
                SelectPopUp(1);
                break;
            case "BackBtn":
                UIManager.instance.Hide(overlay);
                break;
            case "QuitBtn":
                GameManager.instance.QuitGame();
                break;
            case "ResetBtn":
                SelectPopUp(4);
                break;
            case "ApplyBtn":
                if (textFields[2].value == "041020")
                {
                    RankingData.DeleteFile();
                    RefreshRanking();
                    UIManager.instance.Hide(overlay);
                }
                break;
        }
    }

    void SelectPopUp(int index)
    {
        UIManager.instance.Show(overlay);
        for (int i = 0; i < popUps.Count; i++)
        {
            UIManager.instance.Hide(popUps[i]);
        }
        UIManager.instance.Show(popUps[index]);
    }

    void RefreshRanking()
    {
        scrollView.Clear();

        ScoreList data = RankingData.Load();
        int count = Mathf.Min(data.scores.Count, 100);

        for (int i = 0; i < count; i++)
        {
            var item = rankingItemTemplate.Instantiate();
            item.Q<Label>("StudentName").text = data.scores[i].studentName;
            item.Q<Label>("StudentScore").text = data.scores[i].score.ToString();
            scrollView.Add(item);
        }
        if (count > 0) topScore.text = data.scores[0].score.ToString();
        else topScore.text = "0";
    }

    public void Exit()
    {
        gameObject.SetActive(false);
    }
}
