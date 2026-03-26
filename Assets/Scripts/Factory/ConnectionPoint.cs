using UnityEngine;

namespace Factory.Factory
{
    /// <summary>
    /// Attach this to child GameObjects on machine prefabs that represent entry (Input)
    /// or exit (Output) ports. The BuildManager will snap ghost output ports to nearby
    /// placed input ports (and vice-versa) when within snap range.
    /// </summary>
    public class ConnectionPoint : MonoBehaviour
    {
        public enum PointType { Input, Output }

        [Tooltip("Is this an item input or output?")]
        public PointType pointType = PointType.Output;

        private void OnDrawGizmos()
        {
            Gizmos.color = pointType == PointType.Output ? Color.green : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
            Gizmos.DrawRay(transform.position, transform.forward * 0.5f);
        }
    }
}
