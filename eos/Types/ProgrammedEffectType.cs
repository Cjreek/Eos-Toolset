using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum ProgrammedEffectType
    {
        [IgnoreEnumValue]
        Invalid = 0,
        [DisplayName("Skin Overlay")]
        SkinOverlay = 1,
        [DisplayName("Environment Mapping")]
        EnvironmentMapping = 2,
        [DisplayName("Glow Effect")]
        GlowEffect = 3,
        [DisplayName("Add/Remove Lighting")]
        Lighting = 4,
        [DisplayName("Alpha Transparency")]
        AlphaTransparency = 5,
        [DisplayName("Pulsing Aura")]
        PulsingAura = 6,
        [DisplayName("Beam")]
        Beam = 7,
        [DisplayName("Stop Model Rendering")]
        StopModelRendering = 8,
        [DisplayName("Chunk Model")]
        ChunkModel = 9,
        [DisplayName("MIRV Projectile")]
        MIRV = 10,
        [DisplayName("Variant MIRV Projectile")]
        VariantMIRV = 11,
        [DisplayName("Spellcast Failure")]
        SpellCastFailure = 12,
        [DisplayName("Freeze Animation")]
        FreezeAnimation = 13,
    }
}
