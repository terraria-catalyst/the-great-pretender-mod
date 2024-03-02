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

                case "registercallcallback": {
                    if (args.Length < 3 || args[1] is not string modName || args[2] is not Func<object?[], object?> callCallback)
                        return false;

                    PretenderSystem.RegisterCallCallback(modName, callCallback);
                    return null;
                }

                case "registerassembly": {
                    if (args.Length < 3 || args[1] is not string modName || args[2] is not Assembly assembly)
                        return false;

                    PretenderSystem.RegisterAssembly(modName, assembly);
                    return null;
                }

                default:
                    return base.Call(args);
            }
        }
    }
}
