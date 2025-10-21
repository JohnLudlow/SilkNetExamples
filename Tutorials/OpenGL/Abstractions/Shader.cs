using System;
using Silk.NET.OpenGL;

namespace SilkNetExamples.Tutorials.OpenGL.Abstractions;

public sealed class Shader : IDisposable
{
  private uint _handle;
  private GL _gl;

  public Shader(GL gl, string vertexPath, string fragmentPath)
  {
    _gl = gl;
    _handle = _gl.CreateProgram();

    var vertex = CompileShaderFromSource(_gl, _handle, vertexPath, ShaderType.VertexShader);
    var fragment = CompileShaderFromSource(_gl, _handle, fragmentPath, ShaderType.FragmentShader);

    _gl.LinkProgram(_handle);
    _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);

    if (status == 0)
    {
      throw new Exception($"Program failed to link with error {_gl.GetProgramInfoLog(_handle)}");
    }

    _gl.DetachShader(_handle, vertex);
    _gl.DetachShader(_handle, fragment);
    _gl.DeleteShader(vertex);
    _gl.DeleteShader(fragment);
  }

  public void Use()
  {
    _gl.UseProgram(_handle);
  }

  public void SetUniform(string name, int value)
  {
    var location = _gl.GetUniformLocation(_handle, name);
    if (location == -1)
    {
      throw new Exception($"{name} uniform not found on shader");
    }

    _gl.Uniform1(location, value);
  }

  public void SetUniform(string name, float value)
  {
    var location = _gl.GetUniformLocation(_handle, name);
    if (location == -1)
    {
      throw new Exception($"{name} uniform not found on shader");
    }

    _gl.Uniform1(location, value);
  }

  private static uint CompileShaderFromSource(GL _gl, uint shaderProgram, string shaderSourcePath, ShaderType shaderType)
  {
    var shaderCode = File.ReadAllText(shaderSourcePath);
    var shader = _gl.CreateShader(shaderType);
    _gl.ShaderSource(shader, shaderCode);
    _gl.CompileShader(shader);
    _gl.GetShader(shader, ShaderParameterName.CompileStatus, out var shaderStatus);
    if (shaderStatus != (int)GLEnum.True)
    {
      var infoLog = _gl.GetShaderInfoLog(shader);
      Console.WriteLine($"Error compiling {shaderType} shader: {infoLog}");
#pragma warning disable CA2201 // Do not raise reserved exception types
      throw new Exception("{shaderType} shader compilation failed. \n\r" + infoLog);
#pragma warning restore CA2201 // Do not raise reserved exception types
    }
    _gl.AttachShader(shaderProgram, shader);

    return shader;
  }

  public void Dispose()
  {
    _gl.DeleteProgram(_handle);
  }
}
