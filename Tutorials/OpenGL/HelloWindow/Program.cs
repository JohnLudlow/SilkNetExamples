

using Silk.NET.Input;
using Silk.NET.Windowing;

namespace SilkNetExamples.Tutorials.OpenGL.HelloWindow;

internal sealed class Program
{
  private static IWindow? _window = null!;

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

    Console.WriteLine("Hello, World!");
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
  }

  private static void KeyDown(IKeyboard keyboard, Key key, int arg3)
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
    Console.WriteLine($"Frame update: {deltaTime} seconds since last update.");
  }

  private static void OnRender(double deltaTime)
  {
    if (_window is null)
    {
      Console.WriteLine("Window is null on render.");
      return;
    }

    // Called once per frame for rendering
    Console.WriteLine($"Frame render: {deltaTime} seconds since last render.");
  }
}