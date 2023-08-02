using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace BedFix
{
    public class Configuration : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        public override void OnLoaded() => BedSystem.Config = this;

        [DefaultValue(false)]
        public bool ValidHouseRequirement { get; set; }

        [DefaultValue(true)]
        public bool FallAsleepAlways { get; set; }

        [DefaultValue(true)]
        public bool FallAsleepImmediately { get; set; }

        [DefaultValue(false)]
        public bool UsingItemStopsSleeping { get; set; }

        [DefaultValue(true)]
        public bool SleepTogether { get; set; }

        [DefaultValue(true)]
        public bool SleepOnChair { get; set; }
    }
}
