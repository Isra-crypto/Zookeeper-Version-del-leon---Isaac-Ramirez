using UnityEngine;

namespace ZooGame.Care
{
    public class CloudMover : MonoBehaviour
    {
        [SerializeField] private float speed = 0.35f;
        [SerializeField] private float resetX = 14f;
        [SerializeField] private float startX = -14f;

        private void Update()
        {
            transform.position += Vector3.right * (speed * Time.deltaTime);
            if (transform.position.x > resetX)
            {
                transform.position = new Vector3(startX, transform.position.y, transform.position.z);
            }
        }
    }
}
