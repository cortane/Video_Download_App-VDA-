# 動画ダウンロードアプリ (VDA) 動作確認手順

## 1. 前提条件
このアプリは `yt-dlp` コマンドラインツールを内部で使用します。
以下の手順で準備してください。

1. [yt-dlp GitHub Releases](https://github.com/yt-dlp/yt-dlp/releases) から `yt-dlp.exe` をダウンロードします。
2. ダウンロードした `yt-dlp.exe` を以下のフォルダに配置してください（またはシステムのPATHに通してください）。
   `Video_Download_App(VDA)\src\VDA.UI\bin\Release\net9.0-windows\`

## 2. アプリの起動
以下のコマンドでアプリを起動します。

```powershell
cd "c:\Users\developer\WorkSpace\開発プロジェクト用フォルダ\動画ダウンロードアプリ\Video_Download_App(VDA)\src\VDA.UI"
.\bin\Release\net9.0-windows\VDA.UI.exe
```

## 3. 使用方法
1. "動画 URL" 欄に YouTube 等の動画URLを入力します。
2. "保存先フォルダ" を確認または変更します。
3. "ダウンロード開始" ボタンを押します。
4. ステータスバーに "完了: Success" と表示されたらダウンロード成功です。

## 4. トラブルシューティング
- Status: "Error: No such file or directory" -> `yt-dlp.exe` が見つかりません。配置場所を確認してください。
- Status: "Error: ..." (その他) -> URLが正しいか、ネットワーク接続を確認してください。
