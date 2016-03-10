using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;

public class MagicClock : MonoBehaviour
{
    private Queue<Capture> _clockTransforms;
    int _count = 0;

    [Range(0, 30, order = 1)]
    public int MAX = 4;
    public float captureTime = 0.5f;
    Rigidbody2D rigidBody;
    private bool _movedBack = true;
    private Capture _startPosition;
    private Capture _endPosition;
    public bool _stillLerping;
    private Camera mainCamera;
    public Button btnClock;
    public Image coolDown;
    private bool bntClicked = false;
    private Vector3 temp;
    private NoiseEffect cameraNoise;
    public Transform[] IKPositions;
    private BoyController boyController;
    /// <summary>
    /// The time taken to move from the start to finish positions
    /// </summary>
    private float timeTakenDuringLerp;

    private float coolDownStartTime = 0f;
    private float movingBackStartTime = 0f;
    /// <summary>
    /// How far the object should move when 'space' is pressed
    /// </summary>
    private bool _isLerping = false;
    //The Time.time value when we started the interpolation
    private float _timeStartedLerping;

    private class Capture
    {
        public Capture(Vector3[] _list , bool _flipFcae)
        {
            list = _list;
            flipFace = _flipFcae;
        }
        public Vector3[] list { set; get; }
        public bool flipFace { set; get; }
    }

    void Start()
    {
        mainCamera = Camera.main;
        cameraNoise = Camera.main.gameObject.GetComponent<NoiseEffect>();
        timeTakenDuringLerp = captureTime / 2;
        _count = 0;
        _clockTransforms = new Queue<Capture>();
        boyController = GetComponent<BoyController>();
        rigidBody = GetComponent<Rigidbody2D>();
        _movedBack = true;
        _stillLerping = false;
        bntClicked = false;
        btnClock.onClick.AddListener(btnClicked);
    }

    private void btnClicked()
    {
        bntClicked = true;
    }

    void Update()
    {

        if (bntClicked && !_stillLerping && _count == MAX - 1 && coolDown.fillAmount >= 1f)
        {
            bntClicked = false;
            _stillLerping = true;
            moveBack();
        }
        if (_count == 0 && !(IsInvoking("captureTransform")) && _movedBack)
            InvokeRepeating("captureTransform", 0f, captureTime);
        else
        {
            if (!_movedBack)
            {
                movingBackStartTime = Time.time;
                StartLerping(_clockTransforms.Dequeue());
            }
        }
        if (_stillLerping)
        {
            coolDown.fillAmount = 1f - ((Time.time - movingBackStartTime) / ((MAX - 1) * timeTakenDuringLerp));
        }
        else
        {
            coolDown.fillAmount = (Time.time - coolDownStartTime) / (MAX * captureTime);
        }
        if (_isLerping)
        {

            if (coolDown.fillAmount > 0.2f)
            {
                temp = new Vector3(mainCamera.backgroundColor.r, mainCamera.backgroundColor.g, mainCamera.backgroundColor.b);
                temp = Vector3.Lerp(temp, new Vector3(1f, 1f, 1f), 3 * Time.deltaTime);
                mainCamera.backgroundColor = new Color(temp.x, temp.y, temp.z);
                cameraNoise.grainIntensityMax = temp.x * 4;
            }
            else
            {

                temp = new Vector3(mainCamera.backgroundColor.r, mainCamera.backgroundColor.g, mainCamera.backgroundColor.b);
                temp = Vector3.Lerp(temp, new Vector3(0.5f, 0.5f, 0.5f), 4 * Time.deltaTime);
                mainCamera.backgroundColor = new Color(temp.x, temp.y, temp.z);
                cameraNoise.grainIntensityMax = temp.x;
            }

        }
        bntClicked = false;
    }

    void captureTransform()
    {
        if (!_movedBack)
            return;
        if (_count < MAX - 1)
        {
            if (_count == 0)
                coolDownStartTime = Time.time;
            addTransform();
        }
        else
        {
            _clockTransforms.Dequeue();
            _clockTransforms.Enqueue(getAllBodyPositions());
        }
    }

    bool addTransform()
    {
        _clockTransforms.Enqueue(getAllBodyPositions());
        _count++;
        return true;
    }


    bool moveBack()
    {
        if (_count < MAX - 1)
            return false;
        else
        {
            CancelInvoke("captureTransform");
            _clockTransforms = new Queue<Capture>(_clockTransforms.Reverse());
            _movedBack = false;
            return true;
        }
    }

    void StartLerping(Capture end)
    {
        if (end.flipFace != boyController.flipFacing)
            boyController.flipFace();
        _movedBack = true;
        _isLerping = true;
        _timeStartedLerping = Time.time;

        //We set the start position to the current position, and the finish to 10 spaces in the 'forward' direction
        _startPosition = getAllBodyPositions();
        _endPosition = end;
    }
    void LateUpdate()
    {
        if (_isLerping)
        {
            float timeSinceStarted = Time.time - _timeStartedLerping;
            float percentageComplete = timeSinceStarted / timeTakenDuringLerp;

            lerpAllPositions(percentageComplete);

            if (percentageComplete >= 1.0f)
            {
                _isLerping = false;
                if (_clockTransforms.Count == 0)
                {
                    _count = 0;
                    rigidBody.velocity = Vector2.zero;
                    _clockTransforms.Clear();
                    _stillLerping = false;
                    coolDownStartTime = Time.time;
                    cameraNoise.grainIntensityMax = 0.24f;
                }
                if (_clockTransforms.Count > 0)
                {
                    StartLerping(_clockTransforms.Dequeue());
                }

            }

        }
    }

    private void lerpAllPositions(float percentageComplete)
    {
        transform.position = Vector3.Lerp(_startPosition.list[0], _endPosition.list[0], percentageComplete);
        for (int i = 1; i < IKPositions.Length + 1; i++)
        {
            IKPositions[i-1].position = Vector3.Lerp(_startPosition.list[i], _endPosition.list[i], percentageComplete);
        }
    }

    private Capture getAllBodyPositions()
    {
        Vector3[] list = new Vector3[6];
        list[0] = transform.position;
        Transform t = transform.GetChild(2);
        for (int i = 0; i < t.childCount; i++)
        {
            list[i + 1] = t.GetChild(i).position;
        }
        return new Capture(list , boyController.flipFacing);
    }
}
