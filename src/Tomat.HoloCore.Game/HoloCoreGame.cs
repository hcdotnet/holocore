using System;
using System.Collections.Generic;
using Tomat.HoloCore.Framework;
using Tomat.HoloCore.Framework.Platform.Windowing;
using Veldrid;

namespace Tomat.HoloCore.Game;

public abstract class HoloCoreGame : Framework.Game {
    private List<GameWindow> windows = new();

    public override void Initialize() {
        base.Initialize();

        if (Dependencies is null)
            throw new InvalidOperationException("Attempted to initialize game before dependencies were registered.");

        for (var i = 0; i < 10; i++) {
            var windowInfo = new WindowCreationInfo {
                X = 100 + (i * 150),
                Y = 100 + (i * 10),
                Width = 960,
                Height = 540,
                Title = $"Test #{i}",
            };
            var gdOptions = new GraphicsDeviceOptions {
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true,
            };
            _ = CreateWindow<HoloCoreGameWindow>(windowInfo, gdOptions);
        }
    }
}
