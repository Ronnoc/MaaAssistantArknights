---
order: 4
icon: material-symbols:task
---

# タスク フロー プロトコル

`resource/tasks` の使用方法と各フィールドの説明

::: tip
JSONファイルはコメントをサポートしていません。テキスト内のコメントはプレゼンテーション用にのみ使用されます。直接コピーして使用しないでください。
:::

## 完全なフィールドの一覧

```json
{
    "TaskName" : {                          // タスク名

        "baseTask": "xxx",                  // タスクを生成するためのテンプレートとしてxxxタスクを使用します。詳細は以下の特別なタスクタイプのベースタスクを参照してください

        "algorithm": "MatchTemplate",       // オプション、認識アルゴリズムのタイプを示します
                                            // 指定されていない場合はデフォルトで MatchTemplate になります
                                            //     - JustReturn: 認識なしでアクションを実行します
                                            //     - MatchTemplate: 画像をマッチングします
                                            //     - OcrDetect: テキスト認識
                                            //     - Hash: ハッシュ計算

        "action": "ClickSelf",              // オプション、認識後のアクションを示します
                                            // 指定されていない場合はDoNothingになります
                                            //     - ClickSelf: 認識した位置をクリックします（認識された対象範囲内のランダムなポイント）
                                            //     - ClickRand: 画面全体でランダムな位置をクリックします
                                            //     - ClickRect: 指定した領域をクリックします、specificRectフィールドに対応し、このオプションは使用をお勧めしません
                                            //     - DoNothing: 何もしません
                                            //     - Stop: 現在のタスクを停止します
                                            //     - Swipe: スライドします、 specificRect および rectMove フィールドに対応します

        "sub": [ "SubTaskName1", "SubTaskName2" ],
                                            // オプション、サブタスク。現在のタスクが実行された後、各サブタスクが順番に実行されます
                                            // サブタスクにサブタスクの再セットを許可する。ただし、デッドループにならないよう注意してください

        "subErrorIgnored": true,            // オプション、サブタスクのエラーを無視するかどうか。
                                            // 指定されていない場合はデフォルトで false になります
                                            // false の場合、サブタスクにエラーがあると、後続のタスクは実行されません（このタスクがエラーを持つのと同等）
                                            // true の場合、サブタスクにエラーがあるかどうかに影響を与えません

        "next": [ "OtherTaskName1", "OtherTaskName2" ],
                                            // オプション、現在のタスクとサブタスクが実行された後に実行される次のタスクを示します
                                            // 前から後ろに識別され、最初に一致したものが実行されます
                                            // デフォルトでは、現在のタスクが実行された後に停止します
                                            // 同じタスクの場合、最初の認識後に2回目は認識されません。
                                            // "next": [ "A", "B", "A", "A" ] -> "next": [ "A", "B" ]
                                            // 最後のアイテム以外にJustReturnタイプのタスクが配置されないようにします

        "maxTimes": 10,                     // オプション、タスクを実行できる最大回数を示します
                                            // 指定されていない場合は無限になります
                                            // 最大回数に達した場合、 exceededNext フィールドが存在する場合、タスクは exceededNext として実行されます；それ以外の場合、タスクは停止します

        "exceededNext": [ "OtherTaskName1", "OtherTaskName2" ],
                                            // オプション、実行回数の最大値に達したときに実行されるタスクを示します
                                            // 指定されていない場合、実行回数の最大値に達したときに停止します；指定されている場合はここで実行されますが、 next ではありません
        "onErrorNext": [ "OtherTaskName1", "OtherTaskName2" ],
                                            // オプション、実行エラーが発生した場合に実行される後続タスクを示します

        "preDelay": 1000,                   // オプション、認識後にアクションが実行されるまでの時間をミリ秒単位で示します；指定されていない場合はデフォルトで0になります
        "postDelay": 1000,                  // オプション、実行後のアクションが次に認識される前に遅延する時間をミリ秒単位で示します；指定されていない場合はデフォルトで0になります

        "roi": [ 0, 0, 1280, 720 ],         // オプション、認識範囲を示す、フォーマットは [ x, y, width, height ]
                                            // 1280 * 720 に自動スケーリングします；指定されていない場合はデフォルトで [ 0, 0, 1280, 720 ]
                                            // できるだけ記入し、認識範囲を縮小するとパフォーマンス消費が低下し、認識が高速化されます

        "cache": false,                      // オプション、タスクがキャッシュを使用するかどうかを示します、デフォルトは false です；
                                            // 最初の認識後、常に最初に認識された位置のみが永久に認識されます。パフォーマンスを著しく向上させるために有効にします
                                            // ただし、特定する対象場所が全く変わらないタスクのみ適用され、特定する対象場所が変わる場合は false に設定してください

        "rectMove": [ 0, 0, 0, 0 ],         // オプション、認識後のターゲット移動、このオプションは推奨されません。 1280 * 720 を基準とした自動スケーリング
                                            // 例えばAが認識されたが、実際にクリックする場所はAの10ピクセル下の 5 * 2 エリアのどこかにある場合。
                                            // その場合、[ 0, 10, 5, 2 ]を記入してください。直接クリックすべき位置を認識しようとしても、このオプションは推奨されません。
                                            // 追加的に、 action が Swipe の場合、スライドの終了を示します。

        "reduceOtherTimes": [ "OtherTaskName1", "OtherTaskName2" ],
                                            // オプション、他のタスクの実行回数を減らすために実行します。
                                            // 例えば、理性ポーションを取ると、青色の開始アクションボタンに最後にクリックが効かなくなります。したがって、青い開始アクションは -1 になります。

        "specificRect": [ 100, 100, 50, 50 ],
                                            // action が ClickRect の場合、指定されたクリック位置（範囲内のランダムなポイント）を示し、
                                            // Swipe の場合、開始点を示します。
                                            // 1280 * 720 を基準とした自動スケーリング
                                            // algorithm が "OcrDetect" の場合、specificRect[0] と specificRect[1] はグレースケールの上下限閾値を示します。

        "specialParams": [ int, ... ],      // 一部の特殊な認識器に必要なパラメーター
                                            // 追加、 action が Swipe の場合はオプション、 [0] が duration 、 [1] が追加のスライドを有効にするかどうか

        "highResolutionSwipeFix": false,    // オプション項目。高解像度スワイプ修正を有効にするかどうか。
                                            // 現在のところ、ステージナビゲーションのみが Unity のスワイプ方式を使用していないため、有効化が必要。
                                            // デフォルトは false

        /* 以下のフィールドは、 algorithm が MatchTemplate である場合にのみ有効です */

        "template": "xxx.png",              // オプション、マッチングされるイメージファイルの名前
                                            // デフォルト値は"TaskName.png"

        "templThreshold": 0.8,              // オプション、イメージテンプレートのマッチングスコアの閾値。これを超えると、画像は認識されたと見なされます。
                                            // デフォルトは 0.8、ログに従って実際のスコアを確認できます。

        "maskRange": [ 1, 255 ],            // オプション、グレースケールマスク範囲。
                                            // 例えば、認識する必要のない画像の部分は黒く（グレースケール値が0）塗りつぶされます。
                                            // その場合、 "maskRange" を [ 1, 255 ] に設定して、マッチング時に黒く塗りつぶされた部分を瞬時に無視できます。

        "colorScales": [                    // method が HSVCount または RGBCount の場合に有効で必須、色マスク範囲。
            [                               // list<array<array<int, 3>, 2>> / list<array<int, 2>>
                [23, 150, 40],              // 構造は [[lower1, upper1], [lower2, upper2], ...]
                [25, 230, 150]              //     内側が int の場合はグレースケール、
            ],                              //     　　array<int, 3> の場合は三チャネルの色で、method によって RGB または HSV かが決まります；
            ...                             //     中間層の array<*, 2> は色（またはグレースケール）の下限と上限：
        ],                                  //     外側の層は異なる色範囲を示し、識別対象領域はテンプレート画像上のマスクの合併です。

        "colorWithClose": true,             // オプション、method が HSVCount または RGBCount の場合に有効、デフォルトは true
                                            // 色数えの際にマスク範囲を最初に閉じる処理をするかどうか。
                                            // 閉じる処理は小さな黒点を埋めることができ、通常は色数えのマッチング効果を向上させますが、画像に文字が含まれている場合は false にすることをお勧めします

        "method": "Ccoeff",                 // オプション、テンプレートマッチングアルゴリズム、リスト形式可能
                                            // 指定しない場合はデフォルトで Ccoeff
                                            //      - Ccoeff:       色に敏感でないテンプレートマッチングアルゴリズム、cv::TM_CCOEFF_NORMED に対応
                                            //      - RGBCount:     色に敏感なテンプレートマッチングアルゴリズム、
                                            //                      マスク範囲に基づいてマッチング領域とテンプレート画像を二値化し、
                                            //                      RGB 色空間内での類似度を F1-score を指標に計算し、
                                            //                      結果を Ccoeff の結果と点積する
                                            //      - HSVCount:     RGBCount に似ているが、色空間を HSV に変更

        /* 以下のフィールドは、 algorithm が OcrDetect の場合にのみ有効です */

        "text": [ "接管作戦", "代理指揮" ],  // 認識されるテキスト内容、いずれかが一致する場合に認識されたと見なされます。

        "ocrReplace": [                     // オプション、間違って認識されたテキストの置換（正規表現サポート）
            [ "千员", "干员" ],
            [ ".+击干员", "狙击干员" ]
        ],

        "fullMatch": false,                 // オプション、テキスト認識がすべての単語に一致する必要があるかどうか。デフォルトは false
                                            // false の場合、部分一致でも成功と見なされます。例えば、text：[ "start" ] の場合、実際の認識は "start action" でも成功と見なされます。
                                            // true の場合、 "start" を認識する必要があり、1つ以上の単語ではない

        "isAscii": false,                   // オプション、認識されるテキストコンテンツが ASCII 文字であるかどうか
                                            // 指定されていない場合、デフォルトはfalse

        "withoutDet": false,                // オプション、検出モデルを使用しない場合
                                            // 指定されていない場合、デフォルトは false


        "binThresholdLower": 140,           // オプション項目。グレースケールの二値化下限（デフォルトは140）
                                            // この値より低い画素は背景とみなされ、文字領域から除外されます

        "binThresholdUpper": 255,           // オプション項目。グレースケールの二値化上限（デフォルトは255）
                                            // この値より高い画素は背景とみなされ、文字領域から除外されます
                                            // [lower, upper] 範囲内の画素のみが文字の前景として残ります

        /* 以下のフィールドは、algorithmがHashの場合にのみ有効です */
        // アルゴリズムが未熟であり、特殊な場合にのみ使用されるため、現時点では推奨されていません
        // Todo
        
        /* 以下のフィールドは、アルゴリズムが FeatureMatch の場合のみ有効です */

        "template": "xxx.png",              // オプション。マッチングする画像ファイル名。文字列または文字列リストを指定できます。
                                            // デフォルトは "taskname.png"

        "count": 4,                         // マッチングする特徴点の数（閾値）。デフォルト値は 4

        "ratio": 0.6,                       // KNN マッチングアルゴリズムの距離比。[0 - 1.0]。比率が大きいほどマッチングが緩くなり、接続されやすくなります。デフォルトは 0.6

        "detector": "SIFT",                 // 特徴点検出の種類。オプションの値は SIFT、ORB、BRISK、KAZE、AKAZE、SURF。デフォルト値は SIFT
                                            // SIFT: 計算量が多い、スケール不変性、回転不変性。最も効果的。
                                            // ORB: 計算速度が非常に速い、回転不変性。ただし、スケール不変性は保証されません。
                                            // BRISK: 非常に高速な計算速度、スケール不変性、回転不変性を備えています。
                                            // KAZE: 2Dおよび3D画像に適用可能、スケール不変性、回転不変性を備えています。
                                            // AKAZE: 高速な計算速度、スケール不変性、回転不変性を備えています。
    }
}
```

## 特殊なタスクタイプ

### Template Task（`@` タイプのタスク）

Template task と base task は、**テンプレートタスク**と総称されます。

タスク "A" をテンプレートとして使用し、それを元に生成されたタスクを "B@A" と表現します。

- もしタスク "B@A" が `tasks.json` で明示的に定義されていない場合、 `sub` 、 `next` 、 `onErrorNext` 、 `exceededNext` 、 `reduceOtherTimes` のフィールド（もしくはタスク名が `#` で始まる場合は `B`）に `B@` の接頭辞を追加し、残りのパラメータはタスク "A" のものと同じになります。つまり、タスク "A" が以下のパラメータを持つ場合、

    ```json
    "A": {
        "template": "A.png",
        ...,
        "next": [ "N1", "N2" ]
    }
    ```

    以下の両方を定義することと同じです。

    ```json
    "B@A": {
        "template": "A.png",
        ...,
        "next": [ "B@N1", "B@N2" ]
    }
    ```

- もしタスク "B@A" が `tasks.json` で定義されている場合、以下のルールに従います。
    1. もし "B@A" の `algorithm` フィールドが "A" のものと異なる場合、派生クラスのパラメータは継承されません（`TaskInfo` によって定義されたパラメータのみが継承されます）。
    2. 画像マッチングタスクの場合、明示的に定義されていない場合は `template` は `B@A.png` となります（"A" の `template` 名を継承するのではなく、派生クラスのパラメータは直接 "A" タスクから継承されます）。
    3. `TaskInfo` 基底クラスで定義されたパラメータ（`algorithm`、`roi`、`next` などの任意のタスクパラメータの種類）について、"B@A" で明示的に定義されていない場合、これらのパラメータは "A" タスクパラメータから直接継承されます。ただし、前述の5つのフィールド（`sub` など）は、継承時に "B@" が接頭辞として付加されます。

### Base Task

Base task と template task は、**テンプレートタスク**と総称されます。

`baseTask` フィールドを持つタスクが base task です。

Base task は template task よりも優先されます。つまり、 `"B@A": { "baseTask": "C", ... }` はタスク A には関係ありません。

明示的に定義されていないパラメータは、対応する接頭辞なしのタスクの `baseTask` パラメータの値を使用します。ただし、`template` は明示的に定義されていない場合は `"taskName.png"` のままです。

#### マルチファイルタスク

後で読み込まれたタスクファイル（例：Yostarクライアントの `tasks.json`、以下ファイル2と呼ぶ）で定義されたタスクが、さらに早く読み込まれたタスクファイル（例：CNクライアントの `tasks.json`、以下ファイル1と呼ぶ）で同じ名前のタスクも定義されている場合、次のようになります。

- ファイル2のタスクに `baseTask` フィールドがない場合、ファイル1の同じ名前のタスクのフィールドを直接継承します。
- ファイル2のタスクに `baseTask` フィールドがある場合、ファイル1の同じ名前のタスクのフィールドを継承せずに上書きします。

### Virtual Task （仮想タスク）

Virtual task は sharp task （`#` タイプのタスク）とも呼ばれます。

名前に `#` を含むタスクは virtual task です。`#` の後には `next`、`back`、`self`、`sub`、`on_error_next`、`exceeded_next`、`reduce_other_times` が続くことがあります。

| 仮想タスクタイプ | 意味 | 簡単な例 |
|:---------:|:---:|:--------:|
| self | 親タスク名 | `"A": {"next": "#self"}` の `"#self"` は `"A"` と解釈されます。<br>`"B": {"next": "A@B@C#self"}` の `"A@B@C#self"` は `"B"` と解釈されます。<sup>1</sup> |
| back | # の前のタスク名 | `"A@B#back"` は `"A@B"` と解釈されます。<br>もし直接現れた場合は `"#back"` はスキップされます。 |
| next、sub など | 前のタスク名に対応するフィールド | `next` を例に取ります。<br>`"A#next"` は `Task.get("A")->next` と解釈されます。<br>もし直接現れた場合は `"#next"` はスキップされます。 |

_注<sup>1</sup>：`"XXX#self"` は `"#self"` と同じ意味を持ちます。_

#### 簡単な例

```json
{
    "A": { "next": [ "N1", "N2" ] },
    "C": { "next": [ "B@A#next" ] },

    "Loading": {
        "next": [ "#self", "#next", "#back" ]
    },
    "B": {
        "next": [ "Other", "B@Loading" ]
    }
}
```

これにより、次のことができます：

```cpp
Task.get("C")->next = { "B@N1", "B@N2" };

Task.get("B@Loading")->next = { "B@Loading", "Other", "B" };
Task.get("Loading")->next = { "Loading" };
Task.get_raw("B@Loading")->next = { "B#self", "B#next", "B#back" };
```

#### いくつかの用途

- 複数のタスクが `"next": [ "#back" ]` を持つ場合、`"Task1@Task2@Task3"` は `Task3`、`Task2`、`Task1` の順で順次実行されます。

#### その他の関連

```json
{
    "A": { "next": [ "N0" ] },
    "B": { "next": [ "A#next" ] },
    "C@A": { "next": [ "N1" ] }
}
```

上記の場合、`"C@B"` の `next`（つまり `C@A#next`）は `["N1"]` となります。`["C@N0"]` ではありません。

## 実行中のタスク変更

- `Task.lazy_parse()` は、実行時に json のタスク設定ファイルを読み込みます。 `lazy_parse` のルールは [マルチファイルタスク](#マルチファイルタスク) と同じです。
- `Task.set_task_base()` は、タスクの `baseTask` フィールドを変更します。

### 使用例

次のようなタスク設定ファイルがあるとします。

```json
{
    "A": {
        "baseTask": "A_default"
    },
    "A_default": {
        "next": [ "xxx" ]
    },
    "A_mode1": {
        "next": [ "yyy" ]
    },
    "A_mode2": {
        "next": [ "zzz" ]
    }
}
```

以下のコードは、`mode` の値に基づいてタスク "A" を変更し、タスク "B@A" などの他のタスクを変更します。

```cpp
switch (mode) {
case 1:
    Task.set_task_base("A", "A_mode1");  // これは基本的に A を A_mode1 の内容に置き換えることと同じである
    break;
case 2:
    Task.set_task_base("A", "A_mode2");
    break;
default:
    Task.set_task_base("A", "A_default");
    break;
}
```

## 式の計算

| 記号 | 意味 | 例 |
|:---------:|:---:|:--------:|
| `@` | テンプレートタスク | `Fight@ReturnTo` |
| `#`（単項） | 仮想タスク | `#self` |
| `#`（二項） | 仮想タスク | `StartUpThemes#next` |
| `*` | タスクの繰り返し | `(ClickCornerAfterPRTS+ClickCorner)*5` |
| `+` | タスクリストの結合（next の一連のプロパティで同じ名前を持つ最初のタスクのみが保持されます） | `A+B` |
| `^` | タスクリストの差分（前者にあり、後者にないもので、順序は変わらない） | `(A+A+B+C)^(A+B+D)` (= `C`) |

演算子 `@`、`#`、`*`、`+`、`^` には優先順位があります： `#`（単項）> `@` = `#`（二項）> `*` > `+` = `^`。

## スキーマチェック

このプロジェクトでは、`tasks.json`のjsonスキーマチェックを設定しています。スキーマファイルは `docs/maa_tasks_schema.json` です。

### Visual Studio

`MaaCore.vcxporj` で設定されており、すぐに使用できます。ヒント効果はより不明瞭で、いくつかの情報が欠落しています。

### Visual Studio Code

`.vscode/settings.json` で設定されており、vscode でその **プロジェクトフォルダ** を開くと使用できます。ヒントはより良く機能します。
