using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Camera : Microsoft.Xna.Framework.GameComponent
{


    public Matrix view { get; protected set; }
    public Matrix projection { get; protected set; }

    public Vector3 cameraPosition { get; protected set; }
    Vector3 cameraDirection;
    Vector3 cameraUp;



    //CAMERA MOVEMENT SPEED
    float speed = -0.01F;

    MouseState prevMouseState;

    public Camera(Game game, Vector3 pos, Vector3 target, Vector3 up)
        : base(game)
    {
        // TODO

        // Build camera view matrix
        cameraPosition = pos;
        cameraDirection = target - pos;
        cameraDirection.Normalize();
        cameraUp = up;
        CreateLookAt();

        projection = Matrix.CreateOrthographic(2.5f, 2.5f, 0.1f, 20f);
        //projection = Matrix.CreateOrthographic(1.5f, 1.5f, 0.1f, 10f);
        //projection = Matrix.CreatePerspectiveFieldOfView(2.5f, 2.5f, 1f, 100f);
        //projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)Game.Window.ClientBounds.Width / (float)Game.Window.ClientBounds.Height, 1, 50);
    }
    /// Allows the game component to perform any initialization it needs to before starting
    /// to run.  This is where it can query for any required services and load content.
    /// </summary>
    public override void Initialize()
    {
        // TODO: Add your initialization code here

        // Set mouse position and do initial get state
        Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);

        prevMouseState = Mouse.GetState();

        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        // TODO: Add your update code here


        //Calculate the mouse delta
        MouseState mouseState = Mouse.GetState();
        int deltaX = mouseState.X - (Game.Window.ClientBounds.Width / 2);
        int deltaY = mouseState.Y - (Game.Window.ClientBounds.Height / 2);

        // Rotate the camera based on the mouse delta
        float rotationSpeed = 0.01f;
        cameraDirection = Vector3.Transform(cameraDirection, Matrix.CreateFromAxisAngle(cameraUp, rotationSpeed * deltaX));
        cameraDirection = Vector3.Transform(cameraDirection, Matrix.CreateFromAxisAngle(Vector3.Cross(cameraUp, cameraDirection), -rotationSpeed * deltaY));

        // Reset the previous mouse state
        Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);

        CreateLookAt();

        base.Update(gameTime);
    }

    private void CreateLookAt()
    {
        view = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraDirection, cameraUp);
    }

}