using System;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using MGDynamicTTF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;



namespace Game.Desktop
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private readonly GraphicsDeviceManager graphics;
        private DynamicSpritefont dynamicSpriteFont;

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            IsFixedTimeStep = false;
            IsMouseVisible = true;
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            graphics.ApplyChanges();
            base.Initialize();
        }


        protected override void LoadContent()
        {          
            spriteBatch = new SpriteBatch(GraphicsDevice);       
            dynamicSpriteFont = new DynamicSpritefont(Content, GraphicsDevice,"simkai.ttf", 2048);
            
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        private SpriteBatch spriteBatch;

        protected override void Draw(GameTime gameTime)
        {
           GraphicsDevice.Clear(Color.DarkSlateGray);
            
            spriteBatch.Begin();
            var sw = Stopwatch.StartNew();

            var str =
                "又出學持，流發文續統痛進錢著根等金學，師人有作進出先能到教我資的好好票亞不人放生者，流智把速拉速方變我劇過高正、制傷地切不學就那才告識衣香不預。\n\n洋整書，要什和開女子！有使展覺下時收易個，如說例並備國然吃的治內早通我地重已能作個了的神得！中斷藝：變認力我想成節字電取條商地外苦現除其時立也愛公向。花發上終關空部話充觀友才放舉子局生她是，麼又兒心供我變。\n\n土金下心。滿界理防費家西顯不為，於留有四頭易道中作、務樣主眼以她出驗方臺全問就表造點件；絕型極設不性量由多實此東利那知聽當反風，他保麼經？白形盡計界唱不人道別子四快不聽不一以之門選多體個他業來費，西母示老代然裡是聯雖一上環開只一灣滿更你臺得區？像問同照！學難人！商此麗社少市金未發原他北處有民使格酒去！由結化師企出不水：今靈陸直女兒就便萬；質河聞系科急人以們：不中整和票的口去怎子因、部量家告數。後來說輕：表寫動界立然。不遊對多作想美綠我些。變別時沒字集況有在表把客質造。";

            var amount = str.Length / 30;

            for (var i = 0; i < amount; i++)
            {
                var length = str.Length - (i * 30);
                if (length > 30)
                    length = 30;
                var s = str.Substring(i * 30, length);
                dynamicSpriteFont.Draw(spriteBatch, new Vector2(30,10+(i*30)), s , Color.White);

            }
                
          // dynamicSpriteFont.Draw(spriteBatch, new Vector2(50, 50), "你好，我叫云义多", Color.White);
         //   dynamicSpriteFont.Draw(spriteBatch, new Vector2(50, 80), "又出學持，流發文續統痛進錢著根等金學，師人有作進出先能到教我資的好好票亞不人放生者，流智把速拉速方變我劇過高正、制傷地切不學就那才告識衣香不預。\n\n洋整書，要什和開女子！有使展覺下時收易個，如說例並備國然吃的治內早通我地重已能作個了的神得！中斷藝：變認力我想成節字電取條商地外苦現除其時立也愛公向。花發上終關空部話充觀友才放舉子局生她是，麼又兒心供我變。\n\n土金下心。滿界理防費家西顯不為，於留有四頭易道中作、務樣主眼以她出驗方臺全問就表造點件；絕型極設不性量由多實此東利那知聽當反風，他保麼經？白形盡計界唱不人道別子四快不聽不一以之門選多體個他業來費，西母示老代然裡是聯雖一上環開只一灣滿更你臺得區？像問同照！學難人！商此麗社少市金未發原他北處有民使格酒去！由結化師企出不水：今靈陸直女兒就便萬；質河聞系科急人以們：不中整和票的口去怎子因、部量家告數。後來說輕：表寫動界立然。不遊對多作想美綠我些。變別時沒字集況有在表把客質造。", Color.White);
            //dynamicSpriteFont.Draw(spriteBatch, new Vector2(10, 100), "  English is an afterthought here.. It should be rendered with a spritefont", Color.White);
            sw.Stop();
            if(sw.ElapsedMilliseconds >= 1)
                Console.WriteLine(sw.ElapsedMilliseconds);
            
           // dynamicSpriteFont.Draw(spriteBatch, new Vector2(200, 150), "Cache Texture", Color.White);

           // spriteBatch.Draw(dynamicSpriteFont.AtlasTexture,new Vector2(200,170),Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            dynamicSpriteFont.Dispose();
            base.UnloadContent();
        }
    }
}