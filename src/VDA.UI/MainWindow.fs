namespace VDA.UI.MainWindow

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Runtime.InteropServices
open System.Threading.Tasks

module NativeMethods =
    [<DllImport("vda_core.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)>]
    extern IntPtr download_video(
        string url, 
        string path, 
        string format, 
        string exec_path,
        string ffmpeg_path,
        string qjs_path,
        string cookie_path
    )

    [<DllImport("vda_core.dll", CallingConvention = CallingConvention.Cdecl)>]
    extern void free_string(IntPtr s)

type MainWindow(toolsPath: string) as this =
    inherit Window()

    let mutable outputBox : TextBox = null
    let mutable downloadButton : Button = null
    let mutable urlBox : TextBox = null
    let mutable formatBox : ComboBox = null

    // Tools paths
    let ytDlpPath = System.IO.Path.Combine(toolsPath, "yt-dlp.exe")
    let ffmpegPath = System.IO.Path.Combine(toolsPath, "ffmpeg.exe") 
    let denoPath = System.IO.Path.Combine(toolsPath, "deno.exe")
    let cookiePath = "" 

    let log (msg: string) =
        if outputBox <> null then
            outputBox.Dispatcher.Invoke(fun () ->
                outputBox.AppendText(msg + Environment.NewLine)
                outputBox.ScrollToEnd()
            )

    let downloadVideo (url: string) (format: string) =
        async {
            log (sprintf "Starting download: %s [%s]" url format)
            log (sprintf "Tools Path: %s" toolsPath)
            
            // Disable button
            downloadButton.Dispatcher.Invoke(fun () -> downloadButton.IsEnabled <- false)

            let r = 
                Task.Run(fun () ->
                    try
                        log "Calling vda_core..."
                        let resultPtr = NativeMethods.download_video(url, ".", format, ytDlpPath, ffmpegPath, denoPath, cookiePath)
                        if resultPtr = IntPtr.Zero then
                            "Error: Null result from Core"
                        else
                            let result = Marshal.PtrToStringAnsi(resultPtr)
                            NativeMethods.free_string(resultPtr)
                            result
                    with
                    | ex -> sprintf "Exception calling DLL: %s" ex.Message
                ) |> Async.AwaitTask
            
            let! result = r
            log "---------------------------------------------------"
            log result
            log "---------------------------------------------------"
            
            // Enable button
            downloadButton.Dispatcher.Invoke(fun () -> downloadButton.IsEnabled <- true)
        } |> Async.StartImmediate


    let init () =
        this.Title <- "Video Download App (VDA) - Deno Powered"
        this.Width <- 700.0
        this.Height <- 500.0
        
        let grid = Grid()
        grid.RowDefinitions.Add(RowDefinition(Height = GridLength.Auto)) // Setup
        grid.RowDefinitions.Add(RowDefinition(Height = GridLength.Auto)) // URL
        grid.RowDefinitions.Add(RowDefinition(Height = GridLength.Auto)) // Format & Button
        // FIX: GridLength.Star doesn't exist as static member. Use constructor.
        grid.RowDefinitions.Add(RowDefinition(Height = GridLength(1.0, GridUnitType.Star))) // Log

        // Row 0: Info
        let infoLabel = Label(Content = "Engine: Rust + yt-dlp + Deno (Fixed)")
        Grid.SetRow(infoLabel, 0)
        grid.Children.Add(infoLabel) |> ignore

        // Row 1: URL
        let urlPanel = DockPanel(Margin = Thickness(5.0))
        urlPanel.Children.Add(Label(Content = "URL:")) |> ignore
        urlBox <- TextBox(Text = "https://www.youtube.com/watch?v=dQw4w9WgXcQ")
        urlPanel.Children.Add(urlBox) |> ignore
        Grid.SetRow(urlPanel, 1)
        grid.Children.Add(urlPanel) |> ignore

        // Row 2: Controls
        let controlPanel = StackPanel(Orientation = Orientation.Horizontal, Margin = Thickness(5.0))
        
        controlPanel.Children.Add(Label(Content = "Format:")) |> ignore
        formatBox <- ComboBox(Width = 100.0)
        formatBox.Items.Add("mp4") |> ignore
        formatBox.Items.Add("mp3") |> ignore
        formatBox.Items.Add("best") |> ignore
        formatBox.SelectedIndex <- 0
        controlPanel.Children.Add(formatBox) |> ignore
        
        downloadButton <- Button(Content = "Download", Width = 100.0, Margin = Thickness(10.0, 0.0, 0.0, 0.0))
        downloadButton.Click.Add(fun _ -> 
            let url = urlBox.Text
            let fmt = formatBox.SelectedItem.ToString()
            downloadVideo url fmt
        )
        controlPanel.Children.Add(downloadButton) |> ignore

        Grid.SetRow(controlPanel, 2)
        grid.Children.Add(controlPanel) |> ignore

        // Row 3: Output Logger
        outputBox <- TextBox(IsReadOnly = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, AcceptsReturn = true, FontFamily = FontFamily("Consolas"))
        Grid.SetRow(outputBox, 3)
        grid.Children.Add(outputBox) |> ignore

        this.Content <- grid

    do init()
