using UnityEngine;

public class RoverController : MonoBehaviour
{
    [Header("Настройки движения")]
    public float moveSpeed = 15f;       // Сила тяги мотора
    public float maxSpeed = 10f;        // Максимальная скорость
    public float rotationSpeed = 140f;

    [Header("Настройки Nitro (Shift)")]
    public float nitroSpeedMultiplier = 1.8f;
    public float nitroMaxSpeed = 18f;

    [Header("Физика заноса")]
    [Range(0f, 1f)]
    public float driftFactor = 0.88f;   // Чем выше, тем сильнее скользит (0 - рельсы, 1 - лед). Оставим легкий скользящий вайб.

    private Rigidbody rb;
    private RoverEnergy energySystem;

    private float moveInput;
    private float turnInput;
    private bool isNitro;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        energySystem = GetComponent<RoverEnergy>();

        rb.linearDamping = 1f;
        rb.angularDamping = 5f;
    }

    void Update()
    {
        if (energySystem != null && !energySystem.HasEnergy())
        {
            moveInput = 0f;
            turnInput = 0f;
            isNitro = false;
            return;
        }

        // Сбор ввода
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;

            // Вперед/Назад
            moveInput = 0f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveInput = 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveInput = -1f;

            // Влево/Вправо
            turnInput = 0f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) turnInput = 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) turnInput = -1f;

            // Нитро (Shift)
            isNitro = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
        }
    }

    void FixedUpdate()
    {
        if (energySystem != null && energySystem.HasEnergy())
        {
            MoveRover();
            TurnRover();
            ApplyDrift();
            HandleEnergyConsumption();
        }
    }

    void MoveRover()
    {
        float currentMaxSpeed = isNitro ? nitroMaxSpeed : maxSpeed;
        float currentEngineForce = isNitro ? (moveSpeed * nitroSpeedMultiplier) : moveSpeed;

        if (rb.linearVelocity.magnitude < currentMaxSpeed)
        {
            Vector3 forwardForce = transform.forward * moveInput * currentEngineForce;
            rb.AddForce(forwardForce, ForceMode.Acceleration);
        }
    }

    void TurnRover()
    {
        float modifier = 1f;
        if (rb.linearVelocity.magnitude > 0.5f && moveInput < 0)
        {
            modifier = -1f;
        }

        float turnDegrees = turnInput * rotationSpeed * modifier * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turnDegrees, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    void ApplyDrift()
    {
        // Разделяем скорость на продольную и боковую
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
        Vector3 rightVelocity = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);

        // Постоянно применяем легкий занос при поворотах
        rb.linearVelocity = forwardVelocity + rightVelocity * driftFactor;
    }

    void HandleEnergyConsumption()
    {
        if (energySystem == null) return;

        if (isNitro && Mathf.Abs(moveInput) > 0.1f)
        {
            energySystem.ConsumeEnergy(energySystem.nitroDrain);
        }
        else if (Mathf.Abs(moveInput) > 0.1f || Mathf.Abs(turnInput) > 0.1f)
        {
            energySystem.ConsumeEnergy(energySystem.normalDrain);
        }
        else
        {
            energySystem.ConsumeEnergy(energySystem.idleDrain);
        }
    }
}