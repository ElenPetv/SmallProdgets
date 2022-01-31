using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK;

namespace RotatingCube
{
    class Program
    {
        static void Main(string[] args)
        {
            
            using (var window = new Window())
            {
                window.Title = "OpenGL 3, C#";
                window.Run(60);
            }
        }
    }

    class Window : GameWindow
    {
        private Matrix4 _projMatrix;
        private Matrix4 _viewMatrix;
        private Matrix4 _modelMatrix;
        private Matrix4 _mvpMatrix;
        private int _uMvpMatrixLocation;
        private int _uNormalMatrixLocation;
        private int _uColor;
        private int _amountOfVertices = 0;

        private float angleRad = 0;
        private float anglScale = 1;
        private int flag = 1;


        public Window() : base(512, 512, new GraphicsMode(32, 24, 0, 8)) { }

        protected override void OnLoad(EventArgs e)
        {
            var vShaderSource =
                @"
                                #version 130
                                in vec3 aPosition;
                                in vec4 aNormal;
                                uniform mat4 uMvpMatrix;
                                uniform mat4 uNormalMatrix;
                                out float vNdotL;

                                void main()
                                {
                                    gl_Position = uMvpMatrix * vec4(aPosition, 1.0);
                                    vec4 normal = uNormalMatrix * aNormal;
                                    vec3 lightDir = vec3(1, 5, 3);
                                    vNdotL = max(dot(normalize(normal.xyz), normalize(lightDir)), 0.0);
                                }
                            ";


            var fShaderSource =
                @"
                                #version 130
                                precision mediump float;
                                in float vNdotL;
                                uniform vec4 uColor;
                                out vec4 fragColor;

                                void main()
                                {
                                    vec3 diffuseLight = vec3(1.0, 1.0, 1.0);
                                    vec3 diffuseColor = diffuseLight * uColor.rgb * vNdotL;
                                    vec3 ambientLight = vec3(0.2, 0.2, 0.2);
                                    vec3 ambientColor = ambientLight * uColor.rgb;
                                    fragColor = vec4(diffuseColor + ambientColor, uColor.a);
                                }
            ";


            var vShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vShader, vShaderSource);
            GL.CompileShader(vShader);

     

            var fShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fShader, fShaderSource);
            GL.CompileShader(fShader);

            var program = GL.CreateProgram();
            GL.AttachShader(program, vShader);
            GL.AttachShader(program, fShader);
            GL.LinkProgram(program);
            GL.UseProgram(program);

            _amountOfVertices = InitVertexBuffers(program);

            _uMvpMatrixLocation = GL.GetUniformLocation(program, "uMvpMatrix");
            _uNormalMatrixLocation = GL.GetUniformLocation(program, "uNormalMatrix");
            _uColor = GL.GetUniformLocation(program, "uColor");
            GL.Uniform4(_uColor, 1f, 0f, 0f, 1f);



            //_viewMatrix = Matrix4.LookAt(eye: new Vector3(0f, 0f, 10f), target: new Vector3(0f, 0f, 0f), up: new Vector3(0f, 1f, 0f));
            _viewMatrix = Matrix4.LookAt(eye: new Vector3(3, 7, 10.0f), target: new Vector3(0f, 0f, 0f), up: new Vector3(0f, 1f, 0f));



            _modelMatrix = Matrix4.Identity;

            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Enable(EnableCap.DepthTest);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);


            angleRad += 0.01f;
            _modelMatrix = Matrix4.CreateScale(anglScale, anglScale, anglScale) * Matrix4.CreateRotationY(angleRad);
            _mvpMatrix = _modelMatrix * _viewMatrix * _projMatrix;

            if (flag == 1) anglScale += 0.01f;
            else anglScale -= 0.01f;

            if (anglScale >= 2.0f) flag = 2;
            if (anglScale == 1.0f) flag = 1;
          


        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UniformMatrix4(_uNormalMatrixLocation, false, ref _modelMatrix);
            GL.UniformMatrix4(_uMvpMatrixLocation, false, ref _mvpMatrix);

            GL.DrawElements(PrimitiveType.Triangles, _amountOfVertices, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
           _projMatrix = Matrix4.CreatePerspectiveFieldOfView(0.78f, (float)Width / Height, 1f, 100f);
        }

        private int InitVertexBuffers(int program)
        {
            // Create a cube
            //    v6----- v5
            //   /|      /|
            //  v1------v0|
            //  | |     | |
            //  | |v7---|-|v4
            //  |/      |/
            //  v2------v3

            float[] vertices = new float[]
            {
               1f, 1f, 1f, -1f, 1f, 1f, -1f, -1f, 1f, 1f, -1f, 1f,      // v0-v1-v2-v3 front
               1f, 1f, 1f, 1f, -1f, 1f, 1f, -1f, -1f, 1f, 1f, -1f,      // v0-v3-v4-v5 right
               1f, 1f, 1f, 1f, 1f, -1f, -1f, 1f, -1f, -1f, 1f, 1f,      // v0-v5-v6-v1 up
              -1f, 1f, 1f, -1f, 1f, -1f, -1f, -1f, -1f, -1f, -1f, 1f,   // v1-v6-v7-v2 left
              -1f, -1f, -1f, 1f, -1f, -1f, 1f, -1f, 1f, -1f, -1f, 1f,   // v7-v4-v3-v2 down
               1f, -1f, -1f, -1f, -1f, -1f, -1f, 1f, -1f, 1f, 1f, -1f   // v4-v7-v6-v5 back
            };

            float[] normals = new float[]
            {
                0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f,     // v0-v1-v2-v3 front
                1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f,     // v0-v3-v4-v5 right
                0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f,     // v0-v5-v6-v1 up
                -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, // v1-v6-v7-v2 left
                0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, // v7-v4-v3-v2 down
                0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f, 0f, 0f, -1f  // v4-v7-v6-v5 back
            };

            int[] indices = new int[]
            {
                0, 1, 2, 0, 2, 3,       // front
                4, 5, 6, 4, 6, 7,       // right
                8, 9, 10, 8, 10, 11,    // up
                12, 13, 14, 12, 14, 15, // left
                16, 17, 18, 16, 18, 19, // down
                20, 21, 22, 20, 22, 23  // back
            };

            InitArrayBuffer(program, vertices, "aPosition");
            InitArrayBuffer(program, normals, "aNormal");

            int indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * indices.Length, indices, BufferUsageHint.StaticDraw);

            return indices.Length;
        }

        private void InitArrayBuffer(int program, float[] data, string attributeName)
        {
            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Length, data, BufferUsageHint.StaticDraw);
           
            int attributeLocation = GL.GetAttribLocation(program, attributeName);
            GL.VertexAttribPointer(attributeLocation, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(attributeLocation);
        }
    }
}