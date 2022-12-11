using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CupsMovement : MonoBehaviour
{
    [Header("Positions of the cups")]
    [SerializeField] private Vector3[] _cupsPositions;

    [Header("Cups Gameobjects")]
    [SerializeField] private GameObject[] _cups;

    [Header("White Ball Gameobject")]
    [SerializeField] private GameObject _ball;

    [Header("Cups Animator")]
    [SerializeField] private Animator _cupsAnimator;

    [Header("Cup Swap Attempts")]
    [SerializeField] private int _turns;

    [Header("Cup Movement Delay")]
    [SerializeField] private float _swapDelay;

    [Header("Cup Movement Speed")]
    [SerializeField] private float _speed = 0.5f;

    [Header("ResultsPanel")]
    [SerializeField] private GameObject _resultsPanel;
    
    private int _rand;
    private bool _isMix = false;
    private int _mixAttempts = 4;
    private int _rand1;
    private int _rand2;
    private Vector2 _tempPos;
    private Vector2 _tempPos1;
    private bool _canChoose = false;
    public event System.Action IsWon = default;

    private void Update()
    {
        if(_canChoose == true)
        {
            CupHit();
        }
    }

    private void FixedUpdate()
    {
        if(_isMix == true)
        {
            MixCups();
        }
    }

    public void StartGameOnButtonClick()
    {
        _rand = Random.Range(0, _cups.Length);
        StartCoroutine(StartGame());
    }

    private void BallChangePos()
    {
        _ball.transform.position = new Vector2(_cupsPositions[_rand].x, _cupsPositions[_rand].y - 0.3f);
    }

    private void BallSetParent()
    {
        _ball.transform.parent = _cups[_rand].transform;
    }

    private void BallUnsetParent()
    {
        _ball.transform.parent = null;
    }

    private void TempPosSetter()
    {
        _tempPos = _cups[_rand1].transform.position;
        _tempPos1 = _cups[_rand2].transform.position;
    }

    private void MixCups()
    {
        _cups[_rand1].transform.position = Vector2.MoveTowards(_cups[_rand1].transform.position, _tempPos1, _speed * Time.deltaTime);
        _cups[_rand2].transform.position = Vector2.MoveTowards(_cups[_rand2].transform.position, _tempPos, _speed * Time.deltaTime);
    }

    private IEnumerator StartGame()
    {
        BallChangePos();
        yield return new WaitForSeconds(1f);
        _cupsAnimator.SetBool("CloseCups", true);
        yield return new WaitForSeconds(1f);
        BallSetParent();
        StartCoroutine(CupsMix());
        _isMix = true;
        yield return new WaitForSeconds(_swapDelay * _turns);
        _canChoose = true;
    }

    private void CupHit()
    {
        Vector2 curMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D rayHit = Physics2D.Raycast(curMousePos, Vector2.zero);
        if(Input.GetMouseButtonDown(0) && rayHit.collider.CompareTag("RedCup"))
        {
            if(rayHit.transform.childCount > 0)
            {
                IsWin();
            }
            else
            {
                IsLoose();
            }
        }
    }

    private void IsWin()
    {
        BallUnsetParent();
        _cupsAnimator.SetBool("OpenCups", true);
        _canChoose = false;
        if(IsWon != null)
        {
            IsWon();
        }
        StartCoroutine(ResultsPanelOpenDelay());
    }

    private void IsLoose()
    {
        BallUnsetParent();
        _cupsAnimator.SetBool("OpenCups", true);
        _canChoose = false;
        StartCoroutine(ResultsPanelOpenDelay());
    }

    IEnumerator ResultsPanelOpenDelay()
    {
        yield return new WaitForSeconds(1f);
        OpenResultsPanel();
    }

    private void OpenResultsPanel()
    {
        _resultsPanel.SetActive(true);
    }

    private void Randomizer()
    {
        _rand1 = Random.Range(0, _cups.Length);
        _rand2 = Random.Range(0, _cups.Length);
        while(_rand2 == _rand1)
        {
            _rand2 = Random.Range(0, _cups.Length);
        }
    }

    private IEnumerator CupsMix()
    {
        for(int i = 0; i < _turns; i++)
        {
            Randomizer();
            TempPosSetter();
            yield return new WaitForSeconds(_swapDelay);
        }
        _isMix = false;
    }
}