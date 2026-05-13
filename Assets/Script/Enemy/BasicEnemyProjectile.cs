using System;
using UnityEngine;

public class BasicEnemyProjectile : MonoBehaviour
{
        public float speed = 50f;
        private void Update()
        {
                transform.position += transform.forward * speed * Time.deltaTime;
        }

        private void OnCollisionEnter(Collision collision)
        {
                LayerMask todestroyLayer = GlobalReference.Instance.playerLayer | GlobalReference.Instance.TerrainLayer;
                if(collision.gameObject.layer == todestroyLayer) Destroy(gameObject);
        }
}