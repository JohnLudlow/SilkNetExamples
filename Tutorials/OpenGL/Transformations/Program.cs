using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SilkNetExamples.Tutorials.OpenGL.Transformations;

internal sealed class Program
{
  private static IWindow _window;
  private static GL _gl;

  private static BufferObject<float> _vbo;
  private static BufferObject<uint> _ebo;
  private static VertexArrayObject<float, uint> _vao;  
  private static readonly float[] _vertices = [
                //X    Y      Z     S    T
             0.5f,  0.5f, 0.0f, 1.0f, 0.0f,
             0.5f, -0.5f, 0.0f, 1.0f, 1.0f,
            -0.5f, -0.5f, 0.0f, 0.0f, 1.0f,
            -0.5f,  0.5f, 0.5f, 0.0f, 0.0f
  ];

  private static readonly uint[] _indices = [
    0, 1, 3,
    1, 2, 3
  ];

  public static SilkNetExamples.Tutorials.OpenGL.Transformations.Texture Texture { get; private set; }
  private static SilkNetExamples.Tutorials.OpenGL.Transformations.Shader _shader;

  private static void Main(string[] args)
  {
    var options = WindowOptions.Default with
    {
      Size = new Vector2D<int>(1600, 1200),
      Title = "LearnOpenGL with Silk.NET"
    };

    _window = Window.Create(options);
    _window.Load += OnLoad;
    _window.Render += OnRender;
    _window.FramebufferResize += OnFramebufferResize;
    _window.Closing += OnClosing;

    _window.Run();

    _window.Dispose();
  }

  private static void OnLoad()
  {
    using var input = _window.CreateInput();

    foreach (var kbd in input.Keyboards)
    {
      kbd.KeyDown += KeyDown;
    }

    _gl = _window.CreateOpenGL();
    _ebo = new BufferObject<uint>(_gl, _indices, BufferTargetARB.ElementArrayBuffer);
    _vbo = new BufferObject<float>(_gl, _vertices, BufferTargetARB.ArrayBuffer);
    _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);

    //Telling the VAO object how to lay out the attribute pointers
    _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
    _vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);

    _shader = new SilkNetExamples.Tutorials.OpenGL.Transformations.Shader(
      _gl,
      "Resources/Shaders/vertexCode.glsl",
      "Resources/Shaders/fragmentCode.glsl"
    );

    Texture = new SilkNetExamples.Tutorials.OpenGL.Transformations.Texture(_gl, "Resources/Images/silk.png");
  }



  private static unsafe void OnRender(double obj)
  {
    _gl.Clear((uint)ClearBufferMask.ColorBufferBit);
    _vao.Bind();

    _shader.Use();
    _shader.SetUniform("uTexture", 0);

    Texture.Bind(TextureUnit.Texture0);

    _gl.DrawElements(PrimitiveType.Triangles, (uint)_indices.Length, DrawElementsType.UnsignedInt, null);
  }
  private static void OnFramebufferResize(Vector2D<int> newSize)
  {
    _gl.Viewport(newSize);
  }
  private static void OnClosing()
  {
    _vbo.Dispose();
    _ebo.Dispose();
    _vao.Dispose();
    _shader.Dispose();
    Texture.Dispose();
  }
  private static void KeyDown(IKeyboard keyboard, Key key, int arg3)
  {
    if (key == Key.Escape)
    {
      _window.Close();
    }
  }
}