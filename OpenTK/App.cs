using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.CompilerServices;
using ImGuiNET;
using OpenGL;

namespace OpenTK
{
    class App : GameWindow
    {
        public App(int width, int height, string title)
            : base(width, height, GraphicsMode.Default, title)
        { 
        }

        int _vertexBufferObject;
        int _vertexArrayObject;
        int _indexBufferObject; 

        private Mesh _mesh;
        
        private Shader _shader;

        private ImGuiController _controller;

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            _controller.SetWindowSize(Width, Height);

            base.OnResize(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(0.03f, 0.03f, 0.03f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            _shader = new Shader("shaders/shader.v", "shaders/shader.f");

            _controller = new ImGuiController();

            _vertexArrayObject = GL.GenVertexArray();
            _vertexBufferObject = GL.GenBuffer();

            GL.BindVertexArray(_vertexArrayObject);

            _mesh = MeshLoader.LoadMesh("mesh/Chair.obj");

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _mesh.Vertices.Length * Unsafe.SizeOf<Vertex>(), _mesh.Vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(_shader.GetAttributeLocation("aPosition"), 3, VertexAttribPointerType.Float, false, Unsafe.SizeOf<Vertex>(), 0);

            GL.VertexAttribPointer(_shader.GetAttributeLocation("aColor"), 3, VertexAttribPointerType.Float, false, Unsafe.SizeOf<Vertex>(), Unsafe.SizeOf<Vector3>());

            GL.EnableVertexAttribArray(_shader.GetAttributeLocation("aPosition"));
            GL.EnableVertexAttribArray(_shader.GetAttributeLocation("aColor"));

            _indexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _mesh.Indeces.Length * sizeof(int), _mesh.Indeces, BufferUsageHint.StaticDraw);
            
            base.OnLoad(e);
        }

        private float _scale = 1.0f;
        private float _angle = 0.0f;
        private float _angle_z = 0.0f;
        private float _dist = 5.0f;
        private bool _persp = false;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader.Use();

            _controller.NewFrame(this);

            ImGui.SliderFloat("Scale", ref _scale, 0, 10);
            ImGui.SliderFloat("Angle", ref _angle, -3.14f, 3.14f);
            ImGui.SliderFloat("Angle_z", ref _angle_z, -3.14f, 3.14f);
            ImGui.SliderFloat("Distance", ref _dist, 0, 10.0f);
            ImGui.Checkbox("Perspective", ref _persp);

            _shader.SetUniform("scaleFactor", _scale);

            var _model = Matrix4.CreateRotationY(_angle) * Matrix4.CreateRotationX(_angle_z) * Matrix4.CreateTranslation(0, 0, -_dist);

            _shader.SetUniform("model", _model);

            var projection = _persp
                ? Matrix4.CreatePerspectiveFieldOfView((float) (Math.PI / 2), (float) Width / Height, 0.1f, 100.0f)
                : Matrix4.CreateOrthographic(10, 10, -7, 7);
           

            _shader.SetUniform("projection", projection);

            GL.DrawElements(PrimitiveType.Triangles, _mesh.Indeces.Length, DrawElementsType.UnsignedInt, 0);

            _controller.Render();

            Context.SwapBuffers();
            base.OnRenderFrame(e);
        }
    }
}
