namespace KFrameCompliant
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;
    using System.IO;

    [ExecuteInEditMode]
    public class Layout : MonoBehaviour
    {
        [SerializeField] private bool _cullExcessChildren;

        [Space(3)]

        [Header("Layout Properties")]

        [SerializeField] [Range(0, 360)] private float _angle;
        [SerializeField] private ushort _columns;
        [SerializeField] private float _margin;
        [SerializeField] private float _marginColumn;
        [SerializeField] private float _marginRow;
        [SerializeField] private Vector3 _plane;
        [SerializeField] private float _radius;
        [SerializeField] private bool _reverse = false;
        [SerializeField] private LayoutType _layoutType;
        [SerializeField] private bool _fill = false;

        private Transform[] _children;

        private readonly Vector3 _cachedZeroVector = Vector3.zero;

        private struct LayoutJSON
        {
            public float angle;
            public int columns;
            public float margin;
            public float marginColumn;
            public float marginRow;
            public int[] plane;
            public float radius;
            public bool reverse;
            public bool fill;
            public string type;

            public LayoutJSON(float Angle, int Columns, float Margin, float MarginColumn, float MarginRow, Vector3 Plane, float Radius, bool Reverse, bool Fill, LayoutType Type)
            {
                angle = Angle;
                columns = Columns;
                margin = Margin;
                marginColumn = MarginColumn;
                marginRow = MarginRow;
                plane = new int[3] { (int)Plane.x, (int)Plane.y, (int)Plane.z };
                radius = Radius;
                reverse = Reverse;
                fill = Fill;
                type = Type.ToString();
            }
        }

        public void LoadJSON(string jsonString)
        {
            LayoutJSON data = JsonUtility.FromJson<LayoutJSON>(jsonString);

            _angle = data.angle;
            _columns = (ushort)data.columns;
            _margin = data.margin;
            _marginColumn = data.marginColumn;
            _marginRow = data.marginRow;
            _radius = data.radius;
            _reverse = data.reverse;
            _fill = data.fill;

            _layoutType = (LayoutType)System.Enum.Parse(typeof(LayoutType), data.type);
            _plane = new Vector3(data.plane[0], data.plane[1], data.plane[2]);
        }

        public void SaveJSON()
        {
            string path = "C:/LayoutJSONs/";
            string fileName = System.DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            LayoutJSON fileContents = new LayoutJSON(_angle, _columns, _margin, _marginColumn, _marginRow, _plane, _radius, _reverse, _fill, _layoutType);

            File.WriteAllText(path + fileName, JsonUtility.ToJson(fileContents), System.Text.Encoding.UTF8);
        }

#region Private Methods

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            if (ChildrenHaveChanged())
            {
                CacheChildren();
                CalculateLayoutShape();
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                CalculateLayoutShape();
            }
#endif
        }

        private void Init()
        {
            CacheChildren();

            CalculateLayoutShape();
        }

        private void CacheChildren()
        {
            _children = new Transform[transform.childCount];
            for (int i = 0; i < _children.Length; i++)
            {
                _children[i] = transform.GetChild(i);
            }
        }

        private bool ChildrenHaveChanged()
        {
            if (_children.Length != transform.childCount) return true;

            for (int i = 0; i < _children.Length; i++)
            {
                if (_children[i].parent != transform)
                {
                    return true;
                }
            }

            return false;
        }

        private void CalculateLayoutShape()
        {
            Vector3[] positions;

            switch (_layoutType)
            {
                case LayoutType.Box:
                {
                    positions = GetBoxPositions();
                    break;
                }

                case LayoutType.Circle:
                {
                    positions = GetCirclePositions();
                    break;
                }

                case LayoutType.Cube:
                {
                    positions = GetCubePositions();
                    break;
                }

                case LayoutType.Dodecahedron:
                {
                    positions = GetDodecahedronPositions();
                    break;
                }

                case LayoutType.Pyramid:
                {
                    positions = GetPyramidPositions();
                    break;
                }

                default:
                {
                    positions = GetLinePositions();
                    break;
                }
            }

            SetLayoutShape(positions);
        }

        private void SetLayoutShape(Vector3[] inputPositions)
        {
            Vector3[] actualPositions = inputPositions;

            if (_reverse)
            {
                System.Array.Reverse(actualPositions);
            }

            int amount = _children.Length > actualPositions.Length ? actualPositions.Length : _children.Length;

            for (int i = 0; i < amount; i++)
            {
                //print("Setting " + _children[i].name + " position to " + positions[i]);
                _children[i].localPosition = actualPositions[i];
            }

            for (int i = 0; i < _children.Length; i++)
            {
                if (i >= amount && _cullExcessChildren)
                {
                    _children[i].gameObject.SetActive(false);
                }
                else
                {
                    _children[i].gameObject.SetActive(true);
                } 
            }
        }

        private Vector3[] GetBoxPositions()
        {
            List<Vector3> positions = new List<Vector3>();

            int rows = Mathf.CeilToInt(_children.Length / _columns);

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < _columns; column++)
                {
                    Vector3 position = _cachedZeroVector;

                    if (_plane.x == 0)
                    {
                        position.x = column * _marginColumn;
                    }

                    if (_plane.y == 0)
                    {
                        position.y = column * _marginColumn;
                    }

                    if (_plane.y == 1)
                    {
                        position.y = row * _marginRow;
                    }

                    if (_plane.z == 1)
                    {
                        position.z = row * _marginRow;
                    }

                    positions.Add(position);
                }
            }

            return positions.ToArray();
        }

        private Vector3[] GetCirclePositions()
        {
            List<Vector3> positions = new List<Vector3>();

            for (int i = 0; i < _children.Length; i++)
            {

                float radius;

                if (_angle == 0)
                {
                    radius = i * (2 * Mathf.PI) / _children.Length;
                }
                else
                {
                    radius = i* _angle *0.01745329252f;
                }

                Vector3 position = _cachedZeroVector;

                if (_plane.x == 0)
                {
                    position.x = _radius * Mathf.Cos(radius);
                }

                if (_plane.y == 0)
                {
                    position.y = _radius * Mathf.Cos(radius);
                }
                else if (_plane.y == 1)
                {
                    position.y = _radius * Mathf.Sin(radius);
                }

                if (_plane.z == 1)
                {
                    position.z = _radius * Mathf.Sin(radius);
                }

                positions.Add(position);
            }

            return positions.ToArray();
        }

        private Vector3[] GetCubePositions()
        {
            Vector3[] positions = new Vector3[6]
            {
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 1),
                new Vector3(-1, 0, 0),
                new Vector3(0, -1, 0),
                new Vector3(0, 0, -1),
            };

            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] *= _radius / 2;
            }

            return positions;
        }

        private Vector3[] GetDodecahedronPositions()
        {
            float PHI = (1 + Mathf.Sqrt(5)) / 2;
            float B = 1 / PHI;
            float C = 2 - PHI;
            float NB = -1 * B;
            float NC = -1 * C;

            Vector3[] positions = new Vector3[20]
            {
                new Vector3(-1, C, 0),
                new Vector3(-1, NC, 0),
                new Vector3(0, -1, C),
                new Vector3(0, -1, NC),
                new Vector3(0, 1, C),
                new Vector3(0, 1, NC),
                new Vector3(1, C, 0),
                new Vector3(1, NC, 0),
                new Vector3(B, B, B),
                new Vector3(B, B, NB),
                new Vector3(B, NB, B),
                new Vector3(B, NB, NB),
                new Vector3(C, 0, 1),
                new Vector3(C, 0, -1),
                new Vector3(NB, B, B),
                new Vector3(NB, B, NB),
                new Vector3(NB, NB, B),
                new Vector3(NB, NB, NB),
                new Vector3(NC, 0, 1),
                new Vector3(NC, 0, -1)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] *= _radius / 2;
            }

            return positions;
        }

        private Vector3[] GetLinePositions()
        {
            _columns = (ushort)_children.Length;
            return GetBoxPositions();
        }

        private Vector3[] GetPyramidPositions()
        {
            float SQRT_3 = Mathf.Sqrt(3);
            float NEG_SQRT_1_3 = -1 / Mathf.Sqrt(3);
            float DBL_SQRT_2_3 = 2 * Mathf.Sqrt(2 / 3);

            Vector3[] positions = new Vector3[4]
            {
                new Vector3(0, 0, SQRT_3 + NEG_SQRT_1_3),
                new Vector3(-1, 0, NEG_SQRT_1_3),
                new Vector3(1, 0, NEG_SQRT_1_3),
                new Vector3(0, DBL_SQRT_2_3, 0),
            };

            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] *= _radius / 2;
            }

            return positions;
        }

#endregion
    }

    public enum LayoutType
    {
        Box,
        Circle,
        Cube,
        Dodecahedron,
        Line,
        Pyramid
    }
}
