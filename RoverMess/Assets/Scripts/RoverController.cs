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
    public float driftFactor = 0.88f;   // Чем выше, тем сильнее скользит (0 - рельсы, 1 - лед)

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
        else
        {
            // Даже если энергия кончилась, катящийся робот все равно должен стоять ровно
            StabilizeRotation();
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

        // 1. Считаем поворот только по оси Y на основе ввода игрока
        float turnDegrees = turnInput * rotationSpeed * modifier * Time.fixedDeltaTime;

        // Получаем текущий угол по Y и прибавляем к нему смещение
        float newAngleY = transform.eulerAngles.y + turnDegrees;

        // 2. Жестко фиксируем X и Z в 0 градусов, а Y выставляем новый
        Quaternion targetRotation = Quaternion.Euler(0f, newAngleY, 0f);

        rb.MoveRotation(targetRotation);
    }

    // Метод для принудительного выравнивания, если робота наклонило силами PhysX при ударе
    void StabilizeRotation()
    {
        Quaternion stableRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        rb.MoveRotation(stableRotation);
    }

    void ApplyDrift()
    {
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
        Vector3 rightVelocity = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);

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