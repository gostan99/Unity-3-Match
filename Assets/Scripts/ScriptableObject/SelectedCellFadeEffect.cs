using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Cell Fade Effect")]
public class SelectedCellFadeEffect : ScriptableObject
{
    [SerializeField] private ListGameObjectVariable _cellsList;
    [SerializeField] private IntVariable _cellIndex;
    [SerializeField] private float _fadeFrom;
    [SerializeField] private float _duration;

    private int _leanId;

    private void OnEnable()
    {
    }

    public void OnSelectCell()
    {
        var cell = _cellsList.Value[_cellIndex.Value];
        var render = cell.GetComponent<SpriteRenderer>();
        var color = render.color;

        _leanId = LeanTween.value(_fadeFrom, 1f, _duration).setOnUpdate((float val) =>
            {
                render.color = new Color(color.r, color.g, color.b, val);
            }).setLoopPingPong().id;
    }

    public void OnDeselectCell()
    {
        LeanTween.cancel(_leanId);
        var cell = _cellsList.Value[_cellIndex.Value];
        var render = cell.GetComponent<SpriteRenderer>();
        var color = render.color;
        render.color = new Color(color.r, color.g, color.b, 1);
    }
}