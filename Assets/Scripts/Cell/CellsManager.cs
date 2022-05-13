using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static Cell;

public class CellsManager : Singleton<CellsManager>
{
    //public UnityEvent OnCellSwapFinished = new UnityEvent();
    //public UnityEvent OnCellFallDownFinished = new UnityEvent();
    //public UnityEvent OnCellExplode = new UnityEvent();

    [SerializeField] private ListGameObjectVariable _cellsList;
    [SerializeField] private GameConfigs _gameConfigs;
    [SerializeField] private IntVariable _score;

    private List<GameObject> _swappedCells = new List<GameObject>();
    private bool _canReceiveInput = true;
    private GameObject _selectedCell = null;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        var list = GetAllCellCanBlowUp();
        Debug.Log($"Cell left to blow up is: {list.Count}");
    }

    // Update is called once per frame
    private void Update()
    {
        //if (_selectedCell)
        //    Debug.Log(_cellsList.Value.IndexOf(_selectedCell));
        if (_canReceiveInput)
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
            if (_selectedCell)
            {
                _selectedCell.GetComponent<Cell>().StopFade();
            }
            _selectedCell = null;
            return;
        }

        var newSelectedCell = hit.transform.gameObject;

        // player haven't selected any cell
        if (!_selectedCell)
        {
            _selectedCell = newSelectedCell;
            _selectedCell.GetComponent<Cell>().StartFade();
            return;
        }

        // play selected the same cell they have selected in previous
        if (newSelectedCell == _selectedCell)
        {
            _selectedCell.GetComponent<Cell>().StopFade();
            _selectedCell = null;
            return;
        }

        // the cell player just select is the same as previous selected cell
        if (!AreNeighbors(_selectedCell, newSelectedCell))
        {
            _selectedCell.GetComponent<Cell>().StopFade();
            _selectedCell = newSelectedCell;
            _selectedCell.GetComponent<Cell>().StartFade();

            return;
        }

        _selectedCell.GetComponent<Cell>().StopFade();
        SwapCell(_selectedCell, newSelectedCell);
        _selectedCell = null;
    }

    // return true if A and B are neighbor to each other
    private bool AreNeighbors(GameObject cellA, GameObject cellB)
    {
        float xPosA = (float)System.Math.Round(cellA.transform.position.x, 2);
        float xPosB = (float)System.Math.Round(cellB.transform.position.x, 2);
        float yPosA = (float)System.Math.Round(cellA.transform.position.y, 2);
        float yPosB = (float)System.Math.Round(cellB.transform.position.y, 2);

        if (Mathf.Approximately(xPosA, xPosB)) // in the same column
        {
            int indexA = _cellsList.Value.IndexOf(cellA);
            int indexB = _cellsList.Value.IndexOf(cellB);
            int up = indexA + _gameConfigs.Columns;
            int down = indexA - _gameConfigs.Columns;
            return indexB == up || indexB == down;
        }
        else if (Mathf.Approximately(yPosA, yPosB))  // in the same row
        {
            int indexA = _cellsList.Value.IndexOf(cellA);
            int indexB = _cellsList.Value.IndexOf(cellB);
            int left = indexA - 1;
            int right = indexA + 1;
            return indexB == left || indexB == right;
        }
        return false;
    }

    // Swap cell world pos and their index in the cells list
    private void SwapCell(GameObject cellA, GameObject cellB)
    {
        _swappedCells.Clear();
        _canReceiveInput = false;
        _swappedCells.Add(cellA);
        _swappedCells.Add(cellB);

        SwapIndex(cellA, cellB);

        var cellAMoveTo = cellB.transform.position;
        var cellBMoveTo = cellA.transform.position;

        var cellACmp = cellA.GetComponent<Cell>();
        var cellBCmp = cellB.GetComponent<Cell>();
        cellACmp.Move(cellAMoveTo);
        cellBCmp.Move(cellBMoveTo);
        void onFinished()
        {
            _canReceiveInput = true;
            OnCellSwapFinishedResponse();
            cellBCmp.OnMoveFinished -= onFinished;
        };
        cellBCmp.OnMoveFinished += onFinished;
    }

    private void SwapIndex(GameObject cellA, GameObject cellB)
    {
        int indexA = _cellsList.Value.IndexOf(cellA);
        int indexB = _cellsList.Value.IndexOf(cellB);

        _cellsList.Value[indexA] = cellB;
        _cellsList.Value[indexB] = cellA;
    }

    private void UndoSwapCell()
    {
        _canReceiveInput = false;
        var cellA = _swappedCells[0];
        var cellB = _swappedCells[1];

        SwapIndex(cellA, cellB);

        var cellAMoveTo = cellB.transform.position;
        var cellBMoveTo = cellA.transform.position;

        var cellACmp = cellA.GetComponent<Cell>();
        var cellBCmp = cellB.GetComponent<Cell>();
        cellACmp.Move(cellAMoveTo);
        cellBCmp.Move(cellBMoveTo);

        void onFinished()
        {
            _canReceiveInput = true;
            cellBCmp.OnMoveFinished -= onFinished;
        };
        cellBCmp.OnMoveFinished += onFinished;
    }

    private void OnCellSwapFinishedResponse()
    {
        var matchedCells = new List<GameObject>();
        foreach (var cell in _swappedCells)
        {
            var list = GetMatchedCells(cell);
            matchedCells.AddRange(list);
        }

        if (matchedCells.Count < 3)
        {
            // The guide does not tell us to undo swap if there is no match
            //UndoSwapCell();
        }
        else
        {
            int bonus = (matchedCells.Count - 3) * 10;
            _score.Value += 100 + bonus;
            _swappedCells.Clear();
            //OnCellExplode?.Invoke();
            Explode(matchedCells);
            FallDown();
        }

        var left = GetAllCellCanBlowUp();
        Debug.Log($"Cell left to blow up is: {left.Count}");
    }

    // fall down to fill up empty space
    private void FallDown()
    {
        var emptyList = new List<GameObject>();
        for (int i = 0; i < _gameConfigs.NumCell; i++)
        {
            var cell = _cellsList.Value[i];
            var renderer = cell.GetComponent<SpriteRenderer>();
            if (renderer.sprite is null)
            {
                int upper = i + _gameConfigs.Columns;
                while (upper < _cellsList.Value.Count)
                {
                    var upperCell = _cellsList.Value[upper];
                    var upperRenderer = upperCell.GetComponent<SpriteRenderer>();
                    if (upperRenderer.sprite != null)
                    {
                        renderer.sprite = upperRenderer.sprite;
                        upperRenderer.sprite = null;

                        if (upper >= _gameConfigs.NumCell)
                            emptyList.Add(upperCell);

                        var moveTo = cell.transform.position;
                        var cellCmp = cell.GetComponent<Cell>();
                        cell.transform.position = upperCell.transform.position;
                        cellCmp.Move(moveTo);

                        break;
                    }
                    upper += _gameConfigs.Columns;
                }
                if (upper >= _gameConfigs.NumCell)
                {
                    //cell.GetComponent<Cell>().AssignRandomSprite();
                }
            }
        }

        foreach (var cell in emptyList)
        {
            cell.GetComponent<Cell>().AssignRandomSprite();
        }
    }

    private void OnCellFallDownFinishedResponse()
    {
        _canReceiveInput = true;
    }

    private void Explode(List<GameObject> matchedCellIndicies)
    {
        foreach (var cell in matchedCellIndicies)
        {
            var renderer = cell.GetComponent<SpriteRenderer>();
            renderer.sprite = null;
        }
    }

    // return indicies of cell that can be blow up
    private HashSet<int> GetAllCellCanBlowUp()
    {
        HashSet<int> cells = new HashSet<int>();

        GameObject cellA;
        int indexA = 0;
        SpriteRenderer rendererA;
        var buffer = new List<int>();

        // Horizontal
        for (int i = 0; i < _gameConfigs.Rows; i++)
        {
            cellA = _cellsList.Value[indexA];
            rendererA = cellA.GetComponent<SpriteRenderer>();
            buffer.Clear();
            buffer.Add(indexA);
            int indexB = indexA + 1;

            while (indexB % _gameConfigs.Columns < _gameConfigs.Columns && indexB < _gameConfigs.NumCell)
            {
                var cellB = _cellsList.Value[indexB];
                var rendererB = cellB.GetComponent<SpriteRenderer>();

                if (rendererB.sprite == rendererA.sprite)
                {
                    buffer.Add(indexB);
                }
                else
                {
                    if (buffer.Count >= 3)
                    {
                        buffer.ForEach(index => cells.Add(index));
                    }
                    buffer.Clear();
                    cellA = cellB;
                    rendererA = rendererB;
                    indexA = indexB;
                    buffer.Add(indexA);
                }
                indexB++;
            }
            indexA += _gameConfigs.Columns;
        }

        indexA = 0;
        // Virtical
        for (int i = 0; i < _gameConfigs.Columns; i++)
        {
            cellA = _cellsList.Value[indexA];
            rendererA = cellA.GetComponent<SpriteRenderer>();
            buffer.Clear();
            buffer.Add(indexA);
            int indexB = indexA + _gameConfigs.Columns;

            while (indexB < _gameConfigs.NumCell)
            {
                var cellB = _cellsList.Value[indexB];
                var rendererB = cellB.GetComponent<SpriteRenderer>();

                if (rendererB.sprite == rendererA.sprite)
                {
                    buffer.Add(indexB);
                }
                else
                {
                    if (buffer.Count >= 3)
                    {
                        buffer.ForEach(cell => cells.Add(cell));
                    }
                    buffer.Clear();
                    cellA = cellB;
                    rendererA = rendererB;
                    indexA = indexB;
                    buffer.Add(indexA);
                }
                indexB += _gameConfigs.Columns;
            }
            indexA++;
        }

        return cells;
    }

    private List<GameObject> GetMatchedCells(GameObject cell)
    {
        var cells = new List<GameObject>();
        int index = _cellsList.Value.IndexOf(cell);
        var buffer = new List<GameObject>();
        var sprite = cell.GetComponent<SpriteRenderer>().sprite;

        int temp = index;
        float yCoord = cell.transform.position.y;

        // LEFT
        while (temp >= 0
            && Mathf.Approximately(_cellsList.Value[temp].transform.position.y, yCoord)
            && _cellsList.Value[temp].GetComponent<SpriteRenderer>().sprite == sprite)
        {
            if (temp != index)
                buffer.Add(_cellsList.Value[temp]);
            temp--;
        }

        // reset value
        temp = index;
        // RIGHT
        while (temp < _gameConfigs.NumCell
            && Mathf.Approximately(_cellsList.Value[temp].transform.position.y, yCoord)
            && _cellsList.Value[temp].GetComponent<SpriteRenderer>().sprite == sprite)
        {
            if (temp != index)
                buffer.Add(_cellsList.Value[temp]);
            temp++;
        }

        // 3-match
        if (buffer.Count >= 2)
        {
            cells.AddRange(buffer);
        }

        // reset value
        buffer.Clear();
        temp = index;
        // UP
        while (temp < _gameConfigs.NumCell
            && _cellsList.Value[temp].GetComponent<SpriteRenderer>().sprite == sprite)
        {
            if (temp != index)
                buffer.Add(_cellsList.Value[temp]);
            temp += _gameConfigs.Columns;
        }

        // reset value
        temp = index;
        // DOWN
        while (temp >= 0
            && _cellsList.Value[temp].GetComponent<SpriteRenderer>().sprite == sprite)
        {
            if (temp != index)
                buffer.Add(_cellsList.Value[temp]);
            temp -= _gameConfigs.Columns;
        }

        // 3-match
        if (buffer.Count >= 2)
        {
            cells.AddRange(buffer);
        }
        cells.Add(cell);
        if (cells.Count < 3)
        {
            cells.Clear();
        }

        return cells;
    }
}