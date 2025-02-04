using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


public class JoystickGrab : MonoBehaviour
{
    [SerializeField] private GameObject joystickParent;
    
    [SerializeField] private float maxInteractorDistance = 0.707f;
    [SerializeField] private float toleranceAfterMaxThreshold = 0.2f;
    
    [SerializeField] private Transform reference;
    
    
    private XRGrabInteractable _grabInteractable;

    

    public static Vector2 JoystickInput { get; private set; }
    
    
    private bool _isGrabbing = true;
    
    private const string Default_Layer = "Default";
    private const string Grab_Layer = "Grab";

    [SerializeField] private Transform visualJoystick;
    [SerializeField] private float maxTiltAngle = 30f;   
    private Quaternion _initialVisualJoystickRotation;
    
    private void Start()
    {
        
        _grabInteractable = GetComponent<XRGrabInteractable>();
        _grabInteractable.selectEntered.RemoveAllListeners();
        _grabInteractable.selectEntered.AddListener(OnGrabbed);
        _grabInteractable.selectExited.RemoveAllListeners();
        _grabInteractable.selectExited.AddListener(OnGrabEnd);
        
        
        if (visualJoystick != null) _initialVisualJoystickRotation = visualJoystick.localRotation;
    }

    private void OnGrabEnd(SelectExitEventArgs arg0) => _isGrabbing = false;
    private void OnGrabbed(SelectEnterEventArgs arg0) => _isGrabbing = true;

    private void FixedUpdate()
    {
        if (!_isGrabbing)
        {
            JoystickInput = Vector2.zero;
            transform.position = reference.position;
            ChangeLayerMask(Grab_Layer);
            return;
        }
       
        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(reference.position, currentPosition);
        if (distance > maxInteractorDistance + toleranceAfterMaxThreshold)
        {
            ChangeLayerMask(Default_Layer);
            _isGrabbing = false;
            transform.position = reference.position;
            return;
        }
        
        
        Vector3 difference = currentPosition - reference.position;
        JoystickInput = CalculateInput(difference);
    }

    private Vector2 CalculateInput(Vector3 difference)
    {
        Vector2 diffXZ = new Vector2(difference.x, difference.z);
        float currentMagnitude = diffXZ.magnitude;
        if (currentMagnitude > maxInteractorDistance) diffXZ = diffXZ.normalized * maxInteractorDistance;
        Vector2 input = diffXZ / maxInteractorDistance;

        return input;
    }

    private void Update()
    {
        Debug.Log(JoystickInput);
        if (visualJoystick != null)
        {
            float tiltX = JoystickInput.x * maxTiltAngle;
            float tiltZ = JoystickInput.y * maxTiltAngle;
        
            visualJoystick.localRotation = _initialVisualJoystickRotation * Quaternion.Euler(tiltX, -tiltZ, 0f);
        }
    }

    private void ChangeLayerMask(string mask)
    {
        _grabInteractable.interactionLayers = InteractionLayerMask.GetMask(mask);
    }
}
