using System.Collections.Generic;
using SS14.Launcher.Localization;
using static SS14.Launcher.Api.ServerApi;

namespace SS14.Launcher.ViewModels.MainWindowTabs;

public sealed partial class ServerListFiltersViewModel
{
    private static readonly Dictionary<string, string> RegionNamesEnglish = new()
    {
        // @formatter:off
        { Tags.RegionAfricaCentral,       Loc.GetParticularString("Region - Long Name", "Africa Central")        },
        { Tags.RegionAfricaNorth,         Loc.GetParticularString("Region - Long Name", "Africa North")          },
        { Tags.RegionAfricaSouth,         Loc.GetParticularString("Region - Long Name", "Africa South")          },
        { Tags.RegionAntarctica,          Loc.GetParticularString("Region - Long Name", "Antarctica")            },
        { Tags.RegionAsiaEast,            Loc.GetParticularString("Region - Long Name", "Asia East")             },
        { Tags.RegionAsiaNorth,           Loc.GetParticularString("Region - Long Name", "Asia North")            },
        { Tags.RegionAsiaSouthEast,       Loc.GetParticularString("Region - Long Name", "Asia South East")       },
        { Tags.RegionCentralAmerica,      Loc.GetParticularString("Region - Long Name", "Central America")       },
        { Tags.RegionEuropeEast,          Loc.GetParticularString("Region - Long Name", "Europe East")           },
        { Tags.RegionEuropeWest,          Loc.GetParticularString("Region - Long Name", "Europe West")           },
        { Tags.RegionGreenland,           Loc.GetParticularString("Region - Long Name", "Greenland")             },
        { Tags.RegionIndia,               Loc.GetParticularString("Region - Long Name", "India")                 },
        { Tags.RegionMiddleEast,          Loc.GetParticularString("Region - Long Name", "Middle East")           },
        { Tags.RegionMoon,                Loc.GetParticularString("Region - Long Name", "The Moon")              },
        { Tags.RegionNorthAmericaCentral, Loc.GetParticularString("Region - Long Name", "North America Central") },
        { Tags.RegionNorthAmericaEast,    Loc.GetParticularString("Region - Long Name", "North America East")    },
        { Tags.RegionNorthAmericaWest,    Loc.GetParticularString("Region - Long Name", "North America West")    },
        { Tags.RegionOceania,             Loc.GetParticularString("Region - Long Name", "Oceania")               },
        { Tags.RegionSouthAmericaEast,    Loc.GetParticularString("Region - Long Name", "South America East")    },
        { Tags.RegionSouthAmericaSouth,   Loc.GetParticularString("Region - Long Name", "South America South")   },
        { Tags.RegionSouthAmericaWest,    Loc.GetParticularString("Region - Long Name", "South America West")    },
        // @formatter:on
    };

    private static readonly Dictionary<string, string> RegionNamesShortEnglish = new()
    {
        // @formatter:off
        { Tags.RegionAfricaCentral,       Loc.GetParticularString("Region - Short Name", "Africa Central")  },
        { Tags.RegionAfricaNorth,         Loc.GetParticularString("Region - Short Name", "Africa North")    },
        { Tags.RegionAfricaSouth,         Loc.GetParticularString("Region - Short Name", "Africa South")    },
        { Tags.RegionAntarctica,          Loc.GetParticularString("Region - Short Name", "Antarctica")      },
        { Tags.RegionAsiaEast,            Loc.GetParticularString("Region - Short Name", "Asia East")       },
        { Tags.RegionAsiaNorth,           Loc.GetParticularString("Region - Short Name", "Asia North")      },
        { Tags.RegionAsiaSouthEast,       Loc.GetParticularString("Region - Short Name", "Asia South East") },
        { Tags.RegionCentralAmerica,      Loc.GetParticularString("Region - Short Name", "Central America") },
        { Tags.RegionEuropeEast,          Loc.GetParticularString("Region - Short Name", "Europe East")     },
        { Tags.RegionEuropeWest,          Loc.GetParticularString("Region - Short Name", "Europe West")     },
        { Tags.RegionGreenland,           Loc.GetParticularString("Region - Short Name", "Greenland")       },
        { Tags.RegionIndia,               Loc.GetParticularString("Region - Short Name", "India")           },
        { Tags.RegionMiddleEast,          Loc.GetParticularString("Region - Short Name", "Middle East")     },
        { Tags.RegionMoon,                Loc.GetParticularString("Region - Short Name", "The Moon")        },
        { Tags.RegionNorthAmericaCentral, Loc.GetParticularString("Region - Short Name", "NA Central")      },
        { Tags.RegionNorthAmericaEast,    Loc.GetParticularString("Region - Short Name", "NA East")         },
        { Tags.RegionNorthAmericaWest,    Loc.GetParticularString("Region - Short Name", "NA West")         },
        { Tags.RegionOceania,             Loc.GetParticularString("Region - Short Name", "Oceania")         },
        { Tags.RegionSouthAmericaEast,    Loc.GetParticularString("Region - Short Name", "SA East")         },
        { Tags.RegionSouthAmericaSouth,   Loc.GetParticularString("Region - Short Name", "SA South")        },
        { Tags.RegionSouthAmericaWest,    Loc.GetParticularString("Region - Short Name", "SA West")         },
        // @formatter:on
    };

    private static readonly Dictionary<string, string> RolePlayNames = new()
    {
        // @formatter:off
        { Tags.RolePlayNone,   Loc.GetParticularString("Roleplay Level Filter", "None")   },
        { Tags.RolePlayLow,    Loc.GetParticularString("Roleplay Level Filter", "Low")    },
        { Tags.RolePlayMedium, Loc.GetParticularString("Roleplay Level Filter", "Medium") },
        { Tags.RolePlayHigh,   Loc.GetParticularString("Roleplay Level Filter", "High")   },
        // @formatter:on
    };

    private static readonly Dictionary<string, int> RolePlaySortOrder = new()
    {
        // @formatter:off
        { Tags.RolePlayNone,   0 },
        { Tags.RolePlayLow,    1 },
        { Tags.RolePlayMedium, 2 },
        { Tags.RolePlayHigh,   3 },
        // @formatter:on
    };
}
