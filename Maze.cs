using System.Text;
using CG.Core;
using OpenTK.Mathematics;

namespace CG
{
    internal class Maze
    {
        public MazeCell[,] cells;
        int width, depth;
        public MazeCell startingCell;
        public Maze(int _size, bool debug = false) : this(_size,_size, debug) {}
        public Maze(int _width, int _depth, bool debug = false)
        {
            width = Math.Max(_width,2);
            depth = Math.Max(_depth,2);
            cells = new MazeCell[width,depth];
            PopulateGrid();
            Generate(width,depth,debug);
            SetSpecialCells();
        }

        public MazeCell? MoveToCell(int x, int z, MazeDirection direction)
        { 
            int newX=x;
            int newZ=z;
            switch (direction)
            {
                case MazeDirection.North:
                    newZ = z-1;
                    break;
                case MazeDirection.South:
                    newZ = z+1;
                    break;
                case MazeDirection.East:
                    newX = x+1;
                    break;
                case MazeDirection.West:
                    newX = x-1;
                    break;
            }
            if (newX >= 0 && newX < width && newZ >=0 && newZ < depth)
                return cells[newX,newZ];
            return null;
        }

        // Algoritmo de Prim
        // https://weblog.jamisbuck.org/2011/1/10/maze-generation-prim-s-algorithm
        private void Generate(int width, int depth, bool debug = false)
        {
            // Escolhe uma célula aleatória para começar e adiciona seus vizinhos à "fronteira"
            MazeCell startingCell = cells[RandomManager.Integer(width),RandomManager.Integer(depth)];
            List<MazeCell> inMazeCells = new List<MazeCell>{startingCell};
            List<MazeCell> frontierCells = GetNeighbors(startingCell);
            int step = 0;
            if (debug)
                PrintMaze(inMazeCells, frontierCells, $"Step {step}: Inicial");
            while (inMazeCells.Count < (width*depth))
            {
                // Escolhe uma célula da fronteira aleatoriamente
                int nextFrontierIdx = RandomManager.Integer(frontierCells.Count);
                MazeCell nextFrontierCell = frontierCells[nextFrontierIdx];
                frontierCells.RemoveAt(nextFrontierIdx);
                // A partir da célula da fronteira, escolhe uma célula vizinha que já esteja no labirinto 
                List<MazeCell> validCells = GetNeighbors(nextFrontierCell).Intersect(inMazeCells).ToList();
                int validCellIdx = RandomManager.Integer(validCells.Count);
                MazeCell selectedCell = validCells[validCellIdx];
                // Um caminho é "cavado" entre as células selecionadas
                selectedCell.RemoveWallTo(nextFrontierCell);
                nextFrontierCell.RemoveWallTo(selectedCell);
                // Adiciona a nova célula no labirinto, e adiciona novos vizinhos nas células de fronteira
                inMazeCells.Add(nextFrontierCell);
                List<MazeCell> newFrontierCells = GetNeighbors(nextFrontierCell).Except(inMazeCells).Except(frontierCells).ToList();
                frontierCells.AddRange(newFrontierCells);
                if (debug)
                    PrintMaze(inMazeCells,frontierCells,$"Step {step++}");
            }
        }
        private List<MazeCell> GetCorners()
        {
            return
            [
              cells[0,0],
              cells[0,depth-1],
              cells[width-1,0],
              cells[width-1,depth-1]  
            ];
        }
        public List<GameObject> GetGameObjects()
        {
            List<GameObject> result = new List<GameObject>();
            for (int x = 0; x < width; x++)
            {
                for(int z = 0; z < depth; z++)
                {
                    result.AddRange(cells[x,z].GenerateGameObjects());
                }
            }
            return result;
        }
        private List<MazeCell> GetNeighbors(MazeCell cell)
        {
            List<MazeCell> neighbors = new List<MazeCell>();
            int x = cell.x;
            int z = cell.z;
            if (cell.x > 0)
                neighbors.Add(cells[x-1,z]);
            if (cell.x < width-1)
                neighbors.Add(cells[x+1,z]);
            if (cell.z > 0)
                neighbors.Add(cells[x,z-1]);
            if (cell.z < depth-1)
                neighbors.Add(cells[x,z+1]);
            return neighbors;
        }
        public List<MazeDirection> GetPossibleExitDirections(MazeCell cell)
        {
            List<MazeDirection> exitDirections = new List<MazeDirection>();
            if (cell.x == 0) exitDirections.Add(MazeDirection.West);
            if (cell.x == width-1) exitDirections.Add(MazeDirection.East);
            if (cell.z == 0) exitDirections.Add(MazeDirection.North);
            if (cell.z == depth-1) exitDirections.Add(MazeDirection.South);
            return exitDirections;
        }
        private void PopulateGrid()
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    cells[x,z] = new MazeCell(x,z);
                }
            }
        }
        private void PrintMaze(List<MazeCell>? inMaze = null, List<MazeCell>? frontier = null, string header = "") 
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(header))
                sb.AppendLine(header);
            
            for (int z = 0; z < depth; z++)
            {
                // Top walls
                for (int x = 0; x < width; x++)
                {
                    sb.Append(cells[x,z].HasWall(MazeDirection.North) ? "+---" : "+   ");
                }
                sb.AppendLine("+");
                // Left walls
                for (int x = 0; x < width; x++)
                {
                    MazeCell cell = cells[x,z];
                    sb.Append(cell.HasWall(MazeDirection.West) ? "|" : " ");
                    // Mostrar status
                    if (inMaze != null && inMaze.Contains(cell))
                        sb.Append("   ");
                    else if (frontier != null && frontier.Contains(cell))
                        sb.Append(" ? ");
                    else sb.Append(" # ");
                }
                // Rightmost walls
                sb.AppendLine(cells[width-1,z].HasWall(MazeDirection.East) ? "|" : " ");
            }

            // Bottom walls
            for (int x = 0; x < width; x++)
            {
                sb.Append(cells[x,depth-1].HasWall(MazeDirection.South) ? "+---" : "+   ");
            }
            sb.AppendLine("+");
            Console.WriteLine(sb.ToString());
            Console.WriteLine("Pressione qualquer tecla para continuar...");
            Console.ReadKey(true);
        }
        private void SetSpecialCells()
        {
            List<MazeCell> corners = GetCorners();
            int startingCellIdx = RandomManager.Integer(corners.Count);
            startingCell = corners[startingCellIdx];
            corners.RemoveAt(startingCellIdx);
            int keyCellIdx = RandomManager.Integer(corners.Count);
            corners[keyCellIdx].SetIsKey(true);
            corners.RemoveAt(keyCellIdx);
            int exitCellIdx = RandomManager.Integer(corners.Count);
            corners[exitCellIdx].SetupExit(GetPossibleExitDirections(corners[exitCellIdx]));
        }
    }

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

    enum MazeDirection : int
    {
        North = 1,
        East = 2,
        South = 4,
        West = 8
    }

    internal static class MazeUtility
    {
        public static float DirectionToYRotation(MazeDirection direction)
        {
            switch (direction)
            {
                case MazeDirection.East: return -90f;
                case MazeDirection.South: return 180f;
                case MazeDirection.West: return 90f;
                default: return 0f;
            }
        }
        public static int DirectionsToFlags(List<MazeDirection> mazeDirections)
        {
            int flag = 0;
            if (mazeDirections.Contains(MazeDirection.North)) flag += (int)MazeDirection.North;
            if (mazeDirections.Contains(MazeDirection.East)) flag += (int)MazeDirection.East;
            if (mazeDirections.Contains(MazeDirection.South)) flag += (int)MazeDirection.South;
            if (mazeDirections.Contains(MazeDirection.West)) flag += (int)MazeDirection.West;
            return flag;
        }
        public static List<MazeDirection> FlagsToDirections(int flags)
        {
            List<MazeDirection> directions = new List<MazeDirection>();
            if ((flags & (int)MazeDirection.North) != 0) directions.Add(MazeDirection.North);
            if ((flags & (int)MazeDirection.East)  != 0) directions.Add(MazeDirection.East);
            if ((flags & (int)MazeDirection.South) != 0) directions.Add(MazeDirection.South);
            if ((flags & (int)MazeDirection.West)  != 0) directions.Add(MazeDirection.West);
            return directions;
        }
        public static MazeDirection ReverseDirection(MazeDirection direction)
        {
            switch (direction)
            {
                case MazeDirection.North: return MazeDirection.South;
                case MazeDirection.South: return MazeDirection.North;
                case MazeDirection.East: return MazeDirection.West;
                default: return MazeDirection.East;
            }
        }
        public static MazeDirection RotateDirectionLeft(MazeDirection direction)
        {
            return direction == MazeDirection.North ? MazeDirection.West : (MazeDirection)((int)direction/2);
        }
        public static MazeDirection RotateDirectionRight(MazeDirection direction)
        {
            return direction == MazeDirection.West ? MazeDirection.North : (MazeDirection)((int)direction*2);
        }
    }
}