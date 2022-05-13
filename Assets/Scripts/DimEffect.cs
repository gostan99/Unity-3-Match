using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DimEffect : MonoBehaviour
{
    [SerializeField] private float _duration;

    //[SerializeField] private float _speed;
    [SerializeField] private float _toAlpha;

    private Image _image;
    private int _tweenFadeId;

    // Start is called before the first frame update
    private void Start()
    {
        _image = GetComponent<Image>();
        GameManager.OnPause += OnPause;
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnDestroy()
    {
        GameManager.OnPause -= OnPause;
    }

    private void OnReset()
    {
        _image.DOFade(0, _duration);
    }

    public void OnPause()
    {
        _tweenFadeId = _image.DOFade(_toAlpha / 255, _duration).SetAutoKill(false).intId;
        //var color = _image.color;
        //var target = new Color(color.r, color.g, color.b, _toAlpha);
        //_image.DOFade(_toAlpha, _duration).SetLoops(-1, LoopType.Yoyo);
        //StartCoroutine(IDim());
    }

    //private IEnumerator IDim()
    //{
    //    var color = _image.color;
    //    float amount = _image.color.a;
    //    while (true)
    //    {
    //        amount += Time.deltaTime * _speed;
    //        _image.color = new Color(color.r, color.g, color.b, amount);
    //        if (amount >= _toAlpha / 255)
    //        {
    //            break;
    //        }
    //        yield return null;
    //    }
    //}
}