using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;

namespace Final_Project_0
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;  
        private SpriteBatch spriteBatch;

        private Texture2D targetTexture;
        private Texture2D menuPage;
        private Vector2 targetPosition;
        private Vector2 mousePosition;

        private Random random;
        SoundEffect hitSound;
        Song lobbyMusic;

        private enum GameState
        {
            MainMenu,
            Countdown,
            Task,
            ScoreMenu
        }

        private GameState currentGameState = GameState.MainMenu;

        private int score = 0;
        private int previousScore = 0;
        private int previousPreviousScore = 0;
        private int hits = 0;
        private int totalShots = 0;

        private TimeSpan countdownTimer;
        private TimeSpan taskTimer;
        private TimeSpan scoreMenuTimer;
        private const int TaskDurationSeconds = 30;

        private SpriteFont font;
        private SpriteFont font2;
        private SpriteFont font3;
        private SpriteFont font4;

        private Camera camera;
        private bool targetHit = false;
        private bool leftButtonReleased = true;
        private bool leftButtonPreviousReleased = true;
        private bool taskRestart = true;

        private Model targetModel;
        private Model targetModel2;
        private Model targetModel3;

        private Texture2D crosshairTexture;
        private Vector2 crosshairPosition;

        private Matrix targetWorldMatrix;
        private Matrix target1WorldMatrix;
        private Matrix target2WorldMatrix;
        private Matrix target3WorldMatrix;

        private BoundingSphere targetBoundingSphere;
        private BoundingSphere target1BoundingSphere;
        private BoundingSphere target2BoundingSphere;
        private BoundingSphere target3BoundingSphere;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            random = new Random();
            camera = new Camera(this, new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
            Components.Add(camera);
            IsMouseVisible = false;
            previousScore = 0;

            lobbyMusic = Content.Load<Song>("CSGO Theme 2");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(lobbyMusic);

            crosshairPosition = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            hitSound = Content.Load<SoundEffect>("Osu hit sound aimlab");
            //lobbyMusic = Content.Load<Song>("CSGO Theme 2");

            //targetTexture = Content.Load<Texture2D>("Target");
            font = Content.Load<SpriteFont>("Font");
            font2 = Content.Load<SpriteFont>("Font2");
            font3 = Content.Load<SpriteFont>("Font3");
            font4 = Content.Load<SpriteFont>("Font4");

            crosshairTexture = Content.Load<Texture2D>("redcrosshair");
            menuPage = Content.Load<Texture2D>("title");

            targetModel = Content.Load<Model>("big plain sphere");
            targetModel2 = Content.Load<Model>("plain sphere");
            targetModel3 = Content.Load<Model>("plain sphere");

            targetWorldMatrix = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(Vector3.Zero);
            target1WorldMatrix = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(new Vector3(-1f, 0f, 0f));
            target2WorldMatrix = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(new Vector3(1f, 0f, 0f));
            target3WorldMatrix = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(new Vector3(0f, 1f, 0f));

            targetBoundingSphere = CalculateModelBoundingSphere(targetModel, targetWorldMatrix);
            target1BoundingSphere = CalculateModelBoundingSphere(targetModel2, target1WorldMatrix);
            target2BoundingSphere = CalculateModelBoundingSphere(targetModel3, target2WorldMatrix);
            target3BoundingSphere = CalculateModelBoundingSphere(targetModel, target3WorldMatrix);
        }

        protected override void Update(GameTime gameTime)
        {
            
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            

            switch (currentGameState)
            {
                case GameState.MainMenu:
                    
                    if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                    {
                      
                        currentGameState = GameState.Countdown;
                        countdownTimer = TimeSpan.FromSeconds(3);
                    }
                    break;
                case GameState.Countdown:
                    countdownTimer -= gameTime.ElapsedGameTime;
                    if (countdownTimer.TotalSeconds <= 0)
                    {
                        currentGameState = GameState.Task;
                        taskTimer = TimeSpan.FromSeconds(TaskDurationSeconds);
                        
                    }
                    break;
                case GameState.Task:
                    MediaPlayer.Stop();

                    crosshairPosition = new Vector2(Mouse.GetState().X - crosshairTexture.Width / 2, Mouse.GetState().Y - crosshairTexture.Height / 2);
                    taskTimer -= gameTime.ElapsedGameTime;
                    if (taskTimer.TotalSeconds <= 0)
                    {
                        taskRestart = true;
                        currentGameState = GameState.ScoreMenu;
                        scoreMenuTimer = TimeSpan.FromSeconds(5);
                        previousPreviousScore = previousScore;
                        previousScore = score;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.R) && taskRestart)
                    {
                        currentGameState = GameState.Countdown;
                        countdownTimer = TimeSpan.FromSeconds(3);
                        hits = 0;
                        score = 0;
                        totalShots = 0;
                        
                    }
                    
                    //if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                    if (Mouse.GetState().LeftButton == ButtonState.Pressed && leftButtonPreviousReleased)
                    {
                        
                        // mouse position- 3D space
                        Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(new Vector3(Mouse.GetState().X, Mouse.GetState().Y, 0f), camera.projection, camera.view, Matrix.Identity);
                        Vector3 farPoint = GraphicsDevice.Viewport.Unproject(new Vector3(Mouse.GetState().X, Mouse.GetState().Y, 1f), camera.projection, camera.view, Matrix.Identity);
                        Vector3 direction = Vector3.Normalize(farPoint - nearPoint);

                        // Raycast from the camera to check for intersection with the target bounding spheres
                        Ray ray = new Ray(nearPoint, direction);
                        float? intersection = ray.Intersects(targetBoundingSphere);
                        float? intersection1 = ray.Intersects(target1BoundingSphere);
                        float? intersection2 = ray.Intersects(target2BoundingSphere);
                        float? intersection3 = ray.Intersects(target3BoundingSphere);

                        leftButtonReleased = false;
                        //leftButtonPreviousReleased = Mouse.GetState().LeftButton == ButtonState.Released;

                        if (intersection.HasValue)
                        {
                            // Target 1 
                            //targetHit = true;
                            hitSound.Play();
                            Random random = new Random();

                            float newX = (float)random.NextDouble() * 1.0f - 0.5f; //  range adjst
                            float newY = (float)random.NextDouble() * 1.0f - 0.5f; 
                            float newZ = (float)random.NextDouble() * 1.0f - 0.5f;
                            targetWorldMatrix = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(new Vector3(newX, newY, newZ));
                            //targetWorldMatrix = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(new Vector3(crosshairPosition.X, crosshairPosition.Y, 0f));
                            targetBoundingSphere = CalculateModelBoundingSphere(targetModel, targetWorldMatrix);
                            //hits++;
                            //score += 100;
                            if (!targetHit)
                            {
                                hits++;
                                score += 100;
                            }
                            targetHit = true;
                        }

                        else if (intersection1.HasValue)
                        {
                            // Target 2 
                            hitSound.Play();
                            Random random = new Random();
                            float newX = (float)random.NextDouble() * 1.0f - 0.1f; 
                            float newY = (float)random.NextDouble() * 1.0f - 0.1f; 
                            float newZ = (float)random.NextDouble() * 1.0f - 0.1f;
                            //target1WorldMatrix = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(new Vector3(crosshairPosition.X - 1f, crosshairPosition.Y, 0f));
                            target1WorldMatrix = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(new Vector3(newX, newY, newZ));
                            target1BoundingSphere = CalculateModelBoundingSphere(targetModel2, target1WorldMatrix);
                            if (!targetHit)
                            {
                                hits++;
                                score += 100;
                            }
                            targetHit = true;
                        }
                        else if (intersection2.HasValue)
                        {
                            // Target 3 
                            hitSound.Play();
                            Random random = new Random();
                            float newX = (float)random.NextDouble() * 1.0f - 0.5f; 
                            float newY = (float)random.NextDouble() * 1.0f - 0.5f; 
                            float newZ = (float)random.NextDouble() * 1.0f - 0.5f;
                            //target2WorldMatrix = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(new Vector3(crosshairPosition.X + 1f, crosshairPosition.Y, 0f));
                            target2WorldMatrix = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(new Vector3(newX, newY, newZ));
                            target2BoundingSphere = CalculateModelBoundingSphere(targetModel3, target2WorldMatrix);
                            if (!targetHit)
                            {
                                hits++;
                                score += 100;
                            }
                            targetHit = true;
                        }
                        else if (intersection3.HasValue)
                        {
                            // Target 4 
                            hitSound.Play();
                            Random random = new Random();
                            float newX = (float)random.NextDouble() * 1.0f - 0.5f; 
                            float newY = (float)random.NextDouble() * 1.0f - 0.5f; 
                            float newZ = (float)random.NextDouble() * 1.0f - 0.5f;
                            //target3WorldMatrix = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(new Vector3(crosshairPosition.X, crosshairPosition.Y + 1f, 0f));
                            target3WorldMatrix = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(new Vector3(newX, newY, newZ));
                            target3BoundingSphere = CalculateModelBoundingSphere(targetModel, target3WorldMatrix);
                            if (!targetHit)
                            {
                                hits++;
                                score += 100;
                            }
                            targetHit = true;
                        }


                        if (Mouse.GetState().LeftButton == ButtonState.Released)
                        {
                            targetHit = false;
                        }

                        //totalShots++;

                        
                    }
                    else
                    {
                        leftButtonReleased = true;
                        targetHit = false;
                    }
                    break;
            }
            

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            switch (currentGameState)
            {
                case GameState.MainMenu:
                    spriteBatch.DrawString(font4, "Press Enter to Start", new Vector2(100, 100), Color.White);
                    spriteBatch.Draw(menuPage, crosshairPosition, Color.White);
                    break;
                case GameState.Countdown:
                    spriteBatch.DrawString(font2, $"{Math.Ceiling(countdownTimer.TotalSeconds)}", new Vector2(900, 400), Color.White);
                    break;
                case GameState.Task:
                    spriteBatch.Draw(crosshairTexture, crosshairPosition, Color.White);
                    spriteBatch.DrawString(font3, $"{Math.Ceiling(taskTimer.TotalSeconds)}", new Vector2(930, 100), Color.White);
                    spriteBatch.DrawString(font, $"Score: {score}", new Vector2(100, 100), Color.White);
                    spriteBatch.DrawString(font, $"Hits: {hits}", new Vector2(100, 150), Color.White);
                   // spriteBatch.DrawString(font, $"Total Shots: {totalShots}", new Vector2(100, 200), Color.White);
                    spriteBatch.DrawString(font, "Press ESC to Quit", new Vector2(100, 900), Color.White);
                    spriteBatch.DrawString(font, "Press R to RESTART", new Vector2(100, 950), Color.White);
                    DrawModel(targetModel, targetWorldMatrix, camera.view, camera.projection);
                    DrawModel(targetModel2, target1WorldMatrix, camera.view, camera.projection);
                    DrawModel(targetModel3, target2WorldMatrix, camera.view, camera.projection);

                    break;
                case GameState.ScoreMenu:
                    spriteBatch.DrawString(font, $"Previous Score: {previousPreviousScore}", new Vector2(900, 200), Color.White);
                    spriteBatch.DrawString(font3, $"Score: {previousScore}", new Vector2(800, 100), Color.White);
                    spriteBatch.DrawString(font, "Press ESC to Quit", new Vector2(900, 550), Color.White);
                    spriteBatch.DrawString(font, "Press Enter to Play Again", new Vector2(900, 600), Color.White);

                    if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                    {
                        currentGameState = GameState.MainMenu;
                        hits = 0;
                        score = 0;
                        totalShots = 0;
                        countdownTimer = TimeSpan.FromSeconds(3);
                    }
                    break;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                }

                mesh.Draw();
            }
        }

        private BoundingSphere CalculateModelBoundingSphere(Model model, Matrix worldMatrix)
        {
            BoundingSphere result = new BoundingSphere();

            foreach (ModelMesh mesh in model.Meshes)
            {
                BoundingSphere meshSphere = mesh.BoundingSphere;
                meshSphere = meshSphere.Transform(worldMatrix);
                result = BoundingSphere.CreateMerged(result, meshSphere);
            }

            return result;
        }
    }
}