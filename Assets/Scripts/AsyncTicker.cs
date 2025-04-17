using System;
using UnityEngine;

namespace BuildingGenerator
{
// Encapsula um meio de executar um metodo de forma assincrona com um intervalo de execução.
public class AsyncTicker : MonoBehaviour
{
    private Action _methodToCall;
    private DateTime _beginTickingTime;
    public float LifeTime => (float)(DateTime.Now - _beginTickingTime).TotalSeconds;

    public static AsyncTicker Instantiate()
    {
        // OBS: o jeito mais facil de se usar contadores e repetições é ter um monobehaviour. E para funcionar é nescessario
        // criar um objecto na cena.
        GameObject go = new GameObject("AsyncTicker");
        return go.AddComponent<AsyncTicker>();
    }

    public void Begin(Action methodToCall, float interval)
    {
        _beginTickingTime = DateTime.Now;
        _methodToCall = methodToCall;
        InvokeRepeating("Tick", 0, interval); // Pode ser subistituido por outro meio.
    }

    void Tick()
    {
        _methodToCall();
    }

    public void End()
    {
        CancelInvoke(_methodToCall.Method.Name);

        if(Application.isPlaying)
        {
            Destroy(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }
}
}
