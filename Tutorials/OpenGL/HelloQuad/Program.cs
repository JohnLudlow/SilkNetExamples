

using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Drawing;

namespace SilkNetExamples.Tutorials.OpenGL.HelloQuad;

internal sealed class Program
{
  private static uint _program;
  private static IWindow? _window = null!;
  private static GL? _gl = null!;
  private static uint _vao;
  private static uint _vbo;
  private static uint _ebo;

  private static void Main(string[] args)
  {
    var windowOptions = WindowOptions.Default with
    {
      Size = new(1600, 1200),
      Title = "Hello, World!",      
    };

    _window = Window.Create(windowOptions);
    _window.Load += OnLoad;
    _window.Update += OnUpdate;
    _window.Render += OnRender;    
    _window.Run();

    Console.WriteLine("Hello, Quad!");
  }

  private static void OnLoad()
  {
    if (_window is null)
    {
      Console.WriteLine("Window is null on load.");
      return;
    }

    // Called once on window creation
    Console.WriteLine("Window loaded.");

    var input = _window.CreateInput();
    foreach (var keyboard in input.Keyboards)
    {
      keyboard.KeyDown += KeyDown;
    }

    _gl = _window.CreateOpenGL();
    _gl.ClearColor(Color.CornflowerBlue);

    _vao = _gl.GenVertexArray();
    _gl.BindVertexArray(_vao);

    BindVertexBuffer(_gl, ref _vbo,
    [
      0.5f,  0.5f, 0.0f,
      0.5f, -0.5f, 0.0f,
      -0.5f, -0.5f, 0.0f,
      -0.5f,  0.5f, 0.0f
    ]);

    BindIndexBuffer(_gl, ref _ebo,
    [
      0, 1, 3,
      1, 2, 3
    ]);

    _program = _gl.CreateProgram();

    var vertexShader = CompileShaderFromSource(_gl, _program, "Resources/Shaders/vertexCode.glsl", ShaderType.VertexShader);
    var fragmentShader = CompileShaderFromSource(_gl, _program, "Resources/Shaders/fragmentCode.glsl", ShaderType.FragmentShader);

    _gl.LinkProgram(_program);
    _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out var linkStatus);

    if (linkStatus != (int)GLEnum.True)
    {
      var infoLog = _gl.GetProgramInfoLog(_program);
      Console.WriteLine($"Error linking shader program: {infoLog}");
#pragma warning disable CA2201 // Do not raise reserved exception types
      throw new Exception("Shader program link failed. \n\r" + infoLog);
#pragma warning restore CA2201 // Do not raise reserved exception types
    }

    _gl.DetachShader(_program, vertexShader);
    _gl.DetachShader(_program, fragmentShader);
    _gl.DeleteShader(vertexShader);
    _gl.DeleteShader(fragmentShader);

    SetParameter(_gl);

    _gl.BindVertexArray(0);
    _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
  }

  private unsafe static void SetParameter(GL gl)
  {
    const uint positionLocation = 0;
    gl.EnableVertexAttribArray(positionLocation);
    gl.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
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
    _gl.AttachShader(_program, shader);

    return shader;
  }

  private static unsafe void BindVertexBuffer(GL gl, ref uint vbo, float[] vertices)
  {
    vbo = gl.GenBuffer();
    gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

    fixed (float* vb = &vertices[0])
    {
      gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), vb, BufferUsageARB.StaticDraw);
    }
  }

  private static unsafe void BindIndexBuffer(GL gl, ref uint ebo, uint[] indices)
  {
    ebo = gl.GenBuffer();
    gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

    fixed (uint* ib = &indices[0])
    {
      gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), ib, BufferUsageARB.StaticDraw);
    }
  }


  private unsafe static void KeyDown(IKeyboard keyboard, Key key, int arg3)
  {
    Console.WriteLine($"Key pressed: {key}");

    if (key == Key.Escape)
    {
      _window?.Close();
    }
  }

  private static void OnUpdate(double deltaTime)
  {
    if (_window is null)
    {
      Console.WriteLine("Window is null on update.");
      return;
    }

    // Called once per frame
    // Console.WriteLine($"Frame update: {deltaTime} seconds since last update.");
  }

  private static void OnRender(double deltaTime)
  {
    if (_window is null)
    {
      Console.WriteLine("Window is null on render.");
      return;
    }

    if (_gl is null)
    {
      Console.WriteLine("OpenGL context is null on render.");
      return;
    }

    // Called once per frame for rendering
    // Console.WriteLine($"Frame render: {deltaTime} seconds since last render.");

    _gl.Clear(ClearBufferMask.ColorBufferBit);
    _gl.BindVertexArray(_vao);
    _gl.UseProgram(_program);
    DrawElements(_gl);
  }

  private static unsafe void DrawElements(GL gl)
  {
    gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
  }
}