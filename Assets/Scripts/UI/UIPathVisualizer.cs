using UnityEngine;
using System.Linq;

namespace Boardgame.UI
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class UIPathVisualizer : MonoBehaviour
    {

        MeshFilter mFilter;

        [SerializeField]
        float linkerWidth = 1f;

        [SerializeField]
        float pointSize = 1.5f;

        [SerializeField]
        float endPointSize = 1.5f;

        [SerializeField, Range(1, 180)]
        float endPointPointAngle = 40;

        [SerializeField]
        float linkerPointDistance = 0.1f;


        static float centerToCornerFactor = Mathf.Sqrt(2);

        static Vector2[] LinkerUV = new Vector2[]
            {
                new Vector2(0.5f, 0.3f), new Vector2(0.2f, 0.3f), new Vector3(0.2f, 0.7f),
                new Vector2(0.5f, 0.3f), new Vector2(0.2f, 0.7f), new Vector2(0.5f, 0.7f),
                new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.7f), new Vector2(0.8f, 0.7f),
                new Vector2(0.5f, 0.3f), new Vector2(0.8f, 0.7f), new Vector2(0.8f, 0.3f)
            };

        static Vector2[] PointUV = new Vector2[] {
                new Vector2(0.1f, 0.1f), new Vector2(0.1f, 0.25f), new Vector2(0.25f, 0.25f),
                new Vector2(0.1f, 0.1f), new Vector2(0.25f, 0.25f), new Vector2(0.25f, 0.1f)
            };


        void OnEnable()
        {
            Pather.OnPathChange += HandleNewPath;
        }

        void OnDisable()
        {
            Pather.OnPathChange -= HandleNewPath;
        }

        private void HandleNewPath(Tile[] path, PathAction action)
        {
            
            mFilter.mesh.Clear();

            Vector3[] verts = new Vector3[Mathf.Max(0, (path.Length - 1) * 12) + Mathf.Max(0, (path.Length - 1) * 6)];
            Vector2[] uv = new Vector2[verts.Length];

            int pos = 0;
            for (int i=0; i<path.Length - 1; i++)
            {
                System.Array.Copy(GetLinker(path[i].PathPoint, path[i + 1].PathPoint, path[i].transform.up, i == path.Length - 2), 0, verts, pos, 12);
                System.Array.Copy(LinkerUV, 0, uv, pos, 12);
                pos += 12;
                if (i > 0)
                {
                    System.Array.Copy(GetMidPoint(path[i].PathPoint, path[i - 1].PathPoint, path[i + 1].PathPoint, path[i].transform.up), 0, verts, pos, 6);
                    System.Array.Copy(PointUV, 0, uv, pos, 6);
                    pos += 6;
                }

                if (i == path.Length - 2)
                {
                    System.Array.Copy(GetEndPoint(path[i].PathPoint, path[i + 1].PathPoint, path[i + 1].transform.up), 0, verts, pos, 6);
                    System.Array.Copy(PointUV, 0, uv, pos, 6);
                    pos += 6;

                }
            }

            mFilter.mesh.vertices = verts;
            mFilter.mesh.triangles = Enumerable.Range(0, verts.Length).ToArray();
            mFilter.mesh.uv = uv;
            mFilter.mesh.RecalculateNormals();
            mFilter.mesh.RecalculateBounds();
        }

        void Start()
        {
            mFilter = GetComponent<MeshFilter>();
            mFilter.mesh = new Mesh();
        }

        Vector3[] GetLinker(Vector3 from, Vector3 to, Vector3 rotaionAxis, bool finalLinker)
        {
            float pointCenter2Corner = centerToCornerFactor * pointSize + linkerPointDistance;
            float pointCenter2CornerFinal = centerToCornerFactor * endPointSize + linkerPointDistance;

            Vector3 direction = (to - from).normalized;

            var sideVectorToRotate = 0.5f * centerToCornerFactor * linkerWidth * direction;

            var fromOffset = from + direction * pointCenter2Corner;
            var fromOffsetL = fromOffset + Quaternion.AngleAxis(135, rotaionAxis) * sideVectorToRotate;
            var fromOffsetR = fromOffset + Quaternion.AngleAxis(225, rotaionAxis) * sideVectorToRotate;

            var toOffset = to - direction * (finalLinker ? pointCenter2CornerFinal : pointCenter2Corner);
            var toOffsetL = toOffset +  Quaternion.AngleAxis(45, rotaionAxis) * sideVectorToRotate;
            var toOffsetR = toOffset + Quaternion.AngleAxis(-45, rotaionAxis) * sideVectorToRotate;

            return new Vector3[] {
                fromOffset, toOffsetL, fromOffsetL,
                fromOffset, toOffset,  toOffsetL, 
                fromOffset, toOffsetR, toOffset,
                fromOffset, fromOffsetR, toOffsetR, 
            };
            
        }

        Vector3[] GetMidPoint(Vector3 center, Vector3 inPoint, Vector3 outPoint, Vector3 rotationAxis)
        {
            float pointCenter2Corner = centerToCornerFactor * pointSize;
            Vector3 directionIn = (inPoint - center).normalized;
            Vector3 directionOut = (outPoint - center).normalized;
            Vector3 sumV = directionIn + directionOut;
            Vector3 betweenVector;

            if (sumV.sqrMagnitude != 0)
            {
                if (Vector3.Dot(Vector3.Cross(directionIn, directionOut), rotationAxis) < 0f)
                {
                    var tmp = directionIn;
                    directionIn = directionOut;
                    directionOut = tmp;
                }
                betweenVector = sumV.normalized * pointCenter2Corner;
            }
            else
            {

                betweenVector = Quaternion.AngleAxis(90, rotationAxis) * directionIn * pointCenter2Corner;
            }
            
            return new Vector3[] {
                center + directionIn * pointCenter2Corner, center + betweenVector, center + directionOut * pointCenter2Corner,
                center + directionIn * pointCenter2Corner, center + directionOut * pointCenter2Corner, center - betweenVector
            };

        }

        Vector3[] GetEndPoint(Vector3 from, Vector3 to, Vector3 rotationAxis)
        {
            var direction = (from - to).normalized;
            var centerToEdgeV = endPointSize * centerToCornerFactor * direction;

            return new Vector3[] {
                to, to + centerToEdgeV, to + Quaternion.AngleAxis(endPointPointAngle/2f, rotationAxis) * centerToEdgeV,
                to, to + Quaternion.AngleAxis(-endPointPointAngle/2f, rotationAxis) * centerToEdgeV, to + centerToEdgeV
            };
        }

    }
}