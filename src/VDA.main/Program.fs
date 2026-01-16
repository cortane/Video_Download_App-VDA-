module Main

open System
open System.Windows

[<STAThread>]
[<EntryPoint>]
let main argv =
    // リソースを展開
    try
        let toolsPath = VDA.System.AppCore.extractEmbeddedResources()
        // DLLの検索パスを追加 (vda_core.dll のため)
        VDA.System.AppCore.setDllDirectory(toolsPath) |> ignore
        
        let app = Application()
        app.Run(new VDA.UI.MainWindow.MainWindow(toolsPath)) |> ignore
        0
    with
    | ex ->
        MessageBox.Show(sprintf "起動エラー: %s" ex.Message) |> ignore
        -1
