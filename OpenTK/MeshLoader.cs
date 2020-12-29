using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OpenTK
{
    public struct Mesh
    {
        public Vertex[] Vertices;
        public int[] Indeces;

        public Mesh(List<Vertex> vertices, List<int> indeces)
        {
            Vertices = vertices.ToArray();
            Indeces = indeces.ToArray();
        }
    }
    public class MeshLoader
    {
        public static Mesh LoadMesh(string path)
        {
            string file;
            using (var stream = new StreamReader(path))
            {
                file = stream.ReadToEnd();
            }

            var lines = file.Split('\n');

            var vertices = new List<Vertex>();
            var indeces = new List<int>();

            var vRegex = new Regex(@"v ([-.\d]+) ([-.\d]+) ([-.\d]+)");
            var iRegex = new Regex(@"f (\d+)/(\d+)/(\d+) (\d+)/(\d+)/(\d+) (\d+)/(\d+)/(\d+)");

            var rand = new Random();

            foreach (var line in lines)
            {
                if (vRegex.IsMatch(line))
                {
                    var match = vRegex.Match(line);

                    var vertex = new Vertex(
                        new Vector3(
                            float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture), 
                            float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture),
                            float.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture)),
                        new Vector3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()));

                    vertices.Add(vertex);
                }

                if (iRegex.IsMatch(line))
                {
                    var match = iRegex.Match(line);

                    indeces.Add(int.Parse(match.Groups[1].Value) - 1);
                    indeces.Add(int.Parse(match.Groups[4].Value) - 1);
                    indeces.Add(int.Parse(match.Groups[7].Value) - 1);
                }
            }
            return new Mesh(vertices, indeces);
        }
    }
}
