using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BasicOpenTK
{
    public class Game : GameWindow
    {
        private int vertexBufferObject;
        private int vertexArrayObject;
        private int elementBufferObject;
        private int shaderProgramObject;

        private Matrix4 projectionMatrix;
        private Matrix4 viewMatrix;
        private Matrix4 modelMatrix;

        public Game()
            : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.CenterWindow(new Vector2i(800, 600));
        }

        protected override void OnLoad()
        {
            GL.ClearColor(new Color4(0.3f, 0.4f, 0.5f, 1f));

            // Define los vértices para una "T" 3D
            float[] vertices = new float[]
            {
                // Front face (parte frontal)
                -0.5f, 0.5f, 0.1f,   // Vértice superior izquierdo
                 0.5f, 0.5f, 0.1f,   // Vértice superior derecho
                -0.5f, 0.3f, 0.1f,   // Vértice inferior izquierdo
                 0.5f, 0.3f, 0.1f,   // Vértice inferior derecho

                -0.1f, 0.3f, 0.1f,   // Vértice superior izquierdo del tronco
                 0.1f, 0.3f, 0.1f,   // Vértice superior derecho del tronco
                -0.1f, -0.5f, 0.1f,  // Vértice inferior izquierdo del tronco
                 0.1f, -0.5f, 0.1f,  // Vértice inferior derecho del tronco

                // Back face (parte trasera)
                -0.5f, 0.5f, -0.1f,  // Vértice superior izquierdo
                 0.5f, 0.5f, -0.1f,  // Vértice superior derecho
                -0.5f, 0.3f, -0.1f,  // Vértice inferior izquierdo
                 0.5f, 0.3f, -0.1f,  // Vértice inferior derecho

                -0.1f, 0.3f, -0.1f,  // Vértice superior izquierdo del tronco
                 0.1f, 0.3f, -0.1f,  // Vértice superior derecho del tronco
                -0.1f, -0.5f, -0.1f, // Vértice inferior izquierdo del tronco
                 0.1f, -0.5f, -0.1f  // Vértice inferior derecho del tronco
            };

            int[] indices = new int[]
            {
                // Front face
                0, 1, 2, 1, 2, 3,
                4, 5, 6, 5, 6, 7,

                // Back face
                8, 9, 10, 9, 10, 11,
                12, 13, 14, 13, 14, 15,

                // Connecting sides (connecting front and back faces)
                0, 2, 8, 10, 2, 8, // Left side of top bar
                1, 3, 9, 11, 3, 9, // Right side of top bar
                4, 6, 12, 14, 6, 12, // Left side of vertical bar
                5, 7, 13, 15, 7, 13  // Right side of vertical bar
            };

            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            // Cargar los vértices en un VBO
            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Cargar los índices en un EBO
            elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            string vertexShaderCode =
            @"
                #version 330 core

                layout (location = 0) in vec3 aPosition;

                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;

                void main()
                {
                    gl_Position = projection * view * model * vec4(aPosition, 1.0);
                }";

            string fragmentShaderCode =
            @"
                #version 330 core

                out vec4 pixelColor;

                void main()
                {
                    pixelColor = vec4(0.8f, 0.8f, 0.1f, 1.0f);
                }";

            int vertexShaderObject = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderObject, vertexShaderCode);
            GL.CompileShader(vertexShaderObject);

            int fragmentShaderObject = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShaderObject, fragmentShaderCode);
            GL.CompileShader(fragmentShaderObject);

            shaderProgramObject = GL.CreateProgram();
            GL.AttachShader(shaderProgramObject, vertexShaderObject);
            GL.AttachShader(shaderProgramObject, fragmentShaderObject);
            GL.LinkProgram(shaderProgramObject);

            GL.DetachShader(shaderProgramObject, vertexShaderObject);
            GL.DetachShader(shaderProgramObject, fragmentShaderObject);
            GL.DeleteShader(vertexShaderObject);
            GL.DeleteShader(fragmentShaderObject);

            // Configurar las matrices de proyección y vista
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), Size.X / (float)Size.Y, 0.1f, 100.0f);
            viewMatrix = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f); // Aleja la cámara
            modelMatrix = Matrix4.Identity;

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vertexBufferObject);
            GL.DeleteBuffer(elementBufferObject);

            GL.UseProgram(0);
            GL.DeleteProgram(shaderProgramObject);

            base.OnUnload();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), e.Width / (float)e.Height, 0.1f, 100.0f);
            base.OnResize(e);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(shaderProgramObject);

            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgramObject, "model"), false, ref modelMatrix);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgramObject, "view"), false, ref viewMatrix);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgramObject, "projection"), false, ref projectionMatrix);

            GL.BindVertexArray(vertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);

            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        public static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
