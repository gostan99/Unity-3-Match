using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResetButton : MonoBehaviour
{
    [SerializeField] private float _duration;
    private Button _button;
    private Image _image;
    private TextMeshProUGUI _text;

    // Start is called before the first frame update
    private void Start()
    {
        _image = GetComponent<Image>();
        _button = GetComponent<Button>();
        _text = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _image.enabled = false;
        _button.enabled = false;
        _text.enabled = false;

        GameManager.OnPause += Appear;
        GameManager.OnRestart += Disappear;
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnDestroy()
    {
        GameManager.OnRestart -= Disappear;
        GameManager.OnPause -= Appear;
    }

    private void Appear()
    {
        _text.enabled = true;
        _image.enabled = true;
        _button.enabled = true;
        transform.DOScale(1, _duration);
    }

    private void Disappear()
    {
        _button.enabled = false;
        transform.DOScale(0, _duration).OnComplete(() =>
        {
            _text.enabled = false;
            _image.enabled = false;
        });
    }
}