using UnityEngine;

namespace Factory.Factory
{
    public class DirectionIndicator : MonoBehaviour
    {
        private LineRenderer line;

        private void Start()
        {
            line = gameObject.AddComponent<LineRenderer>();
            line.startWidth = 0.1f;
            line.endWidth = 0.0f;
            line.positionCount = 2;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = Color.cyan;
            line.endColor = new Color(0, 1, 1, 0);
            line.useWorldSpace = false;
            
            // Draw an arrow pointing forward
            line.SetPosition(0, Vector3.up * 0.1f);
            line.SetPosition(1, Vector3.forward * 0.8f + Vector3.up * 0.1f);
        }
    }
}
