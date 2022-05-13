using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    [SerializeField] private IntVariable _score;
    private TextMeshProUGUI _scoreText;

    // Start is called before the first frame update
    private void Start()
    {
        _scoreText = GetComponent<TextMeshProUGUI>();
        _score.Value = 0;

        GameManager.OnRestart += () => _score.Value = 0;
    }

    private void OnDestroy()
    {
        GameManager.OnRestart -= () => _score.Value = 0;
    }

    // Update is called once per frame
    private void Update()
    {
        _scoreText.text = "Score: " + _score.Value.ToString();
    }
}