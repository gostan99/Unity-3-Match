using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CellsManager : Singleton<CellsManager>
{
    public UnityEvent OnSelectCell = new UnityEvent();
    public UnityEvent OnDeselectCell = new UnityEvent();
    public UnityEvent OnCellSwapped = new UnityEvent();

    [SerializeField] private ListGameObjectVariable _cellsList;
    [SerializeField] private IntVariable _clickedCellIndex; // Clicked cell index
    [SerializeField] private GameConfigs _gameConfigs;

    private List<GameObject> _movedCellsList = new List<GameObject>();

    // Start is called before the first frame update
    private void Start()
    {
        _clickedCellIndex.Value = -1;
    }

    // Update is called once per frame
    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
            OnClick();
    }

    private void OnClick()
    {
        var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layer = LayerMask.NameToLayer("Cell");
        var hit = Physics2D.Raycast(mouseRay.origin, mouseRay.direction, Vector2.Distance(transform.position, mouseRay.origin), ~layer); // ~layer mean we ignore all other layer except Cell layer
        if (!hit)
        {
            if (_clickedCellIndex.Value >= 0)
            {
                OnDeselectCell?.Invoke();
                //StopSelectedCellSpriteFadingEffect();
            }
            _clickedCellIndex.Value = -1;
            return;
        }

        int newClickedCellIndex = _cellsList.Value.IndexOf(hit.transform.gameObject);

        if (_clickedCellIndex.Value == -1)
        {
            _clickedCellIndex.Value = newClickedCellIndex;
            OnSelectCell?.Invoke();
            //LeanTween.resume(_leanTweenFadingEffectID);
            return;
        }

        if (_clickedCellIndex.Value == newClickedCellIndex)
        {
            //StopSelectedCellSpriteFadingEffect();
            OnDeselectCell?.Invoke();
            _clickedCellIndex.Value = -1;
            return;
        }

        if (!AreNeighbors(_clickedCellIndex.Value, newClickedCellIndex))
        {
            OnDeselectCell?.Invoke();
            _clickedCellIndex.Value = newClickedCellIndex;
            OnSelectCell?.Invoke();
            return;
        }

        OnDeselectCell?.Invoke();
        SwapCell(_cellsList.Value[_clickedCellIndex.Value], _cellsList.Value[newClickedCellIndex]);
    }

    private void SwapCellIndex(GameObject cellA, GameObject cellB)
    {
        int indexA = _cellsList.Value.IndexOf(cellA);
        int indexB = _cellsList.Value.IndexOf(cellB);
        _cellsList.Value[indexA] = cellB;
        _cellsList.Value[indexB] = cellA;
    }

    public void SwapCell(GameObject cellA, GameObject cellB, bool raiseEventOnFinished = true)
    {
        var duration = Vector2.Distance(cellA.transform.position, cellB.transform.position) / _gameConfigs.CellMoveSpeed;
        LeanTween.move(cellA, cellB.transform.position, duration);
        LeanTween.move(cellB, cellA.transform.position, duration).setOnComplete(() =>
        {
            SwapCellIndex(cellA, cellB);
            _clickedCellIndex.Value = -1;
            if (raiseEventOnFinished)
            {
                _movedCellsList.Add(cellA);
                _movedCellsList.Add(cellB);
                OnCellSwapped?.Invoke();
            }
        });
    }

    private bool AreNeighbors(int indexA, int indexB)
    {
        int up = indexA + _gameConfigs.Columns;
        int down = indexA - _gameConfigs.Columns;
        int left = indexA - 1;
        int right = indexA + 1;

        return indexB == up || indexB == down || indexB == left || indexB == right;
    }

    public void OnCellSwappedResponse()
    {
        var matchedCells = new List<GameObject>();
        foreach (var cell in _movedCellsList)
        {
            var list = GetMatchedCells(cell);
            matchedCells.AddRange(list);
        }
        if (matchedCells.Count < 3)
            SwapCell(_movedCellsList[0], _movedCellsList[1], false);
        else
            Explode(matchedCells);
        _movedCellsList.Clear();
    }

    private void Explode(List<GameObject> matchedCells)
    {
        foreach (var cell in matchedCells)
        {
            var renderer = cell.GetComponent<SpriteRenderer>();
            renderer.sprite = null;
        }
    }

    private List<GameObject> GetMatchedCells(GameObject cell)
    {
        var list = new List<GameObject>();
        int index = _cellsList.Value.IndexOf(cell);

        var tempList = new HashSet<GameObject>();
        // UP
        while (index < _cellsList.Value.Count
            && _cellsList.Value[index].GetComponent<SpriteRenderer>().sprite.Equals(cell.GetComponent<SpriteRenderer>().sprite))
        {
            tempList.Add(_cellsList.Value[index]);
            index += _gameConfigs.Columns;
        }
        index = _cellsList.Value.IndexOf(cell);
        // DOWN
        while (index >= 0
            && _cellsList.Value[index].GetComponent<SpriteRenderer>().sprite.Equals(cell.GetComponent<SpriteRenderer>().sprite))
        {
            tempList.Add(_cellsList.Value[index]);
            index -= _gameConfigs.Columns;
        }
        if (tempList.Count > 2)
            list.AddRange(tempList);

        tempList.Clear();
        index = _cellsList.Value.IndexOf(cell);
        float yCoord = _cellsList.Value[index].transform.position.y;
        // LEFT
        while (index >= 0
            && _cellsList.Value[index].transform.position.y == yCoord
            && _cellsList.Value[index].GetComponent<SpriteRenderer>().sprite.Equals(cell.GetComponent<SpriteRenderer>().sprite))
        {
            tempList.Add(_cellsList.Value[index]);
            index--;
        }
        index = _cellsList.Value.IndexOf(cell);
        // RIGHT
        while (index < _cellsList.Value.Count
            && _cellsList.Value[index].transform.position.y == yCoord
            && _cellsList.Value[index].GetComponent<SpriteRenderer>().sprite.Equals(cell.GetComponent<SpriteRenderer>().sprite))
        {
            tempList.Add(_cellsList.Value[index]);
            index++;
        }
        if (tempList.Count > 2)
            list.AddRange(tempList);

        return list;
    }
}