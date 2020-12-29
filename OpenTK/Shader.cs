using System.IO;
using System.Text;
using System;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace OpenTK
{
    public class Shader
    {
        int _handle;

        private Dictionary<string, int> _uniformLocations = new Dictionary<string, int>();
        private Dictionary<string, int> _attributeLocations = new Dictionary<string, int>();
        int CompileShader(string path, ShaderType shaderType)
        {
            string _shaderSource;
            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                _shaderSource = reader.ReadToEnd();
            }
            var _shader = GL.CreateShader(shaderType);
            GL.ShaderSource(_shader, _shaderSource);

            GL.CompileShader(_shader);

            var _infolog = GL.GetShaderInfoLog(_shader);
            if (_infolog.Length != 0)
            {
                Console.WriteLine($"Failed at shader {path}");
                Console.WriteLine(_infolog);
                return 0;
            }
            
            return _shader;
        }

        public Shader(string vertexPath, string fragmentPath)
        {
            var _vertexShader = CompileShader(vertexPath, ShaderType.VertexShader);
            var _fragmentShader = CompileShader(fragmentPath, ShaderType.FragmentShader);

            _handle = GL.CreateProgram();

            GL.AttachShader(_handle, _vertexShader);
            GL.AttachShader(_handle, _fragmentShader);

            GL.LinkProgram(_handle); // линковка

            GL.DetachShader(_handle, _vertexShader);
            GL.DetachShader(_handle, _fragmentShader);

            GL.DeleteShader(_vertexShader); // удаляем для сбережения ресурсов
            GL.DeleteShader(_fragmentShader);

        }

        public int GetUniformLocation(string name)
        {
            if (!_uniformLocations.ContainsKey(name))
            {
                _uniformLocations[name] = GL.GetUniformLocation(_handle, name);
            }
            return _uniformLocations[name];
        }

        public int GetAttributeLocation(string name)
        {
            if (!_attributeLocations.ContainsKey(name))
            {
                _attributeLocations[name] = GL.GetAttribLocation(_handle, name);
            }
            return _attributeLocations[name];
        }


        public void SetUniform(string name, float val)
        {
            GL.Uniform1(GetUniformLocation(name), val);
        }

        public void SetUniform(string name, Matrix4 val)
        {
            GL.UniformMatrix4(GetUniformLocation(name), false, ref val);
        }

        public void Use()
        {
            GL.UseProgram(_handle);
        }
    }
}
