using FbonizziMonoGame;

namespace MarkInDeDarkMono.Maths
{
    public static class Utilities
    {
        public static T RandomBetween<T>(params T[] choices)
            => choices[Numbers.RandomBetween(0, choices.Length)];
    }
}
