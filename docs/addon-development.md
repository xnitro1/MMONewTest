# Creating Addons for NightBlade

## Contributing Addons (Easy Way)

**NEW**
[Addon Packager](https://github.com/denariigames/nightblade-addons/tree/master/AddonPackager) is a custom Unity Editor tool designed for NightBlade MMO community contributions. It simplifies the process of packaging your addons by:

- Exporting your addon folder as a .unitypackage with the required guid file
- Automatically generating a properly formatted package.json manifest

This makes it incredibly easy to share your addons with the NightBlade community. You can install Addon Packager directly from Addon Manager.

Latest instructions are available on [NightBlade Addon Packager](https://github.com/denariigames/nightblade-addons/tree/master/AddonPackager).

## Contributing Addons (Hard Way)

To prepare your addon:

- Bundle your assets as a .unitypackage with a guid file (*do not include the Assets folder!*)
  - The guid file should have a filename unique to the project. We recommend [GuidGenerator](https://guidgenerator.com/) to create one.
  -  `touch your-guid-id` at the command line so there is no extension (Windows equivalent: `echo.> your-guid-id`).
- Add a NightBlade compatible package.json with required fields (see below)
- Host on a public github repository
- Share your raw package.json URL on Discord
  - The raw URL links directly to the package.json content and should look like this: `https://raw.githubusercontent.com/<owner>/<repo>/refs/heads/main/package.json`

### NightBlade Compatible package.json

Each addon repository must have a NightBlade compatible package.json with these required fields:

- name (displayed in Addon Manager and used for addon folder name)
- guid (must be unique to the project and match guid filename in the unitypackage)
- author (format: string or object {"name": "yourname", "url": "yoururl"})
- version
- updateDate (format: "YYYY-MM-DD")
- packageUrl (link to the unitypackage, you can use either a release package or raw/refs/head)

Optional, but recommended:

- category (must match one of the approved categories below)
- description (supports \n for line breaks)
- screenshot (filename of screenshot file in repo at same root as package.json, e.g. screenshot.png)

Optional:

- patchFile (filename of patch file in repo at same root as package.json, e.g. myaddon.patch)
- dependencies (array of addon guids, e.g. ["xxx", "yyy"])

A valid example package.json is [available here](https://github.com/denariigames/nightblade-addon-manager/blob/master/example-package.json).

### Approved Categories

Only these categories appear in the filter dropdown:

- Demo
- Characters
- Monsters
- NPCs
- Combat
- Economy
- Items
- Gameplay
- UI
- MMO
- Integration
- Tools

Any other category value will be treated as "Uncategorized" and hidden from filters.

---

*Creating addons for NightBlade is not just about extending the framework—it's about contributing to a thriving ecosystem of game development tools and content.*

