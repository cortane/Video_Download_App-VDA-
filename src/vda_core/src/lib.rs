use std::ffi::{CStr, CString};
use std::os::raw::c_char;
use std::os::windows::process::CommandExt;
use std::process::{Command, Stdio};
use std::io::{BufRead, BufReader};

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
        "--newline".to_string(), // ログ解析用
    // ... (unchanged)
        "--no-playlist".to_string(), // プレイリストでも単体動画のみダウンロード
        "-P".to_string(),
        path_str.clone(),
        url_str.clone()
    ];

    // フォーマット判定
    match format_str.as_str() {
        "mp3" | "m4a" | "wav" | "flac" | "aac" | "opus" => {
            // 指定された拡張子がサーバー上にある場合はそれを優先的にダウンロードする
            // (変換処理の負荷と時間を回避するため)
            args.push("-f".to_string());
            args.push(format!("bestaudio[ext={}]/bestaudio", format_str));

            args.push("--extract-audio".to_string());
            args.push("--audio-format".to_string());
            args.push(format_str.clone());
        },
        _ => {
            // 動画形式の場合 (mp4, mkv, webm, etc)
            // 最高画質・音質をダウンロードして、指定コンテナにマージする
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

    // QuickJS設定 (JavaScript Runtime)
    if !qjs_path_str.is_empty() {
        args.push("--js-runtimes".to_string());
        // quickjs:PATH
        args.push(format!("quickjs:{}", qjs_path_str));
    }
    
    if !cookie_path_str.is_empty() {
        args.push("--cookies".to_string());
        args.push(cookie_path_str);
    }

    // Client Impersonation
    // android/ios等 偽装は一切行わない (ブラウザのCookieだけで通す)
    
    // User-Agent (Chrome)
    // args.push("--user-agent".to_string());
    // args.push("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36".to_string());

    // 実行
    match Command::new(exec_path_str)
        .args(&args)
        .stdout(Stdio::piped())
        .stderr(Stdio::piped())
        .creation_flags(0x08000000) // CREATE_NO_WINDOW
        .spawn() 
    {
        Ok(child) => {
            // 出力をキャプチャしてログを構築
            let stdout = child.stdout.expect("Failed to open stdout");
            let stderr = child.stderr.expect("Failed to open stderr");
            
            let mut log_output = String::new();
            let mut downloaded = false;
            
            let reader = BufReader::new(stdout);
            for line in reader.lines() {
                if let Ok(l) = line {
                    log_output.push_str(&l);
                    log_output.push('\n');
                    
                    // 成功判定キーワード
                    if l.contains("Destination:") || l.contains("has already been downloaded") || l.contains("Merger completed") || l.contains("100%") {
                        downloaded = true;
                    }
                }
            }
            
            // stderrも読む
            let err_reader = BufReader::new(stderr);
            for line in err_reader.lines() {
                if let Ok(l) = line {
                    log_output.push_str("ERR: ");
                    log_output.push_str(&l);
                    log_output.push('\n');
                }
            }

            // 結果判定
            if downloaded {
                let success_msg = format!("Success\n---LOG---\n{}", log_output);
                CString::new(success_msg).unwrap().into_raw()
            } else {
                let fail_msg = format!("Failed (No file created)\n---LOG---\n{}", log_output);
                CString::new(fail_msg).unwrap().into_raw()
            }
        },
        Err(e) => {
            CString::new(format!("Error executing yt-dlp: {}", e)).unwrap().into_raw()
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

mod tests {
    use super::*;

    #[test]
    fn it_works() {
        let result = add(2, 2);
        assert_eq!(result, 4);
    }
}
