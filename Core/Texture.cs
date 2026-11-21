using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace CG.Core
{
    internal class TextureSettings
    {
        public TextureMinFilter minFilter = TextureMinFilter.LinearMipmapLinear;
        public TextureMagFilter magFilter = TextureMagFilter.Linear;
        public TextureWrapMode horizontalWrap = TextureWrapMode.Repeat;
        public TextureWrapMode verticalWrap = TextureWrapMode.Repeat;

        public void Apply()
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)horizontalWrap);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)verticalWrap);
        }
    }

    internal class Texture
    {
        private static Texture? _defaultTexture = null;
        public static Texture Default
        {
            get
            {
                if (_defaultTexture == null)
                {
                    _defaultTexture = new Texture(1, 1);
                }
                return _defaultTexture;
            }
        }

        private int _id;
        public int Id => _id;
        private TextureSettings _settings = new();
        public TextureSettings Settings => _settings;

        public Texture(string path)
        {
            int previous = GL.GetInteger(GetPName.TextureBinding2D);

            ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha);

            _id = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _id);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, result.Width, result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, result.Data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            // Bind da textura anterior para evitar uso incorreto.
            GL.BindTexture(TextureTarget.Texture2D, previous);
        }

        public Texture(int width, int height) : this(width, height, false) { }

        protected Texture(int width, int height, bool depth)
        {
            int previous = GL.GetInteger(GetPName.TextureBinding2D);

            _id = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _id);

            PixelInternalFormat internalFormat = depth ? PixelInternalFormat.DepthComponent32 : PixelInternalFormat.Rgba;
            PixelFormat format = depth ? PixelFormat.DepthComponent : PixelFormat.Rgba;
            GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, width, height, 0, format, PixelType.UnsignedByte, IntPtr.Zero);
            uint[] data = [ ~0u ];
            GL.ClearTexImage(_id, 0, format, PixelType.UnsignedByte, data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            // Bind da textura anterior para evitar uso incorreto.
            GL.BindTexture(TextureTarget.Texture2D, previous);
        }

        public void Use(int unit)
        {
            GL.ActiveTexture((TextureUnit)(TextureUnit.Texture0 + unit));
            GL.BindTexture(TextureTarget.Texture2D, _id);
            _settings.Apply();
        }
    }

    internal class DepthTexture : Texture
    {
        public DepthTexture(int width, int height) : base(width, height, true) { }

        public DepthTexture(int width, int height, TextureSettings settings) : base(width, height, true) { }

    }
}
