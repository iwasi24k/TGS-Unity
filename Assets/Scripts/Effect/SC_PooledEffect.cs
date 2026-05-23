using UnityEngine;
using System;

[RequireComponent(typeof(ParticleSystem))]
public class SC_PooledEffect : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private Action<GameObject> _returnToPoolAction;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();

        // Call the script's callback when the particle ends
        var mainModule = _particleSystem.main;
        mainModule.stopAction = ParticleSystemStopAction.Callback;
    }

    /// <summary>
    /// Register a delegate when returning the pool
    /// </summary>
    public void RegisterReturnAction(Action<GameObject> returnAction)
    {
        _returnToPoolAction = returnAction;
    }

    private void OnParticleSystemStopped()
    {
        _returnToPoolAction?.Invoke(gameObject);
    }
}
