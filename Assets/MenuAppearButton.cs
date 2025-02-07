using UnityEngine;
using UnityEngine.InputSystem;

public class MenuAppearButton : MonoBehaviour
{
    [SerializeField] InputActionReference menuInputActionReference;
    [SerializeField] GameObject smallMenu;

    private void OnEnable()
    {
        menuInputActionReference.action.started += ChangeMenuState;
    }
    private void OnDisable()
    {
        menuInputActionReference.action.started -= ChangeMenuState;
    }
    void ChangeMenuState(InputAction.CallbackContext obj)
    {
        smallMenu.SetActive(!smallMenu.activeSelf);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Zatrzymuje tylko Play Mode w edytorze
#else
    Application.Quit(); // Zamyka grę w buildzie
#endif
        
    }
}
