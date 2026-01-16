module VDA.UI.Program

open System
open System.IO
open System.Windows
open VDA.UI.MainWindow

open VDA.System

[<STAThread>]
[<EntryPoint>]
let main argv =
    // ここで埋め込みリソース (yt-dlp, ffmpeg, dll, qjs) をTempフォルダに展開
    let toolsPath = AppCore.extractEmbeddedResources()
    
    // SetDllDirectory で vda_core.dll のあるパスを検索パスに追加 (Native.fsのDllImport解決用)
    Native.SetDllDirectory(toolsPath) |> ignore

    let app = Application()
    // 展開されたパスを渡す
    // let toolsPath = AppDomain.CurrentDomain.BaseDirectory
    
    let window = new MainWindow(toolsPath)
    app.Run(window) // 終了コードを返す

