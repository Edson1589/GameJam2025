using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuSelector : MonoBehaviour
{
    [SerializeField] private Button firstButton;

    void Start()
    {
        SelectFirstButton();
    }

    void Update()
    {
        // Si no hay nada seleccionado, volver a seleccionar el primer botón
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            SelectFirstButton();
        }
    }

    private void SelectFirstButton()
    {
        if (firstButton != null && EventSystem.current != null)
        {
            firstButton.Select();
            Debug.Log($"✓ Botón seleccionado: {firstButton.name}");
        }
    }
}