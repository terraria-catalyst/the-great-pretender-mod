using System;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using MonoMod.RuntimeDetour;
using Terraria.ModLoader;

namespace WikiThat {
    [UsedImplicitly]
    internal static class Unused { }
}

namespace TeamCatalyst.WikiThat {
    public sealed class WikiThatMod : Mod {
        private Hook? modContentLoadHook = new (
            typeof(ModContent).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Static, new[] { typeof(CancellationToken) })!,
            ModContentLoad
        );

        public override void Unload() {
            base.Unload();

            modContentLoadHook?.Dispose();
            modContentLoadHook = null;
        }

        private static void ModContentLoad(Action<CancellationToken> orig, CancellationToken token) {
            ModLoader.GetMod("TheGreatPretender").Call("register", "Wikithis", null, (Func<object?[], object?>)WikiThisCallback, new Version(2, 5, 1, 2));
            orig(token);
        }

        private static object? WikiThisCallback(object?[] args) {
            return null;
        }
    }
}
