using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JohnStriker.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace JohnStriker.Models
{
    class TerrainModel : IModel
    {
        private VertexPositionNormalTexture[] VertexPosition; //vertex array
        private VertexBuffer VertexBuffer; //vertex buffer
        private int[] Indices; //index array
        private IndexBuffer IndexBuffer; //index buffer
        private float[,] HeightVertex; // vertex height
        private float Height; // height terrain;
        private float CellSize; //distance between vertices on x and z axes;
        private int Width, Length; //number vertices on x and z axes;
        private int Nvertices, Nindexes; //number of vertices and indexes;
        private Effect Effect; //effect used for rendering
        private GraphicsDevice GraphicsDevice;
        private Texture2D HeightMap; //heightmap texture;

        private Texture2D BaseTexture;
        private float TextureTiling;
        private Vector3 LightDirection;

        public Texture2D RTexture;
        public Texture2D GTexture;
        public Texture2D BTexture;
        public Texture2D WeightMap;
        public Texture2D DetailTexture;
        private float DetailDistance = 2500;
        private float DetailTextureTiling = 100;
        
        public TerrainModel(Texture2D heightMap, float cellSize, float height, Texture2D baseTexture, float textureTiling, Vector3 lightDirection,
            ContentManager contentManager, GraphicsDevice gd)
        {
            HeightMap = heightMap;
            Width = HeightMap.Width;
            Length = HeightMap.Height;
            CellSize = cellSize;
            Height = height;
            BaseTexture = baseTexture;
            TextureTiling = textureTiling;
            LightDirection = lightDirection;
            GraphicsDevice = gd;

            Effect = contentManager.Load<Effect>("Shaders/TerrainEffect");

            Nvertices = Width * Length; // 1 vertex per pixel
            Nindexes = (Width - 1) * (Length - 1) * 6;

            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), Nvertices, BufferUsage.WriteOnly);

            IndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, Nindexes, BufferUsage.WriteOnly);

            GetHeight();
            CreateVertices();
            CreatreIndices();
            GenNormals();

            VertexBuffer.SetData<VertexPositionNormalTexture>(VertexPosition);
            IndexBuffer.SetData<int>(Indices);
        }

        public void GetHeight()
        {
            //extract pixel data
            Color[] HeightMapData = new Color[Width * Length];
            HeightMap.GetData<Color>(HeightMapData);

            HeightVertex = new float[Width, Length];

            //for each pixel
            for (int y = 0; y < Length; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    //get color value 0 to 255
                    float amt = HeightMapData[y * Width + x].R;

                    //scale to 0 - 1;
                    amt /= 255.0f;

                    //multiply by max height to get final hieght
                    HeightVertex[x, y] = amt * Height;
                }
            }
        }

        public void CreateVertices()
        {
            VertexPosition = new VertexPositionNormalTexture[Nvertices];
            //calculate the position offset that will position the terrain (0,0,0)

            Vector3 offSetToCounter = -new Vector3(((float)Width / 2.0f) * CellSize, 0, ((float)Length / 2.0f) * CellSize);
            //for each pixel in the image
            for (int y = 0; y < Length; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    //find the posiition based on grd coordinates and heightmap
                    Vector3 position = new Vector3(x * CellSize, HeightVertex[x, y], y * CellSize) + offSetToCounter;

                    //uv coordinates range from (0,0) at grid location (0,0) to (1,1) at grid location width and height
                    Vector2 uv = new Vector2((float)x / Width, (float)y / Length);

                    //create the vertex
                    VertexPosition[y * Width + x] = new VertexPositionNormalTexture(position, Vector3.Zero, uv);
                }
            }

        }

        public void CreatreIndices()
        {
            Indices = new int[Nindexes];

            int i = 0;
            //for each call
            for (int y = 0; y < Width-1; y++)
            {
                for (int x = 0; x < Length-1; x++)
                {
                    //find the indices of the corner
                    int upperLeft = y * Width + x;
                    int upperRight = upperLeft + 1;
                    int lowerLeft = upperLeft + Width;
                    int lowerRight = lowerLeft + 1;

                    //specify upper triangle
                    Indices[i++] = upperLeft;
                    Indices[i++] = upperRight;
                    Indices[i++] = lowerLeft;

                    //specify lower triangle
                    Indices[i++] = lowerLeft;
                    Indices[i++] = upperRight;
                    Indices[i++] = lowerRight;
                }
            }
        }

        public void GenNormals()
        {
            //find each triangle
            for (int i = 0; i < Nindexes; i += 3)
            {
                //find the position of the each corner triangle
                Vector3 v1 = VertexPosition[Indices[i]].Position;
                Vector3 v2 = VertexPosition[Indices[i+1]].Position;
                Vector3 v3 = VertexPosition[Indices[i+2]].Position;

                //cross the vectors between the cornwers to get normal
                Vector3 normal = Vector3.Cross((v1 - v2), (v1 - v3));
                normal.Normalize();

                
                // Add the influence of the normal to each vertex in the
                // triangle
                VertexPosition[Indices[i]].Normal += normal;
                VertexPosition[Indices[i + 1]].Normal += normal;
                VertexPosition[Indices[i + 2]].Normal += normal;

            }

             // Average the influences of the triangles touching each
            // vertex
            for (int i = 0; i < Nvertices; i++)
                VertexPosition[i].Normal.Normalize();
        }

        //returns the height and steepness of the terrain at point (x,y)
        public float GetHeightAtPosition(float x, float z, out float steepness)
        {
            //clamp coordinates to locations on terrain
            //it used to restrict the value with certain range
            x = MathHelper.Clamp(x, (-Width / 2) * CellSize, (Width / 2) * CellSize);
            z = MathHelper.Clamp(z, (-Length / 2) * CellSize, (Length / 2) * CellSize);

            x += (Width / 2f) * CellSize;
            z += (Length / 2f) * CellSize;

            //map to cell coordinates
            x /= CellSize;
            z /= CellSize;

            //truncate coordinates to get coordinates of top left cell vertex
            int x1 = (int)x;
            int z1 = (int)z;

            //try to get coordinates of bottom right cell vertex;
            int x2 = x1 + 1 == Width ? x1 : x1 + 1;
            int z2 = z1 + 1 == Width ? z1 : z1 + 1;

            //get the height of the two corners of the cell.
            float h1 = HeightVertex[x1, z1];
            float h2 = HeightVertex[x2, z2];

            //determine steepness (angle between higher and lower vertex of cell)
            //returns the angle whose tanget is specified number
            steepness = (float)Math.Atan(Math.Abs((h1 - h2)) / (CellSize * Math.Sqrt(2)));

            //find the average of amount lost from coordinates during the truncate above
            float leftOver = ((x - x1) + (z - z1)) / 2f;

            //interpolate between corner vertices and heights
            return MathHelper.Lerp(h1, h2, leftOver);
        }

        public void Update(GameTime time)
        {
           
        }

        public void Draw(ChaseCamera chaseCamera)
        {
            GraphicsDevice.SetVertexBuffer(VertexBuffer);
            GraphicsDevice.Indices = IndexBuffer;

            Effect.Parameters["View"].SetValue(chaseCamera.View);
            Effect.Parameters["Projection"].SetValue(chaseCamera.Projection);
            Effect.Parameters["BaseTexture"].SetValue(BaseTexture);
            Effect.Parameters["TextureTiling"].SetValue(TextureTiling);
            Effect.Parameters["LightDirection"].SetValue(LightDirection);

            Effect.Parameters["RTexture"].SetValue(RTexture);
            Effect.Parameters["GTexture"].SetValue(GTexture);
            Effect.Parameters["BTexture"].SetValue(BTexture);
            Effect.Parameters["WeightMap"].SetValue(WeightMap);

            Effect.Parameters["DetailTexture"].SetValue(DetailTexture);
            Effect.Parameters["DetailDistance"].SetValue(DetailDistance);
            Effect.Parameters["DetailTextureTiling"].SetValue(DetailTextureTiling);

            Effect.Techniques[0].Passes[0].Apply();

            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                Nvertices, 0, Nindexes / 4);


        }

        public void SetClipPlane(Vector4? Plane)
        {
            Effect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);

            if (Plane.HasValue)
                Effect.Parameters["ClipPlane"].SetValue(Plane.Value);
        }
    }
}
