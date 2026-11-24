namespace CG
{
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