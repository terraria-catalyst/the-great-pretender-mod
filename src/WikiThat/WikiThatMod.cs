using System;
using JetBrains.Annotations;
using Terraria.ModLoader;

namespace WikiThat {
    [UsedImplicitly]
    internal static class Unused { }
}

namespace TeamCatalyst.WikiThat {
    public sealed class WikiThatMod : Mod {
        public override void Load() {
            base.Load();

            ModLoader.GetMod("TheGreatPretender").Call("RegisterCallCallback", "Wikithis", (Func<object?[], object?>)WikiThisCallback);
        }

        private static object? WikiThisCallback(object?[] args) {
            return null;
        }
    }
}
