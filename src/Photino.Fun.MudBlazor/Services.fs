[<AutoOpen>]
module Photino.Fun.MudBlazor.Services

open System.Diagnostics
open System.Runtime.InteropServices
open Microsoft.Extensions.Logging

type Platform =
    | Windows
    | Linux
    | MacOS
    | Unknown

type IPlatformService =
    abstract member GetPlatform: unit -> Platform

type PlatformService() =
    interface IPlatformService with
        member _.GetPlatform() =
            if RuntimeInformation.IsOSPlatform OSPlatform.Windows then
                Windows
            elif RuntimeInformation.IsOSPlatform OSPlatform.Linux then
                Linux
            elif RuntimeInformation.IsOSPlatform OSPlatform.OSX then
                MacOS
            else
                Unknown

type IProcessService =
    abstract member Run: command: string * arguments: string -> unit

type ProcessService(logger: ILogger<IProcessService>) =
    interface IProcessService with
        member _.Run(command, arguments) =
            try
                let psi = new ProcessStartInfo(command)
                psi.RedirectStandardOutput <- false
                psi.UseShellExecute <- true
                psi.CreateNoWindow <- true
                psi.Arguments <- arguments

                use p = new Process()
                p.StartInfo <- psi
                p.Start() |> ignore
                p.Dispose()
            with ex ->
                logger.LogError(ex, $"Error running process: {command} {arguments}")

type ILinkOpeningService =
    abstract member OpenUrl: url: string -> unit

type LinkOpeningService
    (platformService: IPlatformService, processService: IProcessService, logger: ILogger<LinkOpeningService>) =
    interface ILinkOpeningService with
        member _.OpenUrl url =
            try
                match platformService.GetPlatform() with
                | Windows -> processService.Run("cmd", $"/c start \"\" \"{url}\"")
                | Linux -> processService.Run("xdg-open", url)
                | MacOS -> processService.Run("open", url)
                | _ -> ()
            with ex ->
                Debug.WriteLine ex
                logger.LogError(ex, "Error while opening next url = {url}")

type SharedResources() = class end
