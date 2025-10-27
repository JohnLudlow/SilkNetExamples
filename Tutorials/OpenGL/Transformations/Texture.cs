using Silk.NET.OpenGL;
using StbImageSharp;

namespace SilkNetExamples.Tutorials.OpenGL.Transformations;

internal sealed class Texture : IDisposable
{
  private readonly uint _handle;
  private readonly GL _gl;

  public unsafe Texture(GL gl, string path)
  {
    _gl = gl;
    _handle = _gl.GenTexture();

    Bind(_gl, _handle);

    var result = ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha);

    fixed (byte* ptr = result.Data)
    {
      _gl.TexImage2D(
        target: TextureTarget.Texture2D,
        level: 0,
        internalformat: InternalFormat.Rgba,
        width: (uint)result.Width,
        height: (uint)result.Height,
        border: 0,
        format: PixelFormat.Rgba,
        type: PixelType.UnsignedByte,
        pixels: ptr
      );
    }

    SetParameters(_gl);
  }

  public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
  {
    Bind(_gl, _handle, textureSlot);
  }

  private static void Bind(GL gl, uint handle, TextureUnit textureSlot = TextureUnit.Texture0)
  {
    gl.ActiveTexture(textureSlot);
    gl.BindTexture(TextureTarget.Texture2D, handle);
  }

  private static void SetParameters(GL gl)
  {
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
    gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);

    gl.GenerateMipmap(TextureTarget.Texture2D);
  }

  public void Dispose()
  {
    _gl.DeleteTexture(_handle);
  }
}