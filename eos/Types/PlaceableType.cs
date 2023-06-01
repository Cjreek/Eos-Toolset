using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum PlaceableType
    {
        Battlefield,
        [DisplayName("Containers & Switches")]
        Containers,
        Military,
        [DisplayName("Miscellaneous")]
        Misc,
        [DisplayName("Miscellaneous Interior")]
        MiscInterior,
        [DisplayName("Parks & Nature")]
        ParksNature,
        [DisplayName("Penants & Signs")]
        PenantsSigns,
        [DisplayName("Trades & Academic & Farm")]
        TradesAcademic,
        [DisplayName("Visual Effects")]
        VisualEffects
    }
}
