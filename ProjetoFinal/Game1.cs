using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace ProjetoFinal
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Model scenario;
        float scenarioScale;
        float proportionScenarioTank;

        Model tank3D;
        float tankScale;
        float tankAngle;
        Vector3 tankPosition;

        float distance;
        float visionAngle;
        float visionY;

        Texture2D flare;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            /* Camera Inicialization */
            distance = 200;
            visionAngle = 0;
            visionY = 40;
            proportionScenarioTank = 3;

            tankScale = 0.02f;
            scenarioScale = tankScale * proportionScenarioTank;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            tank3D = Content.Load<Model>("EBRBB");
            scenario = Content.Load<Model>("cenario_ceu");

            flare = Content.Load<Texture2D>("flare");
            tankPosition = new Vector3(0, 300 * scenarioScale, 3000 * scenarioScale);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            /* Camera movimentation around center (0,0,0) */
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) visionAngle++;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) visionAngle--;
            if (Keyboard.GetState().IsKeyDown(Keys.Z)) distance = distance > 50 ? --distance : 50;
            if (Keyboard.GetState().IsKeyDown(Keys.X)) distance = distance < 2000 ? ++distance : 2000;
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) visionY++;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) visionY--;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            #region camera
            Matrix projecao = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), GraphicsDevice.Viewport.AspectRatio, 1, 10000);
            Matrix view = Matrix.CreateLookAt(
                new Vector3((float)Math.Cos(MathHelper.ToRadians(visionAngle)) * distance , visionY, (float)Math.Sin(MathHelper.ToRadians(visionAngle)) * distance) + tankPosition, 
                tankPosition, 
                Vector3.Up);
            #endregion

            #region scenario
            /* draw scenario */
            Matrix worldScenario = Matrix.CreateScale(scenarioScale);

            Matrix[] transforms = new Matrix[scenario.Bones.Count];
            scenario.CopyAbsoluteBoneTransformsTo(transforms);

            Vector3[] positionBulbs = new Vector3[4];
            foreach (ModelMesh malha in scenario.Meshes)
            {
                foreach (BasicEffect efeitoShader in malha.Effects)
                {
                    efeitoShader.World = transforms[malha.ParentBone.Index] * worldScenario;
                    efeitoShader.Projection = projecao;
                    efeitoShader.View = view;
                    //efeitoShader.EnableDefaultLighting();
                    if (malha.Name == "Sphere001")
                    {
                        efeitoShader.DirectionalLight0.Enabled = true;
                        efeitoShader.DirectionalLight0.Direction = Vector3.Up;
                        efeitoShader.DirectionalLight0.DiffuseColor = Color.Red.ToVector3();
                        efeitoShader.EmissiveColor = Color.LightBlue.ToVector3();
                    }
                    else
                    {
                        efeitoShader.EnableDefaultLighting();
                    }

                    // Get bulb positions in 3D (Sphere 13-16)
                    if (malha.Name == "Sphere13") positionBulbs[0] = GraphicsDevice.Viewport.Project(new Vector3(0, 0, 0), efeitoShader.Projection, efeitoShader.View, efeitoShader.World);
                    if (malha.Name == "Sphere14") positionBulbs[1] = GraphicsDevice.Viewport.Project(new Vector3(0, 0, 0), efeitoShader.Projection, efeitoShader.View, efeitoShader.World);
                    if (malha.Name == "Sphere015") positionBulbs[2] = GraphicsDevice.Viewport.Project(new Vector3(0, 0, 0), efeitoShader.Projection, efeitoShader.View, efeitoShader.World);
                    if (malha.Name == "Sphere016") positionBulbs[3] = GraphicsDevice.Viewport.Project(new Vector3(0, 0, 0), efeitoShader.Projection, efeitoShader.View, efeitoShader.World);
                }

                malha.Draw();
            }
            
            // Draw the texture in the poles
            spriteBatch.Begin(SpriteBlendMode.Additive, SpriteSortMode.Immediate, SaveStateMode.SaveState);

            foreach (Vector3 positionBulb in positionBulbs)
                if (positionBulb.Z < 1)
                spriteBatch.Draw(flare, new Vector2(positionBulb.X, positionBulb.Y), null, Color.White, 0, new Vector2(flare.Width / 2, flare.Height / 2), (1 - positionBulb.Z) * 1500 * scenarioScale, SpriteEffects.None, 0);

            spriteBatch.End();
            #endregion

            #region tank
            /* draw tank */
            //Matrix world1 = Matrix.CreateRotationY(angulo) * Matrix.CreateTranslation(posicao);
            Matrix worldTank = Matrix.CreateScale(tankScale) * Matrix.CreateTranslation(tankPosition);

            transforms = new Matrix[tank3D.Bones.Count];
            tank3D.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh malha in tank3D.Meshes)
            {
                foreach (BasicEffect efeitoShader in malha.Effects)
                {
                    efeitoShader.World = transforms[malha.ParentBone.Index] * worldTank;
                    efeitoShader.Projection = projecao;
                    efeitoShader.View = view;
                    efeitoShader.EnableDefaultLighting();
                }

                malha.Draw();
            }
            base.Draw(gameTime);
            #endregion
        }
    }
}
