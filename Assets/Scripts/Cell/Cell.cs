using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Cell : MonoBehaviour
{
    public UnityEvent OnMoveFinished = new UnityEvent();

    [SerializeField] private GameConfigs _gameConfigs;
    private bool _isMoving = false;
    private int _tweenFadeId;
    private int _tweenMoveId;

    public bool IsMoving => _isMoving;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void Init()
    {
        var collider = gameObject.AddComponent<BoxCollider2D>(); // for Raycast2D
        var cellRenderer = gameObject.AddComponent<SpriteRenderer>();
        gameObject.layer = LayerMask.NameToLayer(_gameConfigs.CellLayer); // for raycasting
        collider.size = _gameConfigs.CellDimension;
        collider.isTrigger = true;
        cellRenderer.sortingOrder = 1;
        cellRenderer.sprite = GetRandomCellSprite();
    }

    public void Move(Vector3 target)
    {
        //if (_isMoving) return;

        //_isMoving = true;
        float duration = Vector3.Distance(transform.position, target) / _gameConfigs.CellMoveSpeed;
        transform.DOMove(target, duration).OnComplete(() =>
        {
            //_isMoving = false;
            OnMoveFinished?.Invoke();
        });
    }

    public void StartFade()
    {
        var renderer = GetComponent<SpriteRenderer>();
        _tweenFadeId = renderer.DOFade(_gameConfigs.CellFadeTo, _gameConfigs.CellFadeDuration).SetLoops(-1, LoopType.Yoyo).SetAutoKill(false).intId;
    }

    public void StopFade()
    {
        DOTween.Kill(_tweenFadeId);
        var render = GetComponent<SpriteRenderer>();
        var color = render.color;
        render.color = new Color(color.r, color.g, color.b, 1);
    }

    public void AssignRandomSprite()
    {
        var render = GetComponent<SpriteRenderer>();
        render.sprite = GetRandomCellSprite();
    }

    private Sprite GetRandomCellSprite()
    {
        int index = Random.Range(0, _gameConfigs.CellSprites.Length);
        return _gameConfigs.CellSprites[index];
    }
}