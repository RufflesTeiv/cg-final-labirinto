using NetCoreAudio;

namespace CG
{
    enum SFX
    {
        Step = 0,
        Key = 1,
        Win = 2
    }

    internal static class AudioManager
    {
        private static List<string> sfxFiles = new List<string>()
        {
            "./assets/sounds/step.wav",
            "./assets/sounds/key.wav",
            "./assets/sounds/win.wav"
        };

        public static void PlaySFX(SFX sfx)
        {
            var player = new Player();
            player.Play(sfxFiles[(int)sfx]);
        }
    }
}