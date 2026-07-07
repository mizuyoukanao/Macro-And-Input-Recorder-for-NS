# Macro & Input Recorder for Nintendo Switch

Nintendo Switch ProコントローラーのUSBスニファ経由バイト列を記録し、シリアル通信でSwitchへ再生できるC#/.NET 8向けアプリです。コンソールに加え、キャプチャ・マクロ編集・Switch送信をまとめて操作できるWPF GUIを同梱しています。ボタン・スティック・ジャイロ角度を自由に指定したマクロ生成にも対応します。Visual Studioでソリューション(`MacroAndInputRecorder.sln`)を開くとそのままビルドできます。

## 参考リポジトリ
- USBスニファ: https://github.com/ataradov/usb-sniffer-lite
- Switch Proコントローラーのプロトコル: https://github.com/dekuNukem/Nintendo_Switch_Reverse_Engineering
- ジョイコンのクォータニオン・エンコード: https://github.com/kitlith/joycon-quat
- シリアル入力プロトコル(UARTControllerNX 拡張前提): https://github.com/uifnm/UARTControllerNX

## 機能
- USBスニファから読み取ったProコンのHIDレポートを時刻付きで記録
- モーションデータは従来の16bitジャイロ値そのまま方式と、joycon-quatを参考にしたクォータニオン方式を`--motion raw|quaternion`または`--quat`で切り替え
- 記録したバイト列をシリアル通信(UARTControllerNX互換フレーム)でSwitchへ再生
- JSONマクロからボタン・スティック・ジャイロをフレーム単位で合成して送信
- `sample_macro.json`を同梱。ボタンや角速度を編集してすぐ試せます

## 使い方
### GUI (WPF)
1. Visual Studioで`MacroAndInputRecorder.sln`を開き、`MacroRecorder.Gui`プロジェクトをスタートアップに設定して実行します。
2. "キャプチャ"タブでUSBスニファのポート/ボーレート/秒数/ポーリング間隔を設定し、「記録開始」を押すと`s`実行後に`b`を自動送信しながらバイト列を収集します。
3. 取得したフレームを一覧で確認し、「キャプチャから生成」でマクロ化します。DataGrid上でFramesやボタン名、スティックXY、ジャイロRoll/Pitch/Yawを直接編集できます。
4. "Switch送信"タブでUARTポート/ボーレートを指定し、「マクロ送信」または「キャプチャ送信」でSwitchに入力します。先頭5バイト`0xAA`固定のUARTControllerNXフレームで送ります。

### ビルド(コンソール)
`dotnet`未インストール環境ではVisual Studioのインストーラーで.NET 8 SDKを追加してください。コンソールからビルドする場合:
```bash
cd Macro-And-Input-Recorder-for-NS
# dotnet sdkがある場合
# dotnet build MacroAndInputRecorder.sln
```

### 記録
USBスニファのシリアルポートとボーレートを指定して、指定秒数分のHIDレポートを`capture.bin`に保存します。開始時に`s`を送り、`--poll`で指定した間隔(デフォルト50ms)ごとに`b`を投げてバッファ表示を取得します。
```bash
# 例: 12MHzロジックアナライザ出力をCOM5で10秒記録
# dotnet run --project MacroRecorder -- record --port COM5 --baud 12000000 --seconds 10 --poll 50 --output capture.bin --motion quaternion
```

### 再生
保存した`capture.bin`をSwitchに入力します。`--loop`でループ再生。
```bash
# dotnet run --project MacroRecorder -- replay --port /dev/ttyUSB1 --baud 2000000 --input capture.bin --loop --motion quaternion
```

### マクロ送信
`sample_macro.json`をテンプレートに、ボタン(列挙値)やスティック(-2048〜2047)、ジャイロ角度(度単位を内部で16bitスケール)を設定して送信します。
```bash
# dotnet run --project MacroRecorder -- macro --port COM6 --baud 2000000 --config MacroRecorder/sample_macro.json --motion quaternion
```

## マクロJSONの項目
- `Name`: 識別用文字列
- `FrameIntervalMs`: 1フレームの時間(ミリ秒)
- `Steps`: 各入力ステップ
  - `Frames`: 繰り返すフレーム数
  - `Buttons`: `A,B,X,Y,L,R,ZL,ZR,Plus,Minus,Home,Capture,Up,Down,Left,Right,LStick,RStick`のカンマ区切り
  - `LeftStick` / `RightStick`: `X` / `Y` は-2048〜2047で中心0
  - `Gyro`: `Roll`/`Pitch`/`Yaw` は16bitスケール済みの角速度。度で指定したい場合はコード中の`GyroState.FromDegrees`を利用してください。`--motion quaternion`指定時は送信直前にクォータニオンへエンコードされます。

## ファイル構成
- `MacroAndInputRecorder.sln`: Visual Studio用ソリューション
- `MacroRecorder/Program.cs`: コマンドエントリ
- `MacroRecorder/Services/*`: 記録・再生・マクロ生成・シリアル送信
- `MacroRecorder/Protocols/*`: ProコンHIDパケットのデコード
- `MacroRecorder/Serialization/*`: バイナリ/JSON入出力
- `MacroRecorder/Models/*`: 入力状態やマクロ定義のモデル
- `MacroRecorder/sample_macro.json`: 編集可能なマクロ例
- `MacroRecorder.Gui/*`: WPFベースのGUI。キャプチャ表示、マクロ編集、Switch送信タブを含みます。

## 注意
- 実機接続時はUSBスニファとSwitch UARTアダプタのボーレートを合わせてください。
- 実際のジャイロ変換係数は環境に応じて調整が必要です。`GyroState.DegreesScale`を変更して合わせてください。
- クォータニオン方式は8バイトの`x,y,z,w` little-endian signed 16bit / 16384スケールを想定します。従来の6バイトジャイロ値を扱う場合はデフォルトの`--motion raw`を使ってください。
- UARTControllerNX拡張フレームのバイト0〜4は0xAA固定です。本実装はこのヘッダーを付けてから長さ・ペイロード・チェックサムを送ります。
