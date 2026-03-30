using UnityEngine;

public class HandHeldCardFollower : MonoBehaviour
{
    [Header("Offsets inside hand slot")]
    [SerializeField] private Vector3 localPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 localRotationOffset = Vector3.zero;

    [Header("Smoothing")]
    [SerializeField] private bool useSmoothing = false;
    [SerializeField] private float followSpeed = 18f;
    [SerializeField] private float rotateSpeed = 18f;

    private Transform targetSlot;
    private bool attached;

    public void AttachToSlot(Transform slot)
    {
        if (slot == null) return;

        targetSlot = slot;
        attached = true;

        // Сразу делаем дочерним объектом слота
        transform.SetParent(slot, worldPositionStays: false);
        transform.localPosition = localPositionOffset;
        transform.localRotation = Quaternion.Euler(localRotationOffset);
    }

    public void Detach(Transform newParent = null)
    {
        attached = false;
        targetSlot = null;
        transform.SetParent(newParent, worldPositionStays: true);
    }

    private void LateUpdate()
    {
        if (!attached || targetSlot == null) return;

        if (!useSmoothing)
        {
            transform.localPosition = localPositionOffset;
            transform.localRotation = Quaternion.Euler(localRotationOffset);
            return;
        }

        Vector3 targetPos = localPositionOffset;
        Quaternion targetRot = Quaternion.Euler(localRotationOffset);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * followSpeed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * rotateSpeed);
    }
}
