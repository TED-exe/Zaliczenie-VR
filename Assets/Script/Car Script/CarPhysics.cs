using System.Linq;
using UnityEngine;

public class CarPhysics : MonoBehaviour
{
    [Header("Car Setting")]
        [SerializeField] private Transform[] allWheelsTransform;
        [SerializeField] private Transform[] steeringWheelsTransform;
        [SerializeField] private Transform[] drivingWheelsTransform;
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private Rigidbody carRigidbody;
        [SerializeField] private bool controlAI;
    
        [Header("Suspension Setting")]
        [SerializeField] private float springLeanght;
        [SerializeField] private float springStrenght;
        [SerializeField] private float springDamper;
    
        [Header("Controll Setting")]
        [SerializeField] private float carAcceleration;
        [SerializeField] private float carBackAcceleration;
        [SerializeField] private AnimationCurve powerCurve;
        [SerializeField] private AnimationCurve powerBackCurve;
        [SerializeField] private float carMaxSpeed;
        [SerializeField] private float carMaxBackSpeed;
        [SerializeField] private float wheelsRotateSpeed;
        [SerializeField] private float wheelsRotateBackSpeed;
        [SerializeField, Range(0, 1)] private float wheelesGripFactor;
        [SerializeField, Range(0, 1)] private float wheelesMass;
        [SerializeField, Range(0, 45)] public float wheelsRotationLimit = 45;
    
        [Header("Flip Car Setting")]
        [SerializeField] private Transform flipCheckRaycast;
        [SerializeField] private float flipCarCooldown = 2f;
        [SerializeField] private float flipForce;
        [SerializeField] private float flipRotationSpeed;
        private float lastUsedTime;
    
        [Header("ONLY READ DATA")]
        [SerializeField] private bool isBreaking = false;
        private float targetRotation;
        private float horizontalInput;
        private float accelerationInput;
    
        private void OnValidate()
        {
            if(gameObject.TryGetComponent<CarAi>(out var ai) && ai.enabled == true)
            {
                controlAI = true;
                
            }
        }
        private void Update()
        {
            if (!controlAI)
            {
                GetInput();
            }
        }
        private void FixedUpdate()
        {
            WheelsRotate();
            for (int i = 0; i < allWheelsTransform.Length; i++)
            {
    
                if (CheckTireTouchGround(allWheelsTransform[i]).succes)
                {
                    SuspesionCalculate(allWheelsTransform[i], CheckTireTouchGround(allWheelsTransform[i]).hit);
                    Steering(allWheelsTransform[i]);
                    Acceleration(allWheelsTransform[i]);
    
                }
                else
                {
                    Debug.DrawLine(allWheelsTransform[i].position, allWheelsTransform[i].position + Vector3.down * springLeanght, Color.white);
                }
            }
        }
        private (bool succes, RaycastHit hit) CheckTireTouchGround(Transform selectedTireTransform)
        {
            if (Physics.Raycast(selectedTireTransform.position, Vector3.down, out var hitPoint, springLeanght, whatIsGround))
                return (succes: true, hit: hitPoint);
            else
                return (succes: false, hit: hitPoint);
        }
        private void SuspesionCalculate(Transform selectedTireTransform, RaycastHit hitPlace)
        {
            Vector3 springDir = selectedTireTransform.up;
            Vector3 tireWorldVel = carRigidbody.GetPointVelocity(selectedTireTransform.position);
    
            float offset = springLeanght - hitPlace.distance;
            float vel = Vector3.Dot(springDir, tireWorldVel);
            float force = (offset * springStrenght) - (vel * springDamper);
    
            carRigidbody.AddForceAtPosition(springDir * force, selectedTireTransform.position);
    
            Debug.DrawLine(selectedTireTransform.position, selectedTireTransform.position + Vector3.down * hitPlace.distance, Color.red);
            Debug.DrawLine(selectedTireTransform.position, selectedTireTransform.position + Vector3.up * force, Color.green);
        }
        public void GetInput(float turnAround = 0, float acceleration = 0)
        {
    
            if (!controlAI)
            {
                horizontalInput = Input.GetAxis("Horizontal");
                accelerationInput = Input.GetAxis("Vertical");
                isBreaking = (Input.GetKey(KeyCode.Space));
                Debug.DrawRay(flipCheckRaycast.position, flipCheckRaycast.up);
                if (Time.time - lastUsedTime >= flipCarCooldown)
                {
                    if (Input.GetKeyDown(KeyCode.R))
                        FlipCar();
                }
            }
            else
            {
                horizontalInput = turnAround;
                accelerationInput = acceleration;
            }
        }
        public void FlipCar()
        {
            if (Physics.Raycast(flipCheckRaycast.position, flipCheckRaycast.up, 0.1f, whatIsGround))
            {
                carRigidbody.AddForce(Vector3.up * flipForce, ForceMode.Impulse);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.identity, flipRotationSpeed * Time.fixedDeltaTime);
            }
    
        }
        private void Steering(Transform selectedWheeleTransform)
        {
            Vector3 steeringDir;
            Debug.DrawRay(selectedWheeleTransform.position, selectedWheeleTransform.forward, Color.blue);
    
            if (selectedWheeleTransform.name.Contains("L"))
            {
                steeringDir = -selectedWheeleTransform.right;
            }
            else
            {
                steeringDir = selectedWheeleTransform.right;
            }
    
            Debug.DrawRay(selectedWheeleTransform.position, steeringDir, Color.red);
    
            Vector3 wheelWorldVel = carRigidbody.GetPointVelocity(selectedWheeleTransform.position);
    
            float steeringVel = Vector3.Dot(steeringDir, wheelWorldVel);
    
            float desiredVelChange = -steeringVel * wheelesGripFactor;
    
            float desiredAcceleration = desiredVelChange / Time.deltaTime;
    
            carRigidbody.AddForceAtPosition(steeringDir * wheelesMass * desiredAcceleration, selectedWheeleTransform.position);
        }
        public void Acceleration(Transform selectedWheeleTransform)
        {
            Vector3 accelerationDir = selectedWheeleTransform.forward;
            float avaibleTorque = 0;
            if (drivingWheelsTransform.Contains(selectedWheeleTransform))
            {
                if (isBreaking)
                {
                    return;
                }
                if (accelerationInput > 0)
                {
                    float carSpeed = Vector3.Dot(transform.forward, carRigidbody.linearVelocity);
    
                    float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carMaxSpeed);
    
                    avaibleTorque = powerCurve.Evaluate(normalizedSpeed) * accelerationInput * carAcceleration;
    
                }
                else if (accelerationInput < 0)
                {
                    float carSpeed = Vector3.Dot(transform.forward, carRigidbody.linearVelocity);
    
                    float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carMaxBackSpeed);
    
                    avaibleTorque = powerBackCurve.Evaluate(normalizedSpeed) * accelerationInput * carBackAcceleration;
    
                }
                carRigidbody.AddForceAtPosition(accelerationDir * avaibleTorque, selectedWheeleTransform.position);
            }
            else
            {
                avaibleTorque = carRigidbody.linearVelocity.magnitude;
            }
        }
        public void WheelsRotate()
        {
            // set rotate value
            if (horizontalInput != 0)
            {
                targetRotation = Mathf.Lerp(targetRotation, wheelsRotationLimit * horizontalInput, wheelsRotateBackSpeed);
            }
            else
            {
                targetRotation = Mathf.MoveTowards(targetRotation, 0f, wheelsRotateBackSpeed * Time.fixedDeltaTime);
            }
    
            //rotate wheeles
            for (int i = 0; i < steeringWheelsTransform.Length; i++)
            {
                steeringWheelsTransform[i].localEulerAngles = new Vector3(0, targetRotation, 0);
            }
        }

        public float GetSpeed()
        {
            return carRigidbody.linearVelocity.magnitude;
        }
        
}
