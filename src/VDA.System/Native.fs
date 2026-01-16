module VDA.System.Native

open System
open System.Runtime.InteropServices

// Rust DLL Import
// DLL名は環境によって libvda_core.dll か vda_core.dll になるが、Windowsでは vda_core.dll
// 文字コード問題を避けるため UTF-8 (LPUTF8Str) でマーシャリングする
[<DllImport("vda_core.dll", CallingConvention = CallingConvention.Cdecl)>]
extern IntPtr download_video(
    [<MarshalAs(UnmanagedType.LPUTF8Str)>] string url,
    [<MarshalAs(UnmanagedType.LPUTF8Str)>] string path,
    [<MarshalAs(UnmanagedType.LPUTF8Str)>] string format,
    [<MarshalAs(UnmanagedType.LPUTF8Str)>] string exec_path,
    [<MarshalAs(UnmanagedType.LPUTF8Str)>] string ffmpeg_path,
    [<MarshalAs(UnmanagedType.LPUTF8Str)>] string qjs_path,
    [<MarshalAs(UnmanagedType.LPUTF8Str)>] string cookie_path
)

[<DllImport("vda_core.dll", CallingConvention = CallingConvention.Cdecl)>]
extern void free_string(IntPtr s)

let downloadVideo (url: string) (path: string) (format: string) (execPath: string) (ffmpegPath: string) (qjsPath: string) (cookiePath: string) =
    let ptr = download_video(url, path, format, execPath, ffmpegPath, qjsPath, cookiePath)
    try
        // 返り値も UTF-8 として読み取る
        Marshal.PtrToStringUTF8(ptr)
    finally
        free_string(ptr)
