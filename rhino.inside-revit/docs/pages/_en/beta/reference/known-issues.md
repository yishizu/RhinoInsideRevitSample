---
title: Known Issues
order: 43
group: Deployment & Configs
---

This guide looks at errors that can appear with {{ site.terms.rir }}. This address most of the common errors we have seen. [Please Contact Us](https://www.rhino3d.com/support) whether any of these options worked or did not work. We are working to minimize any of these messages.

## Unsupported openNURBS

### Problem

When {{ site.terms.rir }} attempts to load, the error below appears.

![]({{ "/static/images/reference/known-issues-unsupported-opennurbs.jpg" | prepend: site.baseurl }}){: class="small-image"}

This normally appears when there is a conflict when an older version of the Rhino file reader (openNURBS) has been loaded in Revit.  This normally happens because:

1. A Rhino 3DM file was inserted into Revit before {{ site.terms.rir }} was loaded. Revit is shipped with a different version of the openNURBS module, and loading a Rhino model into the Revit document before activating {{ site.terms.rir }}, cause the conflict
2. Other third-party Revit plugins that have loaded already, reference the Rhino file reader (openNURBS)

### Workaround

Please follow the instructions on [Submitting Debug Info]({{ site.baseurl }}{% link _en/beta/reference/toubleshooting.md %}#submitting-debug-info) to submit the error and debug information to {{ site.terms.rir }} development team.

Saving the project, then restarting Revit is usually the fastest workaround. If {{ site.terms.rir }} is loaded first, then everything should work with no issues.

Some plugins may need to be updated.  Common conflicts are seen with older versions of:
1. [Conveyer](https://provingground.io/tools/conveyor/)
2. [Avail](https://getavail.com/avail-adds-integration-with-mcneel-rhino-modeler/)
3. [{{ site.terms.pyrevit }} ](https://www.notion.so/pyRevit-bd907d6292ed4ce997c46e84b6ef67a0) 

We continue to work with all our partners on this error. Information gathered from the Error Reporting enables us to actively target these conflicts.


## Initialization Error -200

### Problem

When {{ site.terms.rir }} loads, the error below appears.

![]({{ "/static/images/reference/known-issues-error-200.png" | prepend: site.baseurl }})

### Workaround

This normally appears when there is a conflict between Rhino.inside and one or more Revit plugins that have loaded already. 

A common conflict is an older version of the {{ site.terms.pyrevit }} plugin.  While the newer versions to {{ site.terms.pyrevit }} do not cause a problem, an older version might.  Information on the {{ site.terms.pyrevit }} site can be found [{{ site.terms.pyrevit }} issue #628](https://github.com/eirannejad/pyRevit/issues/628). To update the older version of {{ site.terms.pyrevit }} use these steps:

  - Download [Microsoft.WindowsAPICodePack.Shell](https://www.nuget.org/packages/Microsoft.WindowsAPICodePack.Shell/) and place under `bin/` directory in pyRevit installation directory. This fix will be shipped with the next pyRevit version

  - DLL is also uploaded here for convenience if you don't know how to download NuGet packages. It's placed inside a ZIP archive for security. Unpack and place under `bin/` directory in pyRevit installation directory. [Microsoft.WindowsAPICodePack.Shell.dll.zip](https://github.com/eirannejad/pyRevit/files/3503717/Microsoft.WindowsAPICodePack.Shell.dll.zip)

If this does not solve the problem, then using the [Search for Conflicting Plugins]({{ site.baseurl }}{% link _en/beta/reference/toubleshooting.md %}#search-for-conflicting-plugins) section.

## JSON Error

### Problem

A Long JSON error shows up as shown below

![]({{ "/static/images/reference/known-issues-error-json.png" | prepend: site.baseurl }})

### Workaround

Like the previous -200 error, this is a conflict with another plugin. See the Error - 200 solution for this problem, and the [Search for Conflicting Plugins]({{ site.baseurl }}{% link _en/beta/reference/toubleshooting.md %}#search-for-conflicting-plugins) section below.

