
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private DocumentReference jugadorDoc;

    private readonly SemaphoreSlim bloqueoGuardado =
        new SemaphoreSlim(1, 1);

    public bool listo { get; private set; }

    public int monedasTotales { get; private set; }

    public int nivel1Progreso { get; private set; }
    public int nivel2Progreso { get; private set; }

    public bool nivel1Completado { get; private set; }
    public bool nivel2Completado { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        try
        {
            DependencyStatus estado =
                await FirebaseApp.CheckAndFixDependenciesAsync();

            if (estado != DependencyStatus.Available)
            {
                Debug.LogError("Firebase no disponible: " + estado);
                return;
            }

            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;

            // Iniciar sesión sin pedir usuario y contraseña.
            if (auth.CurrentUser == null)
            {
                await auth.SignInAnonymouslyAsync();
            }

            string uid = auth.CurrentUser.UserId;

            jugadorDoc = db.Collection("jugadores").Document(uid);

            // Recuperar los datos anteriores.
            await CargarDatos();

            listo = true;

            Debug.Log("Firebase conectado. Jugador: " + uid);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al iniciar Firebase: " + ex);
        }
    }

    private async Task CargarDatos()
    {
        DocumentSnapshot datos = await jugadorDoc.GetSnapshotAsync();

        if (!datos.Exists)
        {
            // Crear registro inicial para un nuevo jugador.
            await GuardarDatos();
            return;
        }

        if (datos.TryGetValue("monedasTotales", out long monedas))
            monedasTotales = (int)monedas;

        if (datos.TryGetValue("nivel1Progreso", out long p1))
            nivel1Progreso = (int)p1;

        if (datos.TryGetValue("nivel2Progreso", out long p2))
            nivel2Progreso = (int)p2;

        if (datos.TryGetValue("nivel1Completado", out bool c1))
            nivel1Completado = c1;

        if (datos.TryGetValue("nivel2Completado", out bool c2))
            nivel2Completado = c2;

        Debug.Log("Datos del jugador cargados correctamente");
    }

    public void AgregarMonedas(int cantidad)
    {
        if (!listo || cantidad <= 0)
            return;

        monedasTotales += cantidad;

        Debug.Log("Monedas actuales: " + monedasTotales);

        _ = GuardarDatos();
    }

    public void ActualizarProgreso(int nivel, int porcentaje)
    {
        if (!listo)
            return;

        porcentaje = Mathf.Clamp(porcentaje, 0, 100);

        if (nivel == 1)
        {
            nivel1Progreso =
                Mathf.Max(nivel1Progreso, porcentaje);
        }
        else if (nivel == 2)
        {
            nivel2Progreso =
                Mathf.Max(nivel2Progreso, porcentaje);
        }
        else
        {
            Debug.LogWarning("Nivel no válido");
            return;
        }

        _ = GuardarDatos();
    }

    public void CompletarNivel(int nivel)
    {
        if (!listo)
            return;

        if (nivel == 1)
        {
            nivel1Progreso = 100;
            nivel1Completado = true;
        }
        else if (nivel == 2)
        {
            nivel2Progreso = 100;
            nivel2Completado = true;
        }
        else
        {
            Debug.LogWarning("Nivel no válido");
            return;
        }

        Debug.Log("Nivel " + nivel + " completado");

        _ = GuardarDatos();
    }

    public async Task GuardarDatos()
    {
        if (jugadorDoc == null)
            return;

        // Evitar escrituras simultáneas del mismo jugador.
        await bloqueoGuardado.WaitAsync();

        try
        {
            var datos = new Dictionary<string, object>
            {
                { "monedasTotales", monedasTotales },
                { "nivel1Progreso", nivel1Progreso },
                { "nivel2Progreso", nivel2Progreso },
                { "nivel1Completado", nivel1Completado },
                { "nivel2Completado", nivel2Completado }
            };

            await jugadorDoc.SetAsync(datos, SetOptions.MergeAll);

            Debug.Log("Datos guardados en Firestore");
        }
        catch (Exception ex)
        {
            Debug.LogError("No se pudieron guardar los datos: " + ex);
        }
        finally
        {
            bloqueoGuardado.Release();
        }
    }
}

