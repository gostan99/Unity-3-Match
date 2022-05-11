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
    public UnityEvent OnCellExplode = new UnityEvent();

    [SerializeField] private ListGameObjectVariable _cellsList;
    [SerializeField] private ListGameObjectVariable _cellSpawnPositions;
    [SerializeField] private IntVariable _clickedCellIndex; // Clicked cell index
    [SerializeField] private GameConfigs _gameConfigs;

    private List<GameObject> _swappedCells = new List<GameObject>();

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
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
        var hit = Physics2D.Raycast(mouseRay.origin, mouseRay.direction, Vector2.Distance(transform.position, mouseRay.origin), ~layer); // ~layer mean we ignore all other layer except Cell's layer
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

        // player haven't selected any cell
        if (_clickedCellIndex.Value == -1)
        {
            _clickedCellIndex.Value = newClickedCellIndex;
            OnSelectCell?.Invoke();
            //LeanTween.resume(_leanTweenFadingEffectID);
            return;
        }

        // play selected the same cell they have selected in previous
        if (_clickedCellIndex.Value == newClickedCellIndex)
        {
            //StopSelectedCellSpriteFadingEffect();
            OnDeselectCell?.Invoke();
            _clickedCellIndex.Value = -1;
            return;
        }

        // the cell player just select is the same as previous selected cell
        if (!AreNeighbors(_clickedCellIndex.Value, newClickedCellIndex))
        {
            OnDeselectCell?.Invoke();
            _clickedCellIndex.Value = newClickedCellIndex;
            OnSelectCell?.Invoke();
            return;
        }

        var oldSelectedCell = _cellsList.Value[_clickedCellIndex.Value];
        var newSelectedCell = _cellsList.Value[newClickedCellIndex];
        _clickedCellIndex.Value = -1;
        // raise this event before swapping because we don't want the selected cell to fade while swapping
        OnDeselectCell?.Invoke();
        SwapCell(oldSelectedCell, newSelectedCell);
    }

    // return true if A and B are neighbor to each other
    private bool AreNeighbors(int indexA, int indexB)
    {
        int up = indexA + _gameConfigs.Columns;
        int down = indexA - _gameConfigs.Columns;
        int left = indexA - 1;
        int right = indexA + 1;

        return indexB == up || indexB == down || indexB == left || indexB == right;
    }

    private void SwapCellIndex(GameObject cellA, GameObject cellB)
    {
        int indexA = _cellsList.Value.IndexOf(cellA);
        int indexB = _cellsList.Value.IndexOf(cellB);
        _cellsList.Value[indexA] = cellB;
        _cellsList.Value[indexB] = cellA;
    }

    // Swap cell world pos and their index in the cells list
    public void SwapCell(GameObject cellA, GameObject cellB, bool raiseEventOnFinished = true)
    {
        var duration = Vector2.Distance(cellA.transform.position, cellB.transform.position) / _gameConfigs.CellMoveSpeed;
        LeanTween.move(cellA, cellB.transform.position, duration);
        LeanTween.move(cellB, cellA.transform.position, duration).setOnComplete(() =>
        {
            SwapCellIndex(cellA, cellB);
            if (raiseEventOnFinished)
            {
                _swappedCells.Add(cellA);
                _swappedCells.Add(cellB);
                OnCellSwapped?.Invoke();
            }
        });
    }

    public void OnCellSwappedResponse()
    {
        var matchedCellIndices = new List<int>();
        foreach (var cell in _swappedCells)
        {
            var list = GetMatchedCellIndices(cell);
            matchedCellIndices.AddRange(list);
        }
        if (matchedCellIndices.Count < 3)
            SwapCell(_swappedCells[0], _swappedCells[1], false); // Undo swap
        else
        {
            Explode(matchedCellIndices);
            //OnCellExplode?.Invoke();
            Colapse(matchedCellIndices);
        }
        _swappedCells.Clear();
    }

    private void Colapse(List<int> indicies)
    {
        indicies.Sort();
        var queue = new Queue<int>(indicies);
        int indexA;
        int indexB = -1;

        while (queue.Count > 0)
        {
            indexA = queue.Dequeue();
            if (indexB == -1)
                indexB = indexA + _gameConfigs.Columns;
            else if (_cellsList.Value[indexA].transform.position.x == _cellsList.Value[indexB].transform.position.x)
                indexB += _gameConfigs.Columns;
            else
                indexB = indexA + _gameConfigs.Columns;

            while (true)
            {
                if (indexB >= _gameConfigs.NumCell)
                {
                    var cellA = _cellsList.Value[indexA];
                    var cellARenderer = cellA.GetComponent<SpriteRenderer>();
                    cellARenderer.sprite = CellGenerator.Instance.GetRandomCellSprite();

                    var moveFrom = _cellsList.Value[indexB].transform.position;
                    var moveTo = cellA.transform.position;
                    cellA.transform.position = moveFrom;
                    float duration = Vector2.Distance(moveFrom, moveTo) / _gameConfigs.CellMoveSpeed;
                    LeanTween.move(cellA, moveTo, duration);
                    break;
                }

                var cellB = _cellsList.Value[indexB];
                var cellBRenderer = cellB.GetComponent<SpriteRenderer>();
                if (cellBRenderer.sprite != null)
                {
                    var cellA = _cellsList.Value[indexA];
                    var cellARenderer = cellA.GetComponent<SpriteRenderer>();
                    cellARenderer.sprite = cellBRenderer.sprite;
                    cellBRenderer.sprite = null;
                    queue.Enqueue(indexB);

                    var moveFrom = cellB.transform.position;
                    var moveTo = cellA.transform.position;
                    cellA.transform.position = moveFrom;
                    float duration = Vector2.Distance(moveFrom, moveTo) / _gameConfigs.CellMoveSpeed;
                    LeanTween.move(cellA, moveTo, duration);
                    break;
                }
                else
                {
                    indexB += _gameConfigs.Columns;
                }
            }
        }
    }

    private void Explode(List<int> indicies)
    {
        foreach (var index in indicies)
        {
            var cell = _cellsList.Value[index];
            var renderer = cell.GetComponent<SpriteRenderer>();
            renderer.sprite = null;
        }
    }

    private List<int> GetMatchedCellIndices(GameObject cell)
    {
        var indices = new List<int>();
        int index = _cellsList.Value.IndexOf(cell);

        var buffer = new HashSet<int>();
        // UP
        while (index < _gameConfigs.NumCell
            && _cellsList.Value[index].GetComponent<SpriteRenderer>().sprite.Equals(cell.GetComponent<SpriteRenderer>().sprite))
        {
            buffer.Add(index);
            index += _gameConfigs.Columns;
        }
        // reset value
        index = _cellsList.Value.IndexOf(cell);
        // DOWN
        while (index >= 0
            && _cellsList.Value[index].GetComponent<SpriteRenderer>().sprite.Equals(cell.GetComponent<SpriteRenderer>().sprite))
        {
            buffer.Add(index);
            index -= _gameConfigs.Columns;
        }
        // 3-match
        if (buffer.Count > 2)
            indices.AddRange(buffer);

        // reset value
        buffer.Clear();
        index = _cellsList.Value.IndexOf(cell);
        float yCoord = cell.transform.position.y;
        // LEFT
        while (index >= 0
            && _cellsList.Value[index].transform.position.y == yCoord
            && _cellsList.Value[index].GetComponent<SpriteRenderer>().sprite.Equals(cell.GetComponent<SpriteRenderer>().sprite))
        {
            buffer.Add(index);
            index--;
        }
        // reset value
        index = _cellsList.Value.IndexOf(cell);
        // RIGHT
        while (index < _gameConfigs.NumCell
            && _cellsList.Value[index].transform.position.y == yCoord
            && _cellsList.Value[index].GetComponent<SpriteRenderer>().sprite.Equals(cell.GetComponent<SpriteRenderer>().sprite))
        {
            buffer.Add(index);
            index++;
        }
        // 3-match
        if (buffer.Count > 2)
            indices.AddRange(buffer);

        return indices;
    }
}