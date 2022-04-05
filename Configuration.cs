using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace BedFix
{
    [Label("$Mods.BedFix.Configs.Title")]
    public class Configuration : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        public override void OnLoaded() => BedSystem.Config = this;

        [DefaultValue(false)]
        [Label("$Mods.BedFix.Configs.Label.ValidHouseRequirement")]
        [Tooltip("$Mods.BedFix.Configs.Tooltip.ValidHouseRequirement")]
        public bool ValidHouseRequirement { get; set; }

        [DefaultValue(true)]
        [Label("$Mods.BedFix.Configs.Label.FallAsleepAlways")]
        [Tooltip("$Mods.BedFix.Configs.Tooltip.FallAsleepAlways")]
        public bool FallAsleepAlways { get; set; }

        [DefaultValue(true)]
        [Label("$Mods.BedFix.Configs.Label.FallAsleepImmediately")]
        [Tooltip("$Mods.BedFix.Configs.Tooltip.FallAsleepImmediately")]
        public bool FallAsleepImmediately { get; set; }

        [DefaultValue(false)]
        [Label("$Mods.BedFix.Configs.Label.UsingItemStopsSleeping")]
        [Tooltip("$Mods.BedFix.Configs.Tooltip.UsingItemStopsSleeping")]
        public bool UsingItemStopsSleeping { get; set; }

        [DefaultValue(true)]
        [Label("$Mods.BedFix.Configs.Label.SleepTogether")]
        [Tooltip("$Mods.BedFix.Configs.Tooltip.SleepTogether")]
        public bool SleepTogether { get; set; }

        [DefaultValue(true)]
        [Label("$Mods.BedFix.Configs.Label.SleepOnChair")]
        [Tooltip("$Mods.BedFix.Configs.Tooltip.SleepOnChair")]
        public bool SleepOnChair { get; set; }
    }
}
