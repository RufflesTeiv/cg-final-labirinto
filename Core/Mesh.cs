using OpenTK.Graphics.OpenGL;

namespace CG.Core
{
    // Classe de abstração do conceito de Mesh. Recebe, em seu construtor, os dados brutos dos vér-
    //tices e triângulos(índices). Depois, poderemos especificar os atributos da malha com a função
    //AddAttribute.
    internal class Mesh
    {
        private int _vbo = 0;// vertex buffer object
        private int _ebo = 0;// element buffer object
        private int _vao = 0;// vertex array object
        private int _count = 0;// contagem de vértices

        public Mesh(float[] vertices, uint[] indices)
        {
            // O primeiro passo na criação da malha é o envio dos vértices, de forma bruta, para a
            //placa de vídeo. Isso é feito por meio de um Vertex Buffer Object.
            _vbo = GL.GenBuffer();// Vertex Buffer Object
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);

            // Em seguida, precisamos criar o Vertex Array Object, que é responsável por "explicar"
            //para a placa de vídeo como funcionam os vértices.
            _vao = GL.GenVertexArray();// Vertex Array Object
            GL.BindVertexArray(_vao);

            // Em malhas mais complexas, é comum que especifiquemos de maneira mais direta os tri-
            //ângulos que a formam. Para isso podemos utilizar element buffers, que vão representar
            //a relação entre os vértices por meio de índices. Índices não podem ser negativos, por
            //isso, podemos utilizar o tipo uint(int sem sinal) para representar os dados.
            _ebo = GL.GenBuffer();// Element Buffer Object
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(uint) * indices.Length, indices, BufferUsageHint.StaticDraw);

            _count = indices.Length;

            GL.BindVertexArray(0);// Unbind para evitar uso incorreto da malha.
        }

        // Construtor simplificado, que não recebe os índices. Quando utilizado, a malha será dese-
        //nhada com a função GL.DrawArrays, no modo TriangleFan.
        public Mesh(float[] vertices, int count)
        {
            _vbo = GL.GenBuffer();// Vertex Buffer Object
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);

            // A contagem de vértices é manual, uma vez que não sabemos seu formato antes de espe-
            //cificar com o VertexArray.
            _count = count;

            _vao = GL.GenVertexArray();// Vertex Array Object
        }

        public void AddAttribute(int id, int size, int stride, int offset = 0)
        {
            // Uma malha normalmente vai conter atributos, dados por vértice, que podem representar
            //posição, cor, direção, coordenadas de textura ou outras informações.
            //
            // Para cada atributo que esteja presente na nossa malha, precisamos fazer a sua habi-
            //litação com a função EnableVertexAttribArray, indicando o seu índice.
            // Além disso, precisamos "explicar" para a GPU o que é o atributo e como acessá-lo com
            //a função VertexAttribPointer, que tem uma série de parâmetros importantes:
            //1. index: O índice do atributo, o mesmo usado para habilitá-lo.
            //2. size: Quantos componentes tem o atributo? Deve ser 1, 2, 3 ou 4, indicando vetores
            //de uma a quatro dimensões.
            //3. type: Tipo do atributo. No nosso caso, estamos utilizando floats.
            //4. normalized: Indica se os valores estão normalizados. Essa opção pode normalmente
            //ser deixada como false.
            //5. stride: Indica de quantos em quantos bytes o atributo aparece no array.
            //6. offset: Indica quantos bytes a partir do início do array devem ser pulados até en-
            //contrar o atributo.
            GL.BindVertexArray(_vao);

            GL.EnableVertexAttribArray(id);
            GL.VertexAttribPointer(id, size, VertexAttribPointerType.Float, false, stride, offset);

            GL.BindVertexArray(0);// Unbind para evitar uso incorreto da malha.
        }

        // Função de ajuda para adicionar vários atributos de vértice sequenciais sem a necessidade
        //de especificação individual.
        public void AddAttributes(params int[] sizes) {
            int stride = 0;
            foreach (int size in sizes) {
                stride += size * sizeof(float);
            }

            int offset = 0;
            for (int i = 0; i < sizes.Length; i++)
            {
                AddAttribute(i, sizes[i], stride, offset);
                offset += sizes[i] * sizeof(float);
            }
        }

        public void Draw()
        {
            GL.BindVertexArray(_vao);

            // Funções que geram a Draw Call para a placa de vídeo. Primeiro, BindVertexArray indi-
            //ca qual malha será utilizada. Depois, DrawElements executa o desenho com o array de
            //índices especificados anteriormente. Caso os índices não tenham sido especificados,
            //utilizamos a função DrawArrays no modo triangle fan.
            // Ali, especificamos como primitivos os triângulos e informamos de índices que serão
            //utilizados. O tipo dos índices(UnsignedInt/uint) também é especificado. O último pa-
            //râmetro é mantido como 0, pois o Element Buffer Object já está associado diretamente
            //ao Vertex Buffer Object.
            if (_ebo != 0)
            {
                GL.DrawElements(PrimitiveType.Triangles, _count, DrawElementsType.UnsignedInt, 0);
            }
            else
            {
                GL.DrawArrays(PrimitiveType.TriangleFan, 0, _count);
            }

            GL.BindVertexArray(0);// Unbind após a conclusão do desenho.
        }

        // A placa de vídeo não conta com um Garbage Collector, por isso devemos chamar manualmente
        //as funções de limpeza de memória quando não estivermos mais utilizando a malha.
        public void Delete()
        {
            GL.DeleteVertexArray(_vao);
            if (_ebo != 0)
            {
                GL.DeleteBuffer(_ebo);
            }
            GL.DeleteBuffer(_vbo);
        }

        // Funções estáticas de criação de primitivos
        public static Mesh CreatePrimitivePlane(float width, float length)
        {
            float hw = width / 2f;
            float hl = length / 2f;

            float[] vertices =
            [
                // Posição      // Normal      // UV
                -hw, 0f,  hl,   0f,  1f,  0f,  0f, 0f,// 0
                 hw, 0f,  hl,   0f,  1f,  0f,  1f, 0f,
                 hw, 0f, -hl,   0f,  1f,  0f,  1f, 1f,
                -hw, 0f, -hl,   0f,  1f,  0f,  0f, 1f,
            ];

            uint[] indices =
            [
                0, 1, 2,
                0, 2, 3,
            ];

            Mesh mesh = new(vertices, indices);
            mesh.AddAttributes(3, 3, 2);
            return mesh;
        }

        public static Mesh CreatePrimitivePlane(float size = 1f)
        {
            return CreatePrimitivePlane(size, size);
        }

        public static Mesh CreatePrimitiveCube(float size = 1f)
        {
            float h = size / 2f;

            float[] vertices =
            [
                // Posição    // Normal      // UV
                -h, -h,  h,   0f,  0f,  1f,  0f, 0f,// 0
                 h, -h,  h,   0f,  0f,  1f,  1f, 0f,
                 h,  h,  h,   0f,  0f,  1f,  1f, 1f,
                -h,  h,  h,   0f,  0f,  1f,  0f, 1f,

                 h, -h, -h,   0f,  0f, -1f,  0f, 0f,// 4
                -h, -h, -h,   0f,  0f, -1f,  1f, 0f,
                -h,  h, -h,   0f,  0f, -1f,  1f, 1f,
                 h,  h, -h,   0f,  0f, -1f,  0f, 1f,

                 h, -h,  h,   1f,  0f,  0f,  0f, 0f,// 8
                 h, -h, -h,   1f,  0f,  0f,  1f, 0f,
                 h,  h, -h,   1f,  0f,  0f,  1f, 1f,
                 h,  h,  h,   1f,  0f,  0f,  0f, 1f,

                -h, -h, -h,  -1f,  0f,  0f,  0f, 0f,// 12
                -h, -h,  h,  -1f,  0f,  0f,  1f, 0f,
                -h,  h,  h,  -1f,  0f,  0f,  1f, 1f,
                -h,  h, -h,  -1f,  0f,  0f,  0f, 1f,

                -h,  h,  h,   0f,  1f,  0f,  0f, 0f,// 16
                 h,  h,  h,   0f,  1f,  0f,  1f, 0f,
                 h,  h, -h,   0f,  1f,  0f,  1f, 1f,
                -h,  h, -h,   0f,  1f,  0f,  0f, 1f,

                -h, -h, -h,   0f, -1f,  0f,  0f, 0f,// 20
                 h, -h, -h,   0f, -1f,  0f,  1f, 0f,
                 h, -h,  h,   0f, -1f,  0f,  1f, 1f,
                -h, -h,  h,   0f, -1f,  0f,  0f, 1f,
            ];

            uint[] indices =
            [
                0, 1, 2,
                0, 2, 3,

                4, 5, 6,
                4, 6, 7,

                8, 9, 10,
                8, 10, 11,

                12,13, 14,
                12, 14, 15,

                16, 17, 18,
                16, 18, 19,

                20, 21, 22,
                20, 22, 23,
            ];

            Mesh mesh = new(vertices, indices);
            mesh.AddAttributes(3, 3, 2);return mesh;
        }
    }
}
