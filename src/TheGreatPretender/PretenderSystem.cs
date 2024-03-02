using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using MonoMod.RuntimeDetour;
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

    private Hook? getModHook;
    private Hook? tryGetModHook;
    private Hook? hasModHook;

    public override void Load() {
        base.Load();

        getModHook = new Hook(
            typeof(ModLoader).GetMethod("GetMod", new[] { typeof(string) })!,
            GetMod
        );

        tryGetModHook = new Hook(
            typeof(ModLoader).GetMethod("TryGetMod", new[] { typeof(string), typeof(Mod).MakeByRefType() })!,
            TryGetMod
        );

        hasModHook = new Hook(
            typeof(ModLoader).GetMethod("HasMod", new[] { typeof(string) })!,
            HasMod
        );
    }

    public override void Unload() {
        base.Unload();

        getModHook?.Dispose();
        tryGetModHook?.Dispose();
        hasModHook?.Dispose();
    }

    #region Hooks
    private static Mod GetMod(Func<string, Mod> orig, string name) {
        return pretenders.TryGetValue(name, out var mod) ? mod : orig(name);
    }

    private delegate bool TryGetModDelegate(string name, out Mod mod);

    private static bool TryGetMod(TryGetModDelegate orig, string name, out Mod mod) {
        if (!pretenders.TryGetValue(name, out var pretendMod))
            return orig(name, out mod);

        mod = pretendMod;
        return true;
    }

    private static bool HasMod(Func<string, bool> orig, string name) {
        return pretenders.ContainsKey(name) || orig(name);
    }
    #endregion

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

        pretenders[modName] = new PretendMod(modName) {
            // TODO: Create dummy TmodFile?
            Logger = LogManager.GetLogger("[PRETENDER] " + modName),
            Side = ModSide.NoSync,
            DisplayName = modName,
            TModLoaderVersion = BuildInfo.tMLVersion,
        };
    }
}
