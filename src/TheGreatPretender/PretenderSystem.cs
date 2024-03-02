using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using log4net;
using MonoMod.RuntimeDetour;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace TeamCatalyst.TheGreatPretender;

internal sealed class PretenderSystem : ModSystem {
    [ExtendsFromMod("")]
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

    private static Hook? getModHook;
    private static Hook? tryGetModHook;
    private static Hook? hasModHook;

    /*public override void Load() {
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
    }*/

#pragma warning disable CA2255
    [ModuleInitializer]
    internal static void Init() {
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
#pragma warning restore CA2255

    public override void Unload() {
        base.Unload();

        getModHook?.Dispose();
        getModHook = null;
        tryGetModHook?.Dispose();
        tryGetModHook = null;
        hasModHook?.Dispose();
        hasModHook = null;
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

    public static void Register(string modName, Assembly? assembly, Func<object?[], object?>? callCallback, Version? version) {
        var mod = GetOrCreatePretender(modName);

        mod.Code ??= assembly;
        mod.CallCallback ??= callCallback;
        mod.File.Version ??= version;
    }

    private static PretendMod GetOrCreatePretender(string modName) {
        if (pretenders.TryGetValue(modName, out var mod))
            return mod;

        return pretenders[modName] = new PretendMod(modName) {
            File = new TmodFile(""),
            Logger = LogManager.GetLogger("[PRETENDER] " + modName),
            Side = ModSide.NoSync,
            DisplayName = modName,
            TModLoaderVersion = BuildInfo.tMLVersion,
        };
    }
}
