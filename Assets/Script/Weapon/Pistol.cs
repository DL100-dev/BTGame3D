using UnityEngine;

public class Pistol : Gun
{
    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private bool isInitialized = false; // Thêm biến để kiểm tra khởi tạo

    protected override bool IsShootingInput()
    {
        return Input.GetMouseButtonDown(0);
    }

    public override void Start()
    {
        base.Start(); // Gọi Start() của lớp Gun
        if (!isInitialized)
        {
            originalRotation = transform.localRotation;
            isInitialized = true;
        }
    }

    public override void Update()
    {
        base.Update();
        // Smoothly return to the original rotation
        transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRotation, Time.deltaTime * gunData.recoilRecoverySpeed);
    }

    public override void ApplyRecoil()
    {
        // Apply the upward recoil rotation
        targetRotation = transform.localRotation * Quaternion.Euler(-gunData.recoilForceUpward * 10f, 0f, 0f);
        transform.localRotation = targetRotation;
    }
}