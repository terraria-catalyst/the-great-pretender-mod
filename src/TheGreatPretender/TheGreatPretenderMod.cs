using System;
using System.Reflection;
using JetBrains.Annotations;
using Terraria.ModLoader;

namespace TheGreatPretender {
    [UsedImplicitly]
    internal static class Unused { }
}

namespace TeamCatalyst.TheGreatPretender {
    public sealed class TheGreatPretenderMod : Mod {
        public override object? Call(params object?[] args) {
            if (args.Length < 1)
                return base.Call(args);

            if (args[0] is not string command)
                return base.Call(args);

            switch (command.ToLowerInvariant()) {
                case "ispretend": {
                    if (args.Length < 2 || args[1] is not string modName)
                        return false;

                    return PretenderSystem.IsPretend(modName);
                }

                case "register": {
                    if (args.Length < 5 || args[1] is not string modName)
                        return false;

                    var assembly = args[2] as Assembly;
                    var callCallback = args[3] as Func<object?[], object?>;
                    var version = args[4] as Version;

                    PretenderSystem.Register(modName, assembly, callCallback, version);
                    return null;
                }

                default:
                    return base.Call(args);
            }
        }
    }
}
