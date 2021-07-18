using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlatformerGame
{
    public class HookBullet : Bullet
    {
        LineRenderer lineRenderer;
        protected override void Awake()
        {
            base.Awake();
            lineRenderer = GetComponent<LineRenderer>();
        }

        protected override void Start()
        {
            Destroy(gameObject, 4);
        }

        public override void Update()
        {
            base.Update();
            if (lineRenderer && OwnerTransform)
            {
                int size = 2;
                lineRenderer.positionCount = size;

                Vector3[] points = new Vector3[size];

                points[0] = OwnerTransform.position + new Vector3(0.25f, 0, 0);
                points[1] = transform.position;

                lineRenderer.SetPositions(points);
            }
        }
    }
}