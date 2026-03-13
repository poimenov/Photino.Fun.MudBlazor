[<AutoOpen>]
module Photino.Fun.MudBlazor.App

open System
open System.Linq
open Microsoft.AspNetCore.Components.Web
open Microsoft.AspNetCore.Components.Routing
open MudBlazor
open Microsoft.Extensions.Localization
open FSharp.Data
open Fun.Blazor
open Fun.Blazor.Router

type WeatherForecastProvider = JsonProvider<""" [{"date": "2018-05-06", "temperatureC": 1, "summary": "Freezing" }] """>

type IShareStore with
    member store.Count = store.CreateCVal(nameof store.Count, 0)
    member store.DrawerOpen = store.CreateCVal(nameof store.DrawerOpen, true)
    member store.IsDarkMode = store.CreateCVal(nameof store.IsDarkMode, true)

    member store.WeatherData =
        store.CreateCVal(nameof store.WeatherData, Array.Empty<WeatherForecastProvider.Root>())

let homePage =
    html.inject (fun (linkOpeningService: ILinkOpeningService, localizer: IStringLocalizer<SharedResources>) ->
        fragment {
            SectionContent'' {
                SectionName "Title"
                localizer["Home"]
            }

            MudText'' {
                Typo Typo.body1
                class' "mb-8"
                localizer["HomeText"]
            }

            MudAlert'' {
                localizer["Links"]

                MudLink'' {
                    Typo Typo.body2
                    Color Color.Primary

                    onclick (fun _ ->
                        linkOpeningService.OpenUrl "https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor")

                    b { "Blazor" }
                }

                ", "

                MudLink'' {
                    Typo Typo.body2
                    Color Color.Primary

                    onclick (fun _ -> linkOpeningService.OpenUrl "https://mudblazor.com")

                    b { "MudBlazor" }
                }

                ", "

                MudLink'' {
                    Typo Typo.body2
                    Color Color.Primary

                    onclick (fun _ -> linkOpeningService.OpenUrl "https://github.com/tryphotino/photino.Blazor")

                    b { "Photino.Blazor" }
                }

                ", "

                MudLink'' {
                    Typo Typo.body2
                    Color Color.Primary

                    onclick (fun _ -> linkOpeningService.OpenUrl "https://slaveoftime.github.io/Fun.Blazor.Docs")

                    b { "Fun.Blazor" }
                }

                ", "

                MudLink'' {
                    Typo Typo.body2
                    Color Color.Primary

                    onclick (fun _ -> linkOpeningService.OpenUrl "https://dotnet.microsoft.com/download/dotnet/10.0")

                    b { ".NET 10" }
                }
            }
        })

let fetchDataPage =
    html.inject (fun (store: IShareStore, hook: IComponentHook, localizer: IStringLocalizer<SharedResources>) ->
        hook.AddInitializedTask(fun () ->
            task {
                if not (store.WeatherData.Value.Any()) then
                    //imitation of waiting for a response on the first call
                    let! t = Async.Sleep 2000
                    let! data = WeatherForecastProvider.AsyncLoad "wwwroot/sample-data/weather.json"
                    store.WeatherData.Publish data
            })

        fragment {
            SectionContent'' {
                SectionName "Title"
                localizer["WeatherHeader"]
            }

            MudText'' {
                Typo Typo.body1
                class' "mb-8"
                localizer["WeatherText"]
            }

            adapt {
                let! items = store.WeatherData

                if not (store.WeatherData.Value.Any()) then
                    MudProgressCircular'' {
                        Color Color.Default
                        Indeterminate true

                    }
                else
                    MudTable'' {
                        Items items
                        Hover true

                        HeaderContent(
                            fragment {
                                MudTh'' { localizer["Date"] }
                                MudTh'' { localizer["TempC"] }
                                MudTh'' { localizer["TempF"] }
                                MudTh'' { localizer["Summary"] }
                            }
                        )

                        RowTemplate(fun x ->
                            fragment {
                                MudTd'' { x.Date.ToString "dd MMM yyyy" }
                                MudTd'' { x.TemperatureC }
                                MudTd'' { Math.Round(float x.TemperatureC * (9.0 / 5.0) + 32.0, 2) }
                                MudTd'' { x.Summary }
                            })

                        PagerContent(MudTablePager'' { PageSizeOptions [| 10 |] })
                    }
            }
        })

let counterPage =
    html.inject (fun (store: IShareStore, snackbar: ISnackbar, localizer: IStringLocalizer<SharedResources>) ->
        fragment {
            SectionContent'' {
                SectionName "Title"
                localizer["Counter"]
            }

            adapt {
                let! count = store.Count

                MudText'' {
                    Typo Typo.body1
                    class' "mb-8"
                    localizer["CurrentCount"]
                    count
                }
            }

            MudButton'' {
                Color Color.Primary
                Variant Variant.Filled

                onclick (fun _ ->
                    store.Count.Publish((+) 1)
                    let currCount = string localizer["CurrentCount"]
                    snackbar.Add($"{currCount} {store.Count.Value}", Severity.Success) |> ignore)

                localizer["ClickMe"]
            }
        })

let appHeader =
    html.inject (fun (store: IShareStore) ->
        MudAppBar'' {
            Elevation 1

            MudIconButton'' {
                Icon Icons.Material.Filled.Menu
                Color Color.Inherit
                Edge Edge.Start
                onclick (fun _ -> store.DrawerOpen.Publish not)
            }

            MudText'' {
                Typo Typo.h5
                class' "ml-3"
                AppSettings.ApplicationName
            }

            MudSpacer''

            adapt {
                let! isDarkMode = store.IsDarkMode

                let darkLightModeButtonIcon =
                    if isDarkMode then
                        Icons.Material.Rounded.AutoMode
                    else
                        Icons.Material.Outlined.DarkMode

                MudIconButton'' {
                    Icon darkLightModeButtonIcon
                    Color Color.Inherit
                    onclick (fun _ -> store.IsDarkMode.Publish(not store.IsDarkMode.Value))
                }

                MudIconButton'' {
                    Icon Icons.Material.Filled.MoreVert
                    Color Color.Inherit
                    Edge Edge.End
                }
            }
        })


let navmenus =
    html.injectWithNoKey (fun (store: IShareStore, localizer: IStringLocalizer<SharedResources>) ->
        adapt {
            let! drawerOpen = store.DrawerOpen.WithSetter()

            MudDrawer'' {
                Open' drawerOpen
                Elevation 2
                ClipMode DrawerClipMode.Always

                MudNavMenu'' {

                    MudNavLink'' {
                        Href "/"
                        Match NavLinkMatch.All
                        Icon Icons.Material.Filled.Home
                        localizer["Home"]
                    }

                    MudNavLink'' {
                        Href "/counter"
                        Match NavLinkMatch.Prefix
                        Icon Icons.Material.Filled.Add
                        localizer["Counter"]
                    }

                    MudNavLink'' {
                        Href "/fetchdata"
                        Match NavLinkMatch.Prefix
                        Icon Icons.Material.Filled.List
                        localizer["Weather"]
                    }
                }
            }

        })

let routes =
    html.route
        [| routeCi "/counter" counterPage
           routeCi "/fetchdata" fetchDataPage
           routeAny homePage |]

let mudTheme =
    let theme = new MudTheme()
    theme.LayoutProperties <- new LayoutProperties()

    let paletteDark =
        let p = new PaletteDark()
        p.Primary <- "#7e6fff"
        p.Surface <- "#1e1e2d"
        p.Background <- "#1a1a27"
        p.BackgroundGray <- "#151521"
        p.AppbarText <- "#92929f"
        p.AppbarBackground <- "rgba(26,26,39,0.8)"
        p.DrawerBackground <- "#1a1a27"
        p.ActionDefault <- "#74718e"
        p.ActionDisabled <- "#9999994d"
        p.ActionDisabledBackground <- "#605f6d4d"
        p.TextPrimary <- "#b2b0bf"
        p.TextSecondary <- "#92929f"
        p.TextDisabled <- "#ffffff33"
        p.DrawerIcon <- "#92929f"
        p.DrawerText <- "#92929f"
        p.GrayLight <- "#2a2833"
        p.GrayLighter <- "#1e1e2d"
        p.Info <- "#4a86ff"
        p.Success <- "#3dcb6c"
        p.Warning <- "#ffb545"
        p.Error <- "#ff3f5f"
        p.LinesDefault <- "#33323e"
        p.TableLines <- "#33323e"
        p.Divider <- "#292838"
        p.OverlayLight <- "#1e1e2d80"
        p

    let paletteLight =
        let p = new PaletteLight()
        p.Black <- "#110e2d"
        p.AppbarText <- "#424242"
        p.AppbarBackground <- "rgba(255,255,255,0.8)"
        p.DrawerBackground <- "#ffffff"
        p.GrayLight <- "#e8e8e8"
        p.GrayLighter <- "#f9f9f9"
        p

    theme.PaletteDark <- paletteDark
    theme.PaletteLight <- paletteLight
    theme

let app =
    html.inject (fun (store: IShareStore, localizer: IStringLocalizer<SharedResources>) ->
        ErrorBoundary'' {
            ErrorContent(fun e ->
                MudAlert'' {
                    Severity Severity.Error
                    string e
                })

            adapt {
                let! isDarkMode = store.IsDarkMode

                MudThemeProvider'' {
                    Theme mudTheme
                    IsDarkMode isDarkMode
                }
            }

            MudSnackbarProvider''
            MudPopoverProvider''
            MudDialogProvider''

            MudLayout'' {
                appHeader
                navmenus

                MudMainContent'' {
                    MudText'' {
                        Typo Typo.h3
                        class' "pa-4"

                        SectionOutlet'' { SectionName "Title" }
                    }

                    MudContainer'' {
                        class' "pa-4"
                        routes
                    }
                }
            }
        })

type App() =
    inherit FunComponent()

    override _.Render() = app
