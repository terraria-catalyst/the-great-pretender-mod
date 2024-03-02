using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace TeamCatalyst.TheGreatPretender;

internal sealed class PretenderSystem : ModSystem {
    private sealed class PretendMod : Mod {
        public override string Name { get; }

        internal Func<object?[], object?>? CallCallback { get; set; }

        public PretendMod(string name) {
            Name = name;
        }

        public override object? Call(params object?[] args) {
            return CallCallback?.Invoke(args);
        }
    }

    private static readonly Dictionary<string, PretendMod> pretenders = new();

    public static bool IsPretend(string modName) {
        return pretenders.ContainsKey(modName);
    }

    public static void RegisterCallCallback(string modName, Func<object?[], object?>? callCallback) {
        if (!pretenders.TryGetValue(modName, out var mod))
            return;

        mod.CallCallback = callCallback;
    }

    public static void RegisterAssembly(string modName, Assembly assembly) {
        if (!pretenders.TryGetValue(modName, out var mod))
            return;

        mod.Code = assembly;
    }

    private static void GetOrCreatePretender(string modName, out PretendMod mod) {
        if (pretenders.TryGetValue(modName, out mod!))
            return;

        mod = new PretendMod(modName) {
            // TODO: Create dummy TmodFile?
            Logger = LogManager.GetLogger("[PRETENDER] " + modName),
            Side = ModSide.NoSync,
            DisplayName = modName,
            TModLoaderVersion = BuildInfo.tMLVersion,
        };
    }
}
