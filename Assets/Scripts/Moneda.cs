
using UnityEngine;

public class Moneda : MonoBehaviour
{
    public int valor = 1;

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (!otro.CompareTag("Player"))
            return;

        if (FirebaseManager.Instance == null ||
            !FirebaseManager.Instance.listo)
            return;

        FirebaseManager.Instance.AgregarMonedas(valor);

        // Eliminar la moneda de la escena.
        Destroy(gameObject);
    }
}
