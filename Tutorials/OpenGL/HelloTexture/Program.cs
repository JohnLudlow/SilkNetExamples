

using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using StbImageSharp;
using System.Drawing;

namespace SilkNetExamples.Tutorials.OpenGL.HelloTexture;

internal sealed class Program
{
  private static uint _program;
  private static IWindow? _window = null!;
  private static GL? _gl = null!;
  private static uint _vao;
  private static uint _vbo;
  private static uint _ebo;
  private static uint _texture;

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
      0.5f  ,  0.5f, 0.0f,  1.0f, 1.0f,
      0.5f  , -0.5f, 0.0f,  1.0f, 0.0f,
      -0.5f , -0.5f, 0.0f,  0.0f, 0.0f,
      -0.5f ,  0.5f, 0.0f,  0.0f, 1.0f
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

    SetParameterPosition(_gl);
    SetParameterTextureCoords(_gl);

    _gl.BindVertexArray(0);
    _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

    _texture = _gl.GenTexture();
    _gl.ActiveTexture(TextureUnit.Texture0);
    _gl.BindTexture(TextureTarget.Texture2D, _texture);

    LoadTexture(_gl, "Resources/Images/silk.png");
    
    _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.Repeat);
    _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.Repeat);
    _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
    _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMinFilter.Nearest);
    _gl.GenerateMipmap(TextureTarget.Texture2D);
    _gl.BindTexture(TextureTarget.Texture2D, 0);

    var location = _gl.GetUniformLocation(_program, "uTexture");
    _gl.Uniform1(location, 0);

    _gl.Enable(EnableCap.Blend);
    _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
  }

  private static unsafe void LoadTexture(GL gl, string textureFilePath)
  {
    if (!Path.Exists(textureFilePath))
      throw new FileNotFoundException(message: $"Failed to find texture path {textureFilePath}", fileName: textureFilePath);

    var result = ImageResult.FromMemory(File.ReadAllBytes(textureFilePath), ColorComponents.RedGreenBlueAlpha);
    fixed (byte* ptr = result.Data)
    {
      gl.TexImage2D(
        TextureTarget.Texture2D,
        0,
        InternalFormat.Rgba,
        (uint)result.Width,
        (uint)result.Height,
        0,
        PixelFormat.Rgba,
        PixelType.UnsignedByte,
        ptr
      );
    }
  }

  private static unsafe void SetParameterPosition(GL gl)
  {
    const uint positionLocation = 0;
    gl.EnableVertexAttribArray(positionLocation);
    gl.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);
  }

  private static unsafe void SetParameterTextureCoords(GL gl)
  {
    const uint textureCoordsLocation = 1;
    gl.EnableVertexAttribArray(textureCoordsLocation);
    gl.VertexAttribPointer(textureCoordsLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3*sizeof(float)));
  }

  private static uint CompileShaderFromSource(GL _gl, uint shaderProgram, string shaderSourcePath, ShaderType shaderType)
  {
    if (!Path.Exists(shaderSourcePath))
      throw new FileNotFoundException(message: $"Failed to find shader source path {shaderSourcePath}", fileName: shaderSourcePath);

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

    _gl.ActiveTexture(TextureUnit.Texture0);
    _gl.BindTexture(TextureTarget.Texture2D, _texture);

    DrawElements(_gl);
  }

  private static unsafe void DrawElements(GL gl)
  {
    gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
  }
}