using UnityEngine;

public class BasketSensor : MonoBehaviour
{
    [SerializeField]
    private bool sensorEntrada;

    private void OnTriggerEnter(Collider other)
    {
        BasketballBall bola =
            other.GetComponentInParent<BasketballBall>();

        if (bola == null)
            return;

        bola.PassarSensor(sensorEntrada);
    }
}
