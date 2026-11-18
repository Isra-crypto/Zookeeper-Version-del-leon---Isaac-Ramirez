using UnityEngine;

namespace ZooGame.CameraControl
{
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        [Header("Camera View Settings")]
        [Tooltip("Tamaño ortográfico de la cámara (mayor valor = cámara más alejada).")]
        [SerializeField] private float orthographicSize = 6.5f;

        [Header("Follow Settings")]
        [Tooltip("Si está activo, utiliza interpolación suave sin saltos de píxel. Si está inactivo, sigue 1:1 de forma directa al jugador.")]
        [SerializeField] private bool smoothFollow = false;
        [SerializeField] private float smoothTime = 0.15f;

        private Camera cam;
        private Vector3 currentVelocity;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        public float OrthographicSize
        {
            get => orthographicSize;
            set
            {
                orthographicSize = value;
                ApplyCameraSize();
            }
        }

        private void Awake()
        {
            cam = GetComponent<Camera>();
            ApplyCameraSize();
        }

        private void OnValidate()
        {
            if (cam == null) cam = GetComponent<Camera>();
            ApplyCameraSize();
        }

        private void ApplyCameraSize()
        {
            if (cam != null && cam.orthographic)
            {
                cam.orthographicSize = orthographicSize;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = new Vector3(target.position.x + offset.x, target.position.y + offset.y, offset.z);

            if (smoothFollow)
            {
                transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
            }
            else
            {
                transform.position = desiredPosition;
            }
        }
    }
}
