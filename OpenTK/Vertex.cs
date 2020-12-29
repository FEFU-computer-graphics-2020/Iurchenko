//using system;
//using system.collections.generic;
//using system.linq;
//using system.text;
//using system.threading.tasks;

namespace OpenTK
{
    public struct Vertex
    {
        public Vertex(Vector3 position, Vector3 color)
        {
            this.Position = position;
            this.Color = color;
        }

        public Vector3 Position;
        public Vector3 Color;
    }
}
