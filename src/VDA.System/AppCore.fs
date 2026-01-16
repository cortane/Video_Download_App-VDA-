module VDA.System.AppCore

open System
open System.IO
open System.Reflection
open System.Runtime.InteropServices

[<DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
extern bool SetDllDirectory(string lpPathName)

let setDllDirectory (path: string) =
    SetDllDirectory(path)

/// 埋め込みリソースを一時フォルダに展開する
let extractEmbeddedResources () =
    // 展開先ディレクトリ (Temp/VDA_Tools)
    // 毎回最新を使うために、バージョン番号などをパスに含めると良いが、
    // ここではシンプルに Temp/VDA_Tools に展開し、存在確認をする
    let tempPath = Path.Combine(Path.GetTempPath(), "VDA_Tools")
    if not (Directory.Exists(tempPath)) then
        Directory.CreateDirectory(tempPath) |> ignore

    let assembly = Assembly.GetExecutingAssembly()
    let resourceNames = assembly.GetManifestResourceNames()

    let extract (resourceName: string) (fileName: string) =
        let destPath = Path.Combine(tempPath, fileName)
        // 簡易チェック: ファイルがなければ展開。更新が必要な場合はサイズ比較などを入れる。
        // ここでは毎回上書きする (開発中はEXEが変わる可能性があるため)
        // 実運用では「存在すればスキップ」の方が起動が早いが、
        // 埋め込みリソースが更新された場合に反映されないリスクがあるため上書き推奨、あるいはハッシュチェック
        try
            use stream = assembly.GetManifestResourceStream(resourceName)
            if stream <> null then
                use fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write)
                stream.CopyTo(fileStream)
        with
        | ex -> printfn "リソース展開エラー: %s %s" resourceName ex.Message
    
    // リソース名は "名前空間.フォルダ.ファイル名" のようになる
    // VDA.System... ではなく、プロジェクトのルート名前空間 + ディレクトリ構造
    // リソース名を確認してマッチングさせるのが確実
    
    // 特定のファイル郡を展開
    // yt-dlp.exe, ffmpeg.exe, ffprobe.exe, vda_core.dll
    
    for name in resourceNames do
        if name.EndsWith("yt-dlp.exe") then extract name "yt-dlp.exe"
        elif name.EndsWith("ffmpeg.exe") then extract name "ffmpeg.exe"
        elif name.EndsWith("ffprobe.exe") then extract name "ffprobe.exe"
        elif name.EndsWith("deno.exe") then extract name "deno.exe"
        elif name.EndsWith("vda_core.dll") then 
            // DLLは実行ディレクトリにも必要かもしれないが、LoadLibraryで読み込むならパス指定が必要
            // 今回は DllImport("vda_core.dll") なので、
            // 1. PATHにこのTempフォルダを追加する
            // 2. SetDllDirectory API を呼ぶ
            // 3. 実行ファイルの横に置く
            // のいずれかが必要。
            // 一番確実なのは、実行ファイルの横に置くことだが、それだと「配布ファイルが増える」ように見える（実行時に生成されるだけだが）。
            // ここでは Temp に展開し、SetDllDirectory でそのパスを参照させるアプローチをとる。
            extract name "vda_core.dll"

    tempPath

