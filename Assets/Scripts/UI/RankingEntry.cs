using UnityEngine;
using TMPro;

public class RankingEntry : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;

    public void SetData(int rank, string name, int score)
    {
        rankText.text = $"{rank}位";
        nameText.text = name;
        scoreText.text = score.ToString();
    }
}
