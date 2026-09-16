using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;

    void LateUpdate()
    {
        Vector3 targetPosition = _playerTransform.position;
        transform.position = new Vector3(
                targetPosition.x,
                targetPosition.y,
                transform.position.z
            );
    }
}
