using CG.Core;
using OpenTK.Mathematics;

namespace CG
{
    internal class MazeCell
    {

        public int x,z;
        public int wallFlags = 15;
        public bool isKey = false;
        public bool isExit = false;
        public MazeDirection exitDirection;
        public MazeCell(int _x, int _z)
        {
            x = _x;
            z = _z;
        }

        public void AddWall(MazeDirection direction)
        {
            wallFlags = wallFlags | (int)direction;
        }
        public Vector3 CameraPosition() => new(x,0.5f,z);
        public List<GameObject> GenerateGameObjects()
        {
            List<GameObject> result = new List<GameObject>();
            // Ground
            Mesh groundMesh = Mesh.CreatePrimitivePlane(1);
            GameObject groundGameObj = new GameObject(groundMesh,(int)TextureIdx.Ground);
            groundGameObj.transform.position = new Vector3(x,0f,z);
            result.Add(groundGameObj);
            // Ceiling
            Mesh ceilingMesh = Mesh.CreatePrimitivePlane(1);
            GameObject ceilingGameObj = new GameObject(ceilingMesh,(int)TextureIdx.Ceiling);
            ceilingGameObj.transform.position = new Vector3(x,1f,z);
            result.Add(ceilingGameObj);
            // Walls
            int flags = WallsToRender();
            List<MazeDirection> directions = new List<MazeDirection>{ MazeDirection.North, MazeDirection.East, MazeDirection.South, MazeDirection.West };
            foreach (MazeDirection direction in directions)
            {
                if ((flags & (int)direction) != 0)
                    result.Add(GenerateWallGameObject(direction));
            }

            return result;
        }
        public GameObject GenerateWallGameObject(MazeDirection direction)
        {
            Mesh mesh = Mesh.CreatePrimitivePlane(1);
            GameObject result = new GameObject(mesh,(int)TextureIdx.Wall);
            switch (direction)
            {
                case MazeDirection.North:
                {
                    result.transform.position = new Vector3(x,0.5f,z-0.5f);
                    result.transform.rotation = new Vector3(90f,0f,0f);
                    break;
                }
                case MazeDirection.East:
                {
                    result.transform.position = new Vector3(x+0.5f,0.5f,z);
                    result.transform.rotation = new Vector3(0f,-90f,90f);
                    break;
                }
                case MazeDirection.South:
                {;
                    result.transform.position = new Vector3(x,0.5f,z+0.5f);
                    result.transform.rotation = new Vector3(90f,0f,0f);
                    break;
                }
                default:
                {
                    result.transform.position = new Vector3(x-0.5f,0.5f,z);
                    result.transform.rotation = new Vector3(0f,90f,-90f);
                    break;
                }
            }
            return result;
        }
        public MazeDirection GetDirectionTo(MazeCell otherCell)
        {
            if(x < otherCell.x) return MazeDirection.East;
            if(x > otherCell.x) return MazeDirection.West;
            if(z < otherCell.z) return MazeDirection.South;
            return MazeDirection.North;
        }
        public bool HasWall(MazeDirection direction)
        {
            return (wallFlags & (int)direction) != 0;
        }
        public void PickUpKey() => SetIsKey(false);
        public void RemoveWall(MazeDirection direction)
        {
            wallFlags = wallFlags & ~(int)direction;
        }
        public void RemoveWallTo(MazeCell otherCell) => RemoveWall(GetDirectionTo(otherCell));
        public void SetIsKey(bool value) => isKey = value;
        public void SetupExit(List<MazeDirection> possibleExitDirections)
        {
            isExit = true;
            if (possibleExitDirections.Count == 0) return;
            exitDirection = possibleExitDirections[RandomManager.Integer(possibleExitDirections.Count)];
        }
        public int WallsToRender()
        {
            int wallsToRenderFlags = wallFlags;
            if (x > 0)
                wallsToRenderFlags = wallsToRenderFlags & ~(int)MazeDirection.West;
            if (z > 0)
                wallsToRenderFlags = wallsToRenderFlags & ~(int)MazeDirection.North;
            return wallsToRenderFlags;
        }
    }
}