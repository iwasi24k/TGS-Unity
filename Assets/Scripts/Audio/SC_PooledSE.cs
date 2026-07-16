using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SC_PooledSE : MonoBehaviour
{
    private AudioSource _audioSource;
    private Action<GameObject> _returnToPoolAction;

    private bool _isPlaying;
    private bool _isReturned;

    private float _defaultVolume;
    private float _defaultPitch;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        _defaultVolume = _audioSource.volume;
        _defaultPitch = _audioSource.pitch;
    }

    public void RegisterReturnAction(Action<GameObject> returnAction)
    {
        _returnToPoolAction = returnAction;
    }

    public void Play()
    {
        Play(_defaultVolume, _defaultPitch);
    }

    public void Play(float volume, float pitch = 1f)
    {
        _isReturned = false;
        _isPlaying = true;

        _audioSource.volume = Mathf.Clamp01(volume);
        _audioSource.pitch = Mathf.Clamp(pitch, -3f, 3f);

        _audioSource.Play();
    }

    private void Update()
    {
        if (!_isPlaying || _isReturned)
        {
            return;
        }

        // AudioSourceの再生が終わったらプールへ返す
        if (!_audioSource.isPlaying)
        {
            ReturnToPool();
        }
    }

    public void StopAndReturn()
    {
        if (_isReturned)
        {
            return;
        }

        _audioSource.Stop();
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (_isReturned)
        {
            return;
        }

        _isReturned = true;
        _isPlaying = false;

        // 一時的に変更した値を戻す
        _audioSource.volume = _defaultVolume;
        _audioSource.pitch = _defaultPitch;

        _returnToPoolAction?.Invoke(gameObject);
    }
}
