using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ImGuiNET;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using Vector2 = System.Numerics.Vector2;

namespace OpenGL
{
    public class ImGuiController
    {
        private int _vertexArray;
        private int _vertexBuffer;
        private int _indexBuffer;

        private Shader _shader;

        private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

        private int _fontTexture;

        struct RenderState
        {
            public int texture;
            public int program;
            public int vao;
            public int arrayBuffer;
            public int[] viewport;
            public int[] scissorBox;
            public bool blend;
            public bool cullFace;
            public bool depthTest;
            public bool scissorTest;
        };

        private RenderState SaveRenderState()
        {
            var res = new RenderState();

            GL.GetInteger(GetPName.CurrentProgram, out res.program);
            GL.GetInteger(GetPName.TextureBinding2D, out res.texture);
            GL.GetInteger(GetPName.ArrayBufferBinding, out res.arrayBuffer);
            GL.GetInteger(GetPName.VertexArrayBinding, out res.vao);
            res.viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, res.viewport);
            res.scissorBox = new int[4];
            GL.GetInteger(GetPName.ScissorBox, res.scissorBox);
            res.blend = GL.IsEnabled(EnableCap.Blend);
            res.cullFace = GL.IsEnabled(EnableCap.CullFace);
            res.depthTest = GL.IsEnabled(EnableCap.DepthTest);
            res.scissorTest = GL.IsEnabled(EnableCap.ScissorTest);

            return res;
        }

        private void RestoreRenderState(RenderState renderState)
        {
            GL.UseProgram(renderState.program);
            GL.BindTexture(TextureTarget.Texture2D, renderState.texture);
            GL.BindVertexArray(renderState.vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, renderState.arrayBuffer);
            if (renderState.blend) { GL.Enable(EnableCap.Blend); } else { GL.Disable(EnableCap.Blend); }
            if (renderState.cullFace) { GL.Enable(EnableCap.CullFace); } else { GL.Disable(EnableCap.CullFace); }
            if (renderState.depthTest) { GL.Enable(EnableCap.DepthTest); } else { GL.Disable(EnableCap.DepthTest); }
            if (renderState.scissorTest) { GL.Enable(EnableCap.ScissorTest); } else { GL.Disable(EnableCap.ScissorTest); }
            GL.Viewport(renderState.viewport[0], renderState.viewport[1], renderState.viewport[2], renderState.viewport[3]);
            GL.Scissor(renderState.scissorBox[0], renderState.scissorBox[1], renderState.scissorBox[2], renderState.scissorBox[3]);
        }

        public ImGuiController()
        {
            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            var io = ImGui.GetIO();
            io.Fonts.AddFontDefault();

            CreateDeviceObjects();
        }

        public void Render()
        {
            ImGui.Render();
            RenderDrawData(ImGui.GetDrawData());
        }

        public void NewFrame(GameWindow wnd)
        {
            UpdateImGuiInput(wnd);
            ImGui.NewFrame();
        }

        public void SetWindowSize(int width, int height)
        {
            ImGui.GetIO().DisplaySize = new Vector2((float)width, (float)height);
        }

        private void CreateDeviceObjects()
        {
            GL.GetInteger(GetPName.VertexArrayBinding, out var prev_vao);

            _vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArray);

            _shader = new Shader("shaders/imgui.v", "shaders/imgui.f");

            _vertexBuffer = GL.GenBuffer();
            _indexBuffer = GL.GenBuffer();

            CreateFontsTexture();

            GL.BindVertexArray(prev_vao);
        }

        private void CreateFontsTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

            _fontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            io.Fonts.TexID = (IntPtr)_fontTexture;
        }

        private void SetupRenderState(ImGuiNET.ImDrawDataPtr drawData, int width, int height, int VAO)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ScissorTest);

            GL.Viewport(0, 0, width, height);

            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
                -1.0f,
                width,
                height,
                0.0f,
                -1.0f,
                1.0f);

            _shader.Use();

            GL.UniformMatrix4(_shader.GetUniformLocation("uProjection"), false, ref mvp);

            GL.BindVertexArray(_vertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);

            GL.EnableVertexAttribArray(_shader.GetAttributeLocation("aPosition"));
            GL.EnableVertexAttribArray(_shader.GetAttributeLocation("aUV"));
            GL.EnableVertexAttribArray(_shader.GetAttributeLocation("aColor"));

            GL.VertexAttribPointer(_shader.GetAttributeLocation("aPosition"), 2, VertexAttribPointerType.Float, false, Unsafe.SizeOf<ImDrawVert>(), 0);
            GL.VertexAttribPointer(_shader.GetAttributeLocation("aUV"), 2, VertexAttribPointerType.Float, false, Unsafe.SizeOf<ImDrawVert>(), 8);
            GL.VertexAttribPointer(_shader.GetAttributeLocation("aColor"), 4, VertexAttribPointerType.UnsignedByte, true, Unsafe.SizeOf<ImDrawVert>(), 16);
        }

        private void RenderDrawData(ImDrawDataPtr drawData)
        {
            var width = (int)(drawData.DisplaySize.X * drawData.FramebufferScale.X);
            var height = (int)(drawData.DisplaySize.Y * drawData.FramebufferScale.Y);

            var renderState = SaveRenderState();

            SetupRenderState(drawData, width, height, _vertexArray);

            var clipOff = drawData.DisplayPos;
            var clipScale = drawData.FramebufferScale;

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                var cmdList = drawData.CmdListsRange[n];

                GL.BufferData(BufferTarget.ArrayBuffer, cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmdList.VtxBuffer.Data, BufferUsageHint.StreamDraw);
                GL.BufferData(BufferTarget.ElementArrayBuffer, cmdList.IdxBuffer.Size * sizeof(short), cmdList.IdxBuffer.Data, BufferUsageHint.StreamDraw);

                for (int cmd_i = 0; cmd_i < cmdList.CmdBuffer.Size; cmd_i++)
                {
                    var pcmd = cmdList.CmdBuffer[cmd_i];

                    Vector4 clipRect = new Vector4
                    (
                        (pcmd.ClipRect.X - clipOff.X) * clipScale.X,
                        (pcmd.ClipRect.Y - clipOff.Y) * clipScale.Y,
                        (pcmd.ClipRect.Z - clipOff.X) * clipScale.X,
                        (pcmd.ClipRect.W - clipOff.Y) * clipScale.Y
                    );

                    if (clipRect.X < width && clipRect.Y < height && clipRect.Z >= 0.0f && clipRect.W >= 0.0f)
                    {
                        GL.Scissor((int)(clipRect.X), (int)(height - clipRect.W), (int)(clipRect.Z - clipRect.X), (int)(clipRect.W - clipRect.Y));

                        GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);

                        GL.DrawElements(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(short)));
                    }
                }
            }

            RestoreRenderState(renderState);
        }

        MouseState PrevMouseState;
        KeyboardState PrevKeyboardState;
        readonly List<char> PressedChars = new List<char>();

        private void UpdateImGuiInput(GameWindow wnd)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            MouseState MouseState = Mouse.GetCursorState();
            KeyboardState KeyboardState = Keyboard.GetState();

            io.MouseDown[0] = MouseState.LeftButton == ButtonState.Pressed;
            io.MouseDown[1] = MouseState.RightButton == ButtonState.Pressed;
            io.MouseDown[2] = MouseState.MiddleButton == ButtonState.Pressed;

            var screenPoint = new System.Drawing.Point(MouseState.X, MouseState.Y);
            var point = wnd.PointToClient(screenPoint);
            io.MousePos = new System.Numerics.Vector2(point.X, point.Y);

            io.MouseWheel = MouseState.Scroll.Y - PrevMouseState.Scroll.Y;
            io.MouseWheelH = MouseState.Scroll.X - PrevMouseState.Scroll.X;

            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                io.KeysDown[(int)key] = KeyboardState.IsKeyDown(key);
            }

            foreach (var c in PressedChars)
            {
                io.AddInputCharacter(c);
            }
            PressedChars.Clear();

            io.KeyCtrl = KeyboardState.IsKeyDown(Key.ControlLeft) || KeyboardState.IsKeyDown(Key.ControlRight);
            io.KeyAlt = KeyboardState.IsKeyDown(Key.AltLeft) || KeyboardState.IsKeyDown(Key.AltRight);
            io.KeyShift = KeyboardState.IsKeyDown(Key.ShiftLeft) || KeyboardState.IsKeyDown(Key.ShiftRight);
            io.KeySuper = KeyboardState.IsKeyDown(Key.WinLeft) || KeyboardState.IsKeyDown(Key.WinRight);

            PrevMouseState = MouseState;
            PrevKeyboardState = KeyboardState;
        }


        internal void PressChar(char keyChar)
        {
            PressedChars.Add(keyChar);
        }

        private static void SetKeyMappings()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.KeyMap[(int)ImGuiKey.Tab] = (int)Key.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Key.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Key.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Key.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Key.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Key.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Key.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Key.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Key.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Key.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Key.BackSpace;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Key.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Key.Escape;
            io.KeyMap[(int)ImGuiKey.A] = (int)Key.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Key.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Key.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Key.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Key.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Key.Z;
        }
    }
}