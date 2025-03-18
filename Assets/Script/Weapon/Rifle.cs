using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Rifle : Gun
{
    protected override bool IsShootingInput()
    {
        return Input.GetMouseButton(0); // Kiểm tra chuột trái có đang được giữ hay không
    }

    public override void Update()
    {
        base.Update();
        // Rifle-specific update logic can be added here if needed
    }

    public override void ApplyRecoil()
    {
        targetRecoilRotation += new Vector3(-gunData.recoilForceUpward, Random.Range(-5f, 5f), 0f);
        transform.localPosition -= Vector3.forward * gunData.recoilForceBackward;
    }
}