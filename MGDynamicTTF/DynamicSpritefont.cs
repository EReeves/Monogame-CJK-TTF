using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TrueTypeSharp;

namespace MGDynamicTTF
{
    public class DynamicSpritefont : IDisposable
    {
        //Pixel data is held in this array, save writing to and from the GPU.
        private readonly Color[][] atlasColorData;

        //Frequency queue to work out which character to remove when texture is full,
        private readonly Queue<char> charFrequencyQueue = new Queue<char>();

        //character, with source position  and size
        private readonly Dictionary<char, Glyph> charSourceMap = new Dictionary<char, Glyph>();
        private readonly ContentManager content;
        private readonly int textureSize;

        //Font metric
        private int ascent;
        private float scale; //Generated scale for use with TrueTypeSharp


        //Positioning.
        private Point atlasPosition = new Point(0, 0);
        private int atlasYMax;
        
        //The font
        private TrueTypeFont font;

        public DynamicSpritefont(ContentManager content, GraphicsDevice graphics, string fontPath,
            int textureSize = 128, int fontSize = 18)
        {
            Size = fontSize;
            this.content = content;
            this.textureSize = textureSize;
            var fullPath = $"{content.RootDirectory}/{fontPath}";

            //Set up atlas texture/dara.
            AtlasTexture = new Texture2D(graphics, textureSize, textureSize);
            atlasColorData = new Color[textureSize][];
            for (var j = 0; j < textureSize; j++)
            {
                atlasColorData[j] = new Color[textureSize];
                for (var k = 0; k < textureSize; k++) atlasColorData[j][k] = Color.CornflowerBlue;
            }

            font = Load(fullPath);
            SetScaleAndMetrics();
        }

        /// <summary>
        ///     The font size.
        /// </summary>
        public int Size
        {
            get => size;
            set
            {
                size = value;
                if (font != null)
                    font.GetScaleForMappingEmToPixels(value);
            }
        }
        private int size;

        /// <summary>
        ///     Spacing between each character.
        /// </summary>
        public int CharacterSpacing { get; set; } = 1;
        
        /// <summary>
        /// Width for spaces there may be a better way to do this.
        /// </summary>
        public int SpaceWidth { get; set; } = 5;
        
        //Chinese full width comma etc
        public int FullWidthPunctuationSpaceWidth { get; set; } = 10;
        
        private string fullWidthPunctuationList = "！？；：( ) ，。、》《“”「」";

        //The main texture used for cachuing glyphs.
        public Texture2D AtlasTexture { get; }
        
        

        public void Dispose()
        {
            AtlasTexture?.Dispose();
        }

        /// <summary>
        ///     Sets scale and metrics used for positioning, sizes and rendering.
        /// </summary>
        private void SetScaleAndMetrics()
        {
            var ss = Size * 96.0f / 72.0f;
            scale = font.GetScaleForMappingEmToPixels(ss);

            font.GetFontVMetrics(out ascent, out _, out _);
            ascent = (int) (ascent * scale);
            //descent = (int) (descent * scale);
        }

        private TrueTypeFont Load(string path)
        {
            //load the font.
            using (var fs = new FileStream(path, FileMode.Open))
            {
                font = new TrueTypeFont(fs);
            }

            return font;
        }

        /// <summary>
        ///     Draws a string using the spritefont.
        /// </summary>
        /// <param name="text">The string to draw.</param>
        public void Draw(SpriteBatch batcher, Vector2 position, string text, Color color)
        {
            Draw(batcher, position, text, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
        }

        /// <summary>
        ///     Draws a string using the spritefont.
        /// </summary>
        /// <param name="text">The string to draw.</param>
        public void Draw(SpriteBatch batcher, Vector2 position, string text, Color color, int rotation, Vector2 origin,
            Vector2 scale, SpriteEffects effect, float depth)
        {
            var glyphs = GetTexturesFromString(batcher, text);
            var x = 0;
            for (var i = 0; i < glyphs.Length; i++)
            {
                var glyph = glyphs[i];
                var pos = position; //Base position

                //Work out the ascent (the position on the Y, every character is different.)
                pos.Y += glyph.Ascent;

                //Add the x offset for the position of the character, as well as spacing between characters.
                pos.X += x;

                //Draw
                batcher.Draw(AtlasTexture, pos, glyph.Rectangle, color, rotation, Vector2.Zero, Vector2.One, effect,
                    depth);

                //All done, add the width to x ready for the next character.
                x += glyph.Rectangle.Width;
                
                //spacing
                x += CharacterSpacing;
                
                //Spacing if it's a space or full width comma.
                if(glyph.Character == 32)
                    x += SpaceWidth;

                if (glyph.FullWidthSpaceRequired)
                    x += FullWidthPunctuationSpaceWidth;

                //Ddd kerning for the next character if it exits.
                if (i >= glyphs.Length - 1) continue;
                var kern = font.GetCodepointKernAdvance(glyph.Character, glyphs[i + 1].Character);
                x += (int) (kern * scale.X);
            }
        }

        private Glyph[] GetTexturesFromString(SpriteBatch batcher, string text)
        {
            var glyphs = new Glyph[text.Length];
            var changed = false;

            for (var characterIndex = 0; characterIndex < text.Length; characterIndex++)
            {
                var ch = text[characterIndex];

                if (charSourceMap.TryGetValue(ch, out var value))
                {
                    glyphs[characterIndex] = value;
                    continue;
                }

                //Change has happened, so we know to update the texture later.
                changed = true;

                //First get the glyph index.
                var index = font.FindGlyphIndex(ch);

                //If not found, replace with a placeholder image. TODO: backup charsets.
                if (index <= 1) index = font.FindGlyphIndex('□');
                if (index <= 1) index = font.FindGlyphIndex('_');

                //Set up the glyph  data for use.
                var data = font.GetGlyphBitmap(index, scale, scale, out var width, out var height, out var w2, out var h2);
                var colorData = new Color[width * height];
                for (var i = 0; i < colorData.Length; i++)
                    colorData[i] = new Color(data[i], data[i], data[i], data[i]);

                //Add to the queue for sorting later
                charFrequencyQueue.Enqueue(ch);

                //find a free position to draw
                if (atlasPosition.X + width > textureSize)
                {
                    atlasPosition.Y = atlasYMax;
                    atlasYMax = atlasYMax + height;
                    atlasPosition.X = 0;
                }

                //When we hit the bottom
                if (atlasPosition.Y + height > textureSize)
                {
                    var rect = Rectangle.Empty;
                    char? c = null;
                    var firstChar = charFrequencyQueue.Peek();

                    //we need to find another character to replace, there's no room left.
                    while (rect.Width < width || rect.Height < height)
                    {
                        if (c != null && c == firstChar)
                        {
                            //Oh no, there's no spots at all. Just clear it. TODO: maybe find a better algorithm.
                            rect.Location = Point.Zero;
                            batcher.GraphicsDevice.Clear(Color.Transparent);
                            break;
                        }

                        if (c != null) charFrequencyQueue.Enqueue(c.Value);
                        c = charFrequencyQueue.Dequeue();
                        rect = charSourceMap[c.Value].Rectangle;
                        charSourceMap.Remove(c.Value);
                    }

                    //Got a spot
                    atlasPosition = rect.Location;
                }

                //Make sure we have the Y max.
                if (atlasYMax < atlasPosition.Y + height)
                    atlasYMax = atlasPosition.Y + height;
                
                //Get ready to calculate the ascent
                font.GetCodepointBitmapBox(ch, this.scale, this.scale, out _, out var p1y, out _,
                    out _);
                    
                //full width
                bool fullWidth = false;
                foreach (var character in fullWidthPunctuationList)
                {
                    if (ch != character) continue;
                    fullWidth = true;
                    break;
                }
              
                //Add the glyph
                var finalRect = new Rectangle(atlasPosition, new Point(width, height));
                var glyph = new Glyph
                {
                    Rectangle = finalRect,
                    Character = ch,
                    Index = index,
                    Ascent = ascent + p1y,
                    FullWidthSpaceRequired = true
                };
                charSourceMap.Add(ch, glyph);
                glyphs[characterIndex] = glyph;

                //Copy the glyph's data across for writing later.
                var cnt = 0;
                for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    atlasColorData[finalRect.X + x][finalRect.Y + y] = colorData[cnt];
                    cnt++;
                }

                //Now we need to increment on the x.
                atlasPosition.X += width;
            }

            if (changed) //New glyphs were found.
            {
                //Lastly, apply the changes to the texture.
                var tmp = new Color[textureSize * textureSize];
                var count = 0;
                for (var j = 0; j < textureSize; j++)
                for (var k = 0; k < textureSize; k++)
                {
                    tmp[count] = atlasColorData[k][j];
                    count++;
                }

                //now we need to set the data on the actual texture
                AtlasTexture.SetData(tmp);
            }

            return glyphs;
        }

        private struct Glyph
        {
            public char Character;
            public Rectangle Rectangle;
            public int Index { get; set; }
            public float Bearing { get; set; }
            public int Ascent { get; set; }
            public bool FullWidthSpaceRequired { get; set; }
        }
    }
}