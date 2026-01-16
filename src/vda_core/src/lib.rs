use std::ffi::{CStr, CString};
use std::os::raw::c_char;
use std::os::windows::process::CommandExt;
use std::process::{Command, Stdio};
use std::io::{Read, BufRead, BufReader};
use std::fs::File;

#[no_mangle]
pub extern "C" fn download_video(
    url: *const c_char, 
    path: *const c_char, 
    format_ptr: *const c_char, 
    exec_path: *const c_char,
    ffmpeg_path: *const c_char,
    qjs_path: *const c_char,
    cookie_path: *const c_char
) -> *mut c_char {

    if url.is_null() || path.is_null() || exec_path.is_null() {
        return CString::new("Error: Null pointer").unwrap().into_raw();
    }

    let url_str = unsafe { CStr::from_ptr(url).to_string_lossy().into_owned() };
    let path_str = unsafe { CStr::from_ptr(path).to_string_lossy().into_owned() };
    let format_str = unsafe { CStr::from_ptr(format_ptr).to_string_lossy().into_owned() }; // e.g., "mp4", "mp3"
    let exec_path_str = unsafe { CStr::from_ptr(exec_path).to_string_lossy().into_owned() };
    
    let ffmpeg_path_str = if !ffmpeg_path.is_null() {
        unsafe { CStr::from_ptr(ffmpeg_path).to_string_lossy().into_owned() }
    } else {
        String::new()
    };

    let qjs_path_str = if !qjs_path.is_null() {
        unsafe { CStr::from_ptr(qjs_path).to_string_lossy().into_owned() }
    } else {
        String::new()
    };

    let cookie_path_str = if !cookie_path.is_null() {
        unsafe { CStr::from_ptr(cookie_path).to_string_lossy().into_owned() }
    } else {
        String::new()
    };


    // コマンド構築
    let mut args = vec![
        "--encoding".to_string(), "utf-8".to_string(), // 文字化け対策: UTF-8出力を強制
        "--newline".to_string(), // ログ解析用に改行を強制
        "--force-overwrites".to_string(), // ファイル重複時は上書き
        "--no-check-certificates".to_string(), // 証明書エラー無視
        "--no-mtime".to_string(), // 更新日時を変更しない
        "--no-playlist".to_string(), // プレイリストでも単体動画のみダウンロード
        "-P".to_string(),
        path_str.clone(),
        url_str.clone()
    ];

    // フォーマット判定
    match format_str.as_str() {
        "mp3" | "m4a" | "wav" | "flac" | "aac" | "opus" => {
            args.push("-f".to_string());
            args.push(format!("bestaudio[ext={}]/bestaudio", format_str));

            args.push("--extract-audio".to_string());
            args.push("--audio-format".to_string());
            args.push(format_str.clone());
        },
        _ => {
            args.push("--format".to_string());
            args.push("bestvideo+bestaudio/best".to_string());
            args.push("--merge-output-format".to_string());
            args.push(format_str.clone());
        }
    }

    // ffmpeg設定
    if !ffmpeg_path_str.is_empty() {
        args.push("--ffmpeg-location".to_string());
        args.push(ffmpeg_path_str);
    }

    // JS Runtime (Deno)
    if !qjs_path_str.is_empty() {
        args.push("--js-runtimes".to_string());
        args.push(format!("deno:{}", qjs_path_str)); 
    }
    
    if !cookie_path_str.is_empty() {
        args.push("--cookies".to_string());
        args.push(cookie_path_str);
    }

    // ログファイルを作成 (カレントディレクトリ)
    let log_file_path = "vda_debug.log";
    let log_file = match File::create(log_file_path) {
        Ok(f) => f,
        Err(e) => return CString::new(format!("Error: Cannot create log file: {}", e)).unwrap().into_raw()
    };
    
    // Stderr用にもう一つ開く
    let log_file_err = match File::options().append(true).open(log_file_path) {
        Ok(f) => f,
        Err(_) => log_file.try_clone().unwrap() // エラーならcloneで代用
    };

    // 実行
    match Command::new(exec_path_str)
        .args(&args)
        .stdin(Stdio::null())
        .stdout(Stdio::from(log_file)) // ファイルへ直接出力
        .stderr(Stdio::from(log_file_err)) // ファイルへ直接出力
        .creation_flags(0x08000000) // CREATE_NO_WINDOW
        .spawn() 
    {
        Ok(mut child) => {
            // プロセス終了を待機
            let _ = child.wait();
            
            // ログファイルを読み直す
            let mut log_content = String::new();
             if let Ok(mut f) = File::open(log_file_path) {
                let _ = f.read_to_string(&mut log_content);
            }

            // 成功判定 (簡易)
            let is_success = log_content.contains("100%"); // yt-dlp usually prints percent
             
             let status_prefix = if is_success { "Success" } else { "Finished" }; // Finished means maybe success, checking log required

             let success_msg = format!("{} (Check File)\n---LOG (File)---\n{}", status_prefix, log_content);
             CString::new(success_msg).unwrap().into_raw()
        },
        Err(e) => {
            let err_msg = format!("Error: Failed to execute process: {}", e);
            CString::new(err_msg).unwrap().into_raw()
        }
    }
}


#[no_mangle]
pub extern "C" fn free_string(s: *mut c_char) {
    if s.is_null() { return; }
    unsafe {
        let _ = CString::from_raw(s);
    }
}
