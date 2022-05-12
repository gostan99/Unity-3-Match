using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class CellsManager : Singleton<CellsManager>
{
    public UnityEvent OnCellSwapFinished = new UnityEvent();
    public UnityEvent OnCellFallDownFinished = new UnityEvent();
    public UnityEvent OnCellExplode = new UnityEvent();

    [SerializeField] private ListGameObjectVariable _cellsList;
    [SerializeField] private GameConfigs _gameConfigs;

    private List<GameObject> _emptyCellsToStartFallDown = new List<GameObject>();
    private List<GameObject> _cellsThatIsFallingDown = new List<GameObject>();
    private List<GameObject> _swappedCells = new List<GameObject>();
    private bool _canReceiveInput = true;
    private GameObject _selectedCell = null;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    private void Update()
    {
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
        int indexA = _cellsList.Value.IndexOf(cellA);
        int indexB = _cellsList.Value.IndexOf(cellB);
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
    private void SwapCell(GameObject cellA, GameObject cellB)
    {
        _canReceiveInput = false;
        _swappedCells.Add(cellA);
        _swappedCells.Add(cellB);
        var cellACmp = cellA.GetComponent<Cell>();
        var cellBCmp = cellB.GetComponent<Cell>();
        cellACmp.Move(cellB.transform.position);
        cellBCmp.Move(cellA.transform.position);
        void onFinished()
        {
            _canReceiveInput = true;
            OnCellSwapFinishedResponse();
            cellACmp.OnMoveFinished.RemoveListener(onFinished);
        };
        cellACmp.OnMoveFinished.AddListener(onFinished);
        SwapCellIndex(cellA, cellB);
        // var duration = Vector2.Distance(cellA.transform.position, cellB.transform.position) / _gameConfigs.CellMoveSpeed;
        // cellA.transform.DOMove(cellB.transform.position, duration).SetEase(Ease.OutCubic);
        // cellB.transform.DOMove(cellA.transform.position, duration).SetEase(Ease.OutCubic).OnComplete(() =>
        //{
        //    _canReceiveInput = true;
        //    OnCellSwapFinishedResponse();
        //});
    }

    private void UndoSwapCell()
    {
        _canReceiveInput = false;
        var cellA = _swappedCells[0];
        var cellB = _swappedCells[1];
        _swappedCells.Clear();

        var cellACmp = cellA.GetComponent<Cell>();
        var cellBCmp = cellB.GetComponent<Cell>();
        cellACmp.Move(cellB.transform.position);
        cellBCmp.Move(cellA.transform.position);
        void onFinished()
        {
            _canReceiveInput = true;
            cellBCmp.OnMoveFinished.RemoveListener(onFinished);
        };
        cellBCmp.OnMoveFinished.AddListener(onFinished);
        SwapCellIndex(cellA, cellB);
        //_cellsToCheckForMatch.Clear();
        //SwapCellIndex(cellA, cellB);
        //var duration = Vector2.Distance(cellA.transform.position, cellB.transform.position) / _gameConfigs.CellMoveSpeed;
        //cellA.transform.DOMove(cellB.transform.position, duration).SetEase(Ease.OutCubic);
        //cellB.transform.DOMove(cellA.transform.position, duration).SetEase(Ease.OutCubic).OnComplete(() =>
        //{
        //    _canReceiveInput = true;
        //});
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
            UndoSwapCell();
        }
        else
        {
            //OnCellExplode?.Invoke();
            Explode(matchedCells);
            FallDown();
        }
    }

    // fall down to fill up empty space
    private void FallDown()
    {
        _canReceiveInput = false;
        _cellsThatIsFallingDown.Clear();
        foreach (var cell in _emptyCellsToStartFallDown)
        {
            int indexA = _cellsList.Value.IndexOf(cell);
            int indexB = indexA + _gameConfigs.Columns;
            var queue = new Queue<int>();
            queue.Enqueue(indexA);

            while (queue.Count > 0)
            {
                indexA = queue.Dequeue();
                _cellsThatIsFallingDown.Add(_cellsList.Value[indexA]);

                while (true)
                {
                    if (indexB >= _gameConfigs.NumCell)
                    {
                        var cellA = _cellsList.Value[indexA];
                        var cellACmp = cellA.GetComponent<Cell>();
                        cellACmp.AssignRandomSprite();

                        var moveFrom = _cellsList.Value[indexB].transform.position;
                        var moveTo = cellA.transform.position;
                        cellA.transform.position = moveFrom;
                        cellACmp.Move(moveTo);
                        void onFinished()
                        {
                            _cellsThatIsFallingDown.Remove(cellA);
                            cellACmp.OnMoveFinished.RemoveListener(onFinished);
                        }
                        cellACmp.OnMoveFinished.AddListener(onFinished);
                        indexB += _gameConfigs.Columns;
                        break;
                    }

                    var cellB = _cellsList.Value[indexB];
                    var cellBRenderer = cellB.GetComponent<SpriteRenderer>();
                    if (cellBRenderer.sprite != null)
                    {
                        var cellA = _cellsList.Value[indexA];
                        var cellACmp = cellA.GetComponent<Cell>();
                        var cellARenderer = cellA.GetComponent<SpriteRenderer>();
                        cellARenderer.sprite = cellBRenderer.sprite;
                        cellBRenderer.sprite = null;
                        queue.Enqueue(indexB);

                        var moveFrom = cellB.transform.position;
                        var moveTo = cellA.transform.position;
                        cellA.transform.position = moveFrom;
                        cellACmp.Move(moveTo);
                        void onFinished()
                        {
                            _cellsThatIsFallingDown.Remove(cellA);
                            cellACmp.OnMoveFinished.RemoveListener(onFinished);
                        }
                        cellACmp.OnMoveFinished.AddListener(onFinished);

                        indexB += _gameConfigs.Columns;
                        break;
                    }
                    else
                    {
                        queue.Enqueue(indexB);
                        indexB += _gameConfigs.Columns;
                    }
                }
            }
        }
        _emptyCellsToStartFallDown.Clear();
        StartCoroutine(DoCheckFallDownFinished());
    }

    private IEnumerator DoCheckFallDownFinished()
    {
        while (_cellsThatIsFallingDown.Count > 0)
        {
            yield return null;
        }
        //OnCellFallDownFinished?.Invoke();
        OnCellFallDownFinishedResponse();
    }

    private void OnCellFallDownFinishedResponse()
    {
        //TODO: check all cell to blow up

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
            _emptyCellsToStartFallDown.AddRange(buffer);
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
            cells.Clear();

        if (buffer.Count > 0)
        {
            var bottomCell = buffer[buffer.Count - 1];
            if (!Mathf.Approximately(cell.transform.position.x, bottomCell.transform.position.x))
            {
                _emptyCellsToStartFallDown.Add(bottomCell);
                _emptyCellsToStartFallDown.Add(cell);
            }
            else
            {
                if (index < _cellsList.Value.IndexOf(bottomCell))
                    _emptyCellsToStartFallDown.Add(cell);
                else
                    _emptyCellsToStartFallDown.Add(bottomCell);
            }
        }
        else
        {
            _emptyCellsToStartFallDown.Add(cell);
        }

        return cells;
    }
}