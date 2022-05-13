using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public delegate void OnPauseDelegate();

    public delegate void OnRestartDelegate();

    public static event OnPauseDelegate OnPause;

    public static event OnRestartDelegate OnRestart;

    // Start is called before the first frame update
    private void Start()
    {
        CellsManager.OnNoCellCanBlowUp += Pause;
    }

    public void Pause()
    {
        OnPause?.Invoke();
        Time.timeScale = 0f;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public void Reset()
    {
        DOTween.Clear(true);
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //OnRestart?.Invoke();
    }
}