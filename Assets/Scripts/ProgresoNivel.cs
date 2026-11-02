
using UnityEngine;

public class ProgresoNivel : MonoBehaviour
{
    [Range(1, 2)]
    public int numeroNivel = 1;

    [Range(0, 100)]
    public int porcentaje = 50;

    public bool esMetaFinal = false;

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (!otro.CompareTag("Player"))
            return;

        if (FirebaseManager.Instance == null ||
            !FirebaseManager.Instance.listo)
            return;

        if (esMetaFinal)
        {
            FirebaseManager.Instance.CompletarNivel(numeroNivel);
        }
        else
        {
            FirebaseManager.Instance.ActualizarProgreso(
                numeroNivel, porcentaje
            );
        }
    }
}
