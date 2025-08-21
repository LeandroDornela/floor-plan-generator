using BuildingGenerator;
using UnityEngine;

public class ScaleAnimation : MonoBehaviour
{
    public float _animationDuration = 1;
    public float _interval = 0.1f;
    public float _baseStartDelay = 0;
    public float _randomDelay = 0.3f;
    public AnimationCurve _animationCurve;
    //public bool playOnStart = false;
    //public float baseStartDelay = 1f;

    private float _timer = float.MaxValue;
    private Transform _transform;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _transform = transform;
        _transform.localScale = Vector3.zero;
    }


    // Update is called once per frame
    void Update()
    {
        if (!Application.isPlaying) return;

        if (_timer < _animationDuration)
        {
            float value = _animationCurve.Evaluate(_timer / _animationDuration);
            Vector3 scale = new Vector3(value, value, value);
            _transform.localScale = scale;
            _timer += Time.deltaTime;

            if (_timer >= _animationDuration)
            {
                _transform.localScale = Vector3.one;
            }
        }
    }


    void StartAnimation()
    {
        _timer = 0;
    }


    public void TriggerAnimation(float delay)
    {
        if (!Application.isPlaying) return;

        Invoke("StartAnimation", delay*_interval + _baseStartDelay + Utils.Random.RandomRange(0, _randomDelay));
    }
}
