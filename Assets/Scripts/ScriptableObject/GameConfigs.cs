using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Game Configs")]
public class GameConfigs : ScriptableObject
{
    #region Grid Settings

    [Header("Grid Settings")]
    [SerializeField] private int _columns;

    [SerializeField] private int _rows;

    #endregion Grid Settings

    #region Cell Settings

    [Header("Cell Settings")]
    [SerializeField] private Sprite _cellBackground;

    [Tooltip("Remember to set alpha value to 1 in order to see background!")]
    [SerializeField] private Color[] _cellBackgroundColors;

    [SerializeField] private Sprite[] _cellSprites;

    [SerializeField] private Vector2 _cellDimension;

    [SerializeField] private string _cellLayer;
    [SerializeField] private float _cellMoveSpeed;

    #endregion Cell Settings

    public int Columns => _columns;
    public int Rows => _rows;
    public Sprite CellBackground => _cellBackground;
    public Color[] CellBackgroundColors => _cellBackgroundColors;
    public Sprite[] CellSprites => _cellSprites;
    public Vector2 CellDimension => _cellDimension;
    public string CellLayer => _cellLayer;
    public float CellMoveSpeed => _cellMoveSpeed;
}