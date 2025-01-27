﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using Toolbox.Core;

namespace GLFrameworkEngine
{
    public class GLTexture2D : GLTexture
    {
        public GLTexture2D() : base()
        {
            Target = TextureTarget.Texture2D;
        }

        public static GLTexture2D CreateUncompressedTexture(int width, int height,
            PixelInternalFormat format = PixelInternalFormat.Rgba8,
            PixelFormat pixelFormat = PixelFormat.Rgba,
            PixelType pixelType = PixelType.UnsignedByte)
        {
            GLTexture2D texture = new GLTexture2D();
            texture.PixelInternalFormat = format;
            texture.PixelFormat = pixelFormat;
            texture.PixelType = pixelType;
            texture.Width = width; texture.Height = height;
            texture.Target = TextureTarget.Texture2D;
            texture.Bind();

            GL.TexImage2D(TextureTarget.Texture2D, 0, format,
                texture.Width, texture.Height,
                0, pixelFormat, pixelType, IntPtr.Zero);

            texture.UpdateParameters();
            texture.Unbind();
            return texture;
        }

        public static GLTexture2D CreateWhiteTexture(int width, int height)
        {
            GLTexture2D texture = new GLTexture2D();
            texture.PixelInternalFormat = PixelInternalFormat.Rgba8;
            texture.PixelFormat = PixelFormat.Rgba;
            texture.PixelType = PixelType.UnsignedByte;
            texture.Width = width; texture.Height = height;
            texture.Target = TextureTarget.Texture2D;
            texture.Bind();

            byte[] buffer = new byte[width * height * 4];
            for (int i = 0; i < width * height * 4; i++)
                buffer[i] = 255;

            GL.TexImage2D(TextureTarget.Texture2D, 0, texture.PixelInternalFormat,
                texture.Width, texture.Height,
                0, texture.PixelFormat, texture.PixelType, buffer);

            texture.UpdateParameters();
            texture.Unbind();
            return texture;
        }

        public static GLTexture2D CreateConstantColorTexture(int width, int height, byte R, byte G, byte B, byte A)
        {
            GLTexture2D texture = new GLTexture2D();
            texture.PixelInternalFormat = PixelInternalFormat.Rgba8;
            texture.PixelFormat = PixelFormat.Rgba;
            texture.PixelType = PixelType.UnsignedByte;
            texture.Width = width; texture.Height = height;
            texture.Target = TextureTarget.Texture2D;
            texture.Bind();

            byte[] buffer = new byte[width * height * 4];
            int offset = 0;
            for (int i = 0; i < width * height; i++)
            {
                buffer[offset] = R;
                buffer[offset + 1] = G;
                buffer[offset + 2] = B;
                buffer[offset + 3] = A;
                offset += 4;
            }

            GL.TexImage2D(TextureTarget.Texture2D, 0, texture.PixelInternalFormat,
                texture.Width, texture.Height,
                0, texture.PixelFormat, texture.PixelType, buffer);

            texture.UpdateParameters();
            texture.Unbind();
            return texture;
        }

        public static GLTexture2D CreateFloat32Texture(int width, int height, float[] buffer)
        {
            GLTexture2D texture = new GLTexture2D();
            texture.PixelInternalFormat = PixelInternalFormat.R32f;
            texture.PixelFormat = PixelFormat.Red;
            texture.PixelType = PixelType.Float;
            texture.Width = width; texture.Height = height;
            texture.Target = TextureTarget.Texture2D;
            texture.Bind();

            GL.TexImage2D(TextureTarget.Texture2D, 0, texture.PixelInternalFormat,
                texture.Width, texture.Height,
                0, texture.PixelFormat, texture.PixelType, buffer);

            texture.UpdateParameters();
            texture.Unbind();
            return texture;
        }

        public static GLTexture2D FromGeneric(STGenericTexture texture, ImageParameters parameters = null)
        {
            if (parameters == null) parameters = new ImageParameters();

            GLTexture2D glTexture = new GLTexture2D();
            glTexture.Target = TextureTarget.Texture2D;
            glTexture.Width = (int)texture.Width;
            glTexture.Height = (int)texture.Height;
            glTexture.LoadImage(texture, parameters);
            return glTexture;
        }

        public static GLTexture2D FromBitmap(string imageFile)
        {
            Bitmap image = (Bitmap)Bitmap.FromStream(System.IO.File.OpenRead(imageFile));
            return FromBitmap(image);
        }


        public static GLTexture2D FromBitmap(byte[] imageFile, int width, int height)
        {
            Bitmap image = (Bitmap)Bitmap.FromStream(new System.IO.MemoryStream(imageFile));
            image = Toolbox.Core.Imaging.BitmapExtension.Resize(image, width, height);
            return FromBitmap(image);
        }

        public static GLTexture2D FromBitmap(byte[] imageFile, bool isLinear = true)
        {
            Bitmap image =  (Bitmap)Bitmap.FromStream(new System.IO.MemoryStream(imageFile));

            GLTexture2D texture = new GLTexture2D();
            texture.Target = TextureTarget.Texture2D;
            texture.Width = image.Width; texture.Height = image.Height;

            texture.Bind();
            if (isLinear)
            {
                texture.MagFilter = TextureMagFilter.Linear;
                texture.MinFilter = TextureMinFilter.Linear;
            }
            else
            {
                texture.MagFilter = TextureMagFilter.Nearest;
                texture.MinFilter = TextureMinFilter.Nearest;
            }

            texture.UpdateParameters();
            texture.LoadImage(image);
            return texture;
        }

        public static GLTexture2D FromBitmap(Bitmap image)
        {
            GLTexture2D texture = new GLTexture2D();
            texture.Target = TextureTarget.Texture2D;
            texture.Width = image.Width; texture.Height = image.Height;
            texture.LoadImage(image);
            return texture;
        }

        public void LoadImage(Bitmap image)
        {
            Bind();

            System.Drawing.Imaging.BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
              System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(Target, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            image.Dispose();

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            Unbind();
        }

        public void LoadImage(byte[] image)
        {
            Bind();

            GL.TexImage2D(Target, 0, PixelInternalFormat.Rgba, Width, Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, image);

            Unbind();
        }

        public void Reload<T>(int width, int height, T[] data) where T : struct
        {
            Bind();

            GL.TexImage2D(Target, 0, this.PixelInternalFormat, width, height, 0,
                this.PixelFormat, this.PixelType, data);

            Unbind();
        }

        public System.IO.Stream ToStream(bool saveAlpha = false)
        {
            var stream = new System.IO.MemoryStream();
            var bmp = ToBitmap(saveAlpha);
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream;
        }

        public byte[] GetBytes(int level = 0)
        {
            int mipWidth = (int)(Width * Math.Pow(0.5, level));
            int mipHeight = (int)(Height * Math.Pow(0.5, level));

            byte[] outputRaw = new byte[mipWidth * mipHeight * 4];
            GL.GetTexImage(this.Target, level,
              PixelFormat.Bgra, PixelType.UnsignedByte, outputRaw);

            return outputRaw;
        }

        public override void SaveDDS(string fileName)
        {
            List<STGenericTexture.Surface> surfaces = new List<STGenericTexture.Surface>();

            Bind();

            var surface = new STGenericTexture.Surface();
            surfaces.Add(surface);

            for (int m = 0; m < this.MipCount; m++)
            {
                int mipWidth = (int)(Width * Math.Pow(0.5, m));
                int mipHeight = (int)(Height * Math.Pow(0.5, m));

                byte[] outputRaw = new byte[mipWidth * mipHeight * 4];
                GL.GetTexImage(this.Target, m,
                  PixelFormat.Bgra, PixelType.UnsignedByte, outputRaw);

                surface.mipmaps.Add(outputRaw);
            }

            var dds = new DDS();
            dds.MainHeader.Width = (uint)this.Width;
            dds.MainHeader.Height = (uint)this.Height;
            dds.MainHeader.Depth = 1;
            dds.MainHeader.MipCount = (uint)this.MipCount;
            dds.MainHeader.PitchOrLinearSize = (uint)surfaces[0].mipmaps[0].Length;

            dds.SetFlags(TexFormat.RGBA8_UNORM, false, false);

            if (dds.IsDX10)
            {
                if (dds.Dx10Header == null)
                    dds.Dx10Header = new DDS.DX10Header();

                dds.Dx10Header.ResourceDim = 3;
                dds.Dx10Header.ArrayCount = 1;
            }

            dds.Save(fileName, surfaces);

            Unbind();
        }

        public void Save(string fileName, bool saveAlpha = false)
        {
            Bind();

            var bmp = ToBitmap(saveAlpha);
            bmp.Save(fileName + ".png");

            Unbind();
        }

        public override System.Drawing.Bitmap ToBitmap(bool saveAlpha = true)
        {
            Bind();

            var bmp = new System.Drawing.Bitmap(Width, Height);

            System.Drawing.Imaging.BitmapData data =
                bmp.LockBits(new System.Drawing.Rectangle(0, 0, Width, Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                saveAlpha ?
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
                :
                 System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            GL.ReadPixels(0, 0, Width, Height, saveAlpha ? PixelFormat.Bgra : PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            bmp.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);

            Unbind();

            return bmp;
        }
    }
}
