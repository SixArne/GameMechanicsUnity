using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionCone : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] [Range(0, 360 )] float _viewAngle = 45.0f;
    [SerializeField] [Range(0, 200 )] float _viewRadius = 10.0f;
    [SerializeField] LayerMask _targetMask;
    [SerializeField] LayerMask _obstacleMask;
    
    const string _deathTag = "dead";
    public List<Transform> _visibleTargets = new List<Transform>();

    public float ViewRadius
    {
        get => _viewRadius;
    }

    public float ViewAngle
    {
        get => _viewAngle;
    }

    public List<Transform> VisibleTargets
    {
        get => _visibleTargets;
    }

    public bool IsInSight 
    {
        get => _visibleTargets.Count > 0;
    }

    public void Start()
    {
        StartCoroutine("FindTargetsWithDelay", .2f);
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public void FindVisibleTargets()
    {
        _visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, ViewRadius, _targetMask);

        foreach (Collider t in targetsInViewRadius)
        {
            Transform target = t.transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            // within range
            if (Vector3.Angle(transform.forward, dirToTarget) <= ViewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, _obstacleMask) && target.CompareTag(_deathTag))
                {
                    _visibleTargets.Add(target);
                }
            }
        }        
    }
}
