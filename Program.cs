// See https://aka.ms/new-console-template for more information


using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using CG;
using StbImageSharp;

StbImage.stbi_set_flip_vertically_on_load(1);// Inversão das coordenadas de textura.

NativeWindowSettings settings = new()
{
    Title = "Meu Jogo",
    ClientSize = new(800, 600),
    APIVersion = new Version(4,1), // Teto de versão para MacOS
    Flags = ContextFlags.ForwardCompatible
};
Game game = new(GameWindowSettings.Default, settings);
game.Run();
