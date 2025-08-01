---
order: 4
icon: material-symbols:task
---

# Tasks 스키마

::: tip
주의: JSON 형식은 주석을 지원하지 않으므로 아래의 예시에서 주석은 제거해주시기 바랍니다.
:::

`resource/tasks`의 사용법 및 각 필드 설명

## 개요

```json
{
    "작업이름": {                          // 작업 이름

        "baseTask": "xxx",                  // 작업의 템플릿으로 사용될 xxx 작업을 지정합니다. 특수 작업 유형인 경우 아래의 템플릿 작업에 대한 설명을 참조하세요.

        "algorithm": "MatchTemplate",       // 선택 사항, 인식 알고리즘 유형을 나타냅니다.
                                            // 입력하지 않으면 기본값은 MatchTemplate입니다.
                                            // - JustReturn: 인식 없이 작업을 직접 실행합니다.
                                            // - MatchTemplate: 이미지 매칭
                                            // - OcrDetect: 텍스트 인식
                                            // - Hash: 해시 계산

        "action": "ClickSelf",              // 선택 사항, 인식 후 작업 유형을 나타냅니다.
                                            // 입력하지 않으면 기본값은 DoNothing입니다.
                                            // - ClickSelf: 인식된 위치를 클릭합니다 (인식된 대상 범위 내에서 무작위 점).
                                            // - ClickRand: 화면 전체에서 무작위 위치를 클릭합니다.
                                            // - ClickRect: 지정된 영역을 클릭합니다. specificRect 필드에 해당합니다. 이 옵션은 권장하지 않습니다.
                                            // - DoNothing: 아무 작업도 수행하지 않습니다.
                                            // - Stop: 현재 작업을 중지합니다.
                                            // - Swipe: 슬라이드, specificRect 및 rectMove 필드와 관련이 있습니다.

        "sub": [ "SubTaskName1", "SubTaskName2" ],
                                            // 선택 사항, 서브 작업 목록입니다. 현재 작업이 실행된 후에 각 서브 작업을 순차적으로 실행합니다.
                                            // 서브 작업은 재귀적으로 정의할 수 있습니다. 그러나 무한 루프에 빠지지 않도록 주의하세요.

        "subErrorIgnored": true,            // 선택 사항, 서브 작업 오류를 무시할지 여부를 나타냅니다.
                                            // 입력하지 않으면 기본값은 false입니다.
                                            // false인 경우, 서브 작업에 오류가 발생하면 이후 작업을 실행하지 않고 현재 작업이 오류로 간주됩니다.
                                            // true인 경우, 서브 작업의 오류 여부와는 무관하게 이후 작업 실행에 영향을 미치지 않습니다.

        "maxTimes": 10,                     // 선택 사항, 작업이 최대로 실행될 수 있는 횟수를 나타냅니다.
                                            // 입력하지 않으면 무한히 실행됩니다.
                                            // 최대 횟수에 도달하면 exceededNext 필드가 존재한다면 exceededNext 작업이 실행되며, 그렇지 않으면 현재 작업이 중지됩니다.

        "next": [ "OtherTaskName1", "OtherTaskName2" ],
                                            // 선택 사항, 현재 작업 및 서브 작업 실행 후에 실행될 다음 작업을 나타냅니다.
                                            // 리스트의 맨 앞부터 순서대로 인식되며, 첫 번째로 일치하는 작업이 실행됩니다.
                                            // 입력하지 않으면 현재 작업 실행 후에 중지됩니다.
                                            // 동일한 작업에 대해 두 번째 인식부터는 첫 번째 인식 이후에 다시 인식되지 않습니다.
                                            // JustReturn 유형의 작업은 마지막 아이템에 위치하지 않도록 주의하세요.

        "exceededNext": [ "OtherTaskName1", "OtherTaskName2" ],
                                            // 선택 사항, 최대 실행 횟수에 도달했을 때 실행할 작업을 나타냅니다.
                                            // 입력하지 않으면 최대 실행 횟수에 도달하면 중지됩니다. 입력했을 경우, next 대신 여기서 실행됩니다.

        "onErrorNext": [ "OtherTaskName1", "OtherTaskName2" ],
                                            // 선택 사항, 실행 중 오류가 발생한 경우에 실행될 이후 작업을 나타냅니다.

        "preDelay": 1000,                   // 선택 사항, 인식된 후 작업이 실행될 때까지 대기하는 시간을 밀리초로 나타냅니다. 입력하지 않으면 기본값은 0입니다.
        "postDelay": 1000,                  // 선택 사항, 실행된 후 다음 인식까지 지연되는 시간을 밀리초로 나타냅니다. 입력하지 않으면 기본값은 0입니다.

        "roi": [ 0, 0, 1280, 720 ],         // 선택 사항, 인식 범위를 나타냅니다. 포맷은 [ x, y, width, height ]입니다.
                                            // 자동으로 1280 * 720으로 확장됩니다. 입력하지 않으면 기본값은 [ 0, 0, 1280, 720 ]입니다.
                                            // 가능한 한 많이 작성하고 인식 범위를 축소하면 성능 소비가 줄어들고 인식 속도가 빨라질 수 있습니다.

        "cache": false,                      // 선택 사항, 작업이 캐싱을 사용하는지 여부를 나타냅니다. 기본값은 false입니다.
                                            // 최초 인식 이후에는 해당 위치만 인식하며, 성능을 크게 개선할 수 있습니다.
   

 // 단, 대상 인식 위치가 절대로 변하지 않을 작업에만 사용하세요. 대상 인식 위치가 항상 변하는 경우 false로 설정하세요.

        "rectMove": [ 0, 0, 0, 0 ],         // 선택 사항, 인식 후 대상 이동을 나타냅니다. Auto-scaling으로 기준은 1280 * 720입니다.
                                            // 예를 들어 A가 인식되지만 클릭해야 할 실제 위치는 A 아래 10 픽셀 5 * 2 영역의 어딘가에 있을 경우
                                            // [ 0, 10, 5, 2 ]를 채우고 가능한 경우 바로 클릭할 위치를 인식합니다. 이 옵션은 권장하지 않습니다.
                                            // 추가적으로, action이 Swipe인 경우 유효하고 필수입니다. 슬라이드의 끝점을 나타냅니다.

        "reduceOtherTimes": [ "OtherTaskName1", "OtherTaskName2" ],
                                            // 선택 사항, 다른 작업의 실행 횟수를 줄이기 위해 실행합니다.
                                            // 예를 들어 정신약을 먹으면 마지막으로 파란색 시작 동작 버튼을 클릭한 효과가 없으므로 파란색 시작 동작이 -1됩니다.

        "specificRect": [ 100, 100, 50, 50 ],
                                            // action이 ClickRect인 경우 유효하고 필수입니다. 지정된 클릭 위치를 나타냅니다 (범위 내의 무작위 점).
                                            // action이 Swipe인 경우 유효하고 필수입니다. 슬라이드의 시작점을 나타냅니다.
                                            // Auto-scaling으로 기준은 1280 * 720입니다.
                                            // algorithm이 "OcrDetect"일 경우, specificRect[0]과 specificRect[1]은 그레이스케일의 상한선과 하한선 임계값을 나타냅니다.

        "specialParams": [ int, ... ],      // 일부 특수 인식기에 필요한 매개변수를 나타냅니다.
                                            // 추가 옵션, action이 Swipe인 경우 [0]은 지속 시간, [1]은 추가 슬라이드 여부를 나타냅니다.

        "highResolutionSwipeFix": false,    // 선택 항목. 고해상도 스와이프 보정을 활성화할지 여부.
                                            // 현재는 스테이지 내비게이션만 Unity 스와이프 방식을 사용하지 않으므로 이 옵션을 켜야 함
                                            // 기본값은 false

        /* 다음 필드들은 algorithm이 MatchTemplate인 경우에만 유효합니다. */

        "template": "xxx.png",              // 선택 사항, 이미지 매칭에 사용할 이미지 파일의 이름을 나타냅니다.
                                            // 기본값은 "작업이름.png"입니다.

        "templThreshold": 0.8,              // 선택 사항, 이미지 템플릿 매칭 점수의 임계값을 나타냅니다. 이 점수 이상인 경우 이미지가 인식되었다고 판단합니다.
                                            // 기본값은 0.8입니다. 실제 점수는 로그에서 확인할 수 있습니다.

        "maskRange": [ 1, 255 ],            // 선택 사항, 흑백 마스크 범위를 나타냅니다.
                                            // 예를 들어 인식할 필요가 없는 이미지 부분은 검은색으로 칠해질 수 있습니다 (0의 흑백 값).
                                            // 그러면 "maskRange"를 [ 1, 255 ]로 설정하면 검은색 처리된 부분은 즉시 무시됩니다.


        "colorScales": [                    // method가 HSVCount 또는 RGBCount일 때 유효하고 필수, 색상 마스크 범위.
            [                               // list<array<array<int, 3>, 2>> / list<array<int, 2>>
                [23, 150, 40],              // 구조는 [[lower1, upper1], [lower2, upper2], ...]
                [25, 230, 150]              //     내측이 int일 경우는 그레이스케일,
            ],                              //     　　array<int, 3>일 경우는 삼채널 색상으로, method에 따라 RGB 또는 HSV로 결정됩니다；
            ...                             //     중간 층의 array<*, 2>는 색상(또는 그레이스케일)의 하한과 상한:
        ],                                  //     외층은 다른 색상 범위를 나타내며, 식별할 영역은 템플릿 이미지에서 이들의 마스크의 합집합입니다.

        "colorWithClose": true,             // 선택 사항, method가 HSVCount 또는 RGBCount일 때 유효, 기본값은 true
                                            // 색상 수를 세는 동안 마스크 범위에 먼저 닫힘 연산을 적용할지 여부.
                                            // 닫힘 연산은 작은 검은 점을 메울 수 있으며, 일반적으로 색상 수 세기 매칭 효과를 향상시키지만 이미지에 텍스트가 포함된 경우 false로 설정하는 것이 좋습니다

        "method": "Ccoeff",                 // 선택 사항, 템플릿 매칭 알고리즘, 목록 형태일 수 있음
                                            // 지정하지 않을 경우 기본값은 Ccoeff
                                            //      - Ccoeff:       색상에 민감하지 않은 템플릿 매칭 알고리즘, cv::TM_CCOEFF_NORMED에 해당
                                            //      - RGBCount:     색상에 민감한 템플릿 매칭 알고리즘,
                                            //                      먼저 마스크 범위에 따라 매칭 영역과 템플릿 이미지를 이진화하고,
                                            //                      RGB 색 공간 내의 유사도를 F1-score를 지표로 계산한 후,
                                            //                      결과를 Ccoeff 결과와 점곱합니다
                                            //      - HSVCount:     RGBCount와 유사하지만 색상 공간을 HSV로 변경

        /* 다음 필드들은 algorithm이 OcrDetect인 경우에만 유효합니다. */

        "text": [ "接管作战", "代理指挥" ],  // 필수 사항, 인식할 텍스트 내용을 나타냅니다. 어떤 항목이 일치하면 인식되었다고 판단합니다.

        "ocrReplace": [                     // 선택 사항, 자주 오인식되는 텍스트를 대체할 내용입니다 (정규표현식 지원).
            [ "千员", "干员" ],
            [ ".+击干员", "狙击干员" ]
        ],

        "fullMatch": false,                 // 선택 사항, 텍스트 인식이 모든 단어를 일치시키는지 여부를 나타냅니다. 기본값은 false입니다.
                                            // false인 경우, 부분 문자열인 경우 인식됩니다. 예를 들어 text: [ "시작" ]이면 실제 인식에서 "시작 작업"도 인식됩니다.
                                            // true인 경우, "시작"만 인식되어야 합니다.

        "isAscii": false,                   // 선택 사항, 인식할 텍스트 내용이 ASCII 문자인지 여부를 나타냅니다.
                                            // 기본값은 false입니다.

        "withoutDet": false,                // 선택 사항, 탐지 모델을 사용하지 않을지 여부를 나타냅니다.
                                            // 기본값은 false입니다.

        "binThresholdLower": 140,           // 선택 항목. 그레이스케일 이진화의 하한 임계값 (기본값: 140)
                                            // 이 값보다 낮은 픽셀은 배경으로 간주되어 문자 영역에서 제외됩니다

        "binThresholdUpper": 255,           // 선택 항목. 그레이스케일 이진화의 상한 임계값 (기본값: 255)
                                            // 이 값보다 높은 픽셀은 배경으로 간주되어 문자 영역에서 제외됩니다
                                            // 최종적으로 [lower, upper] 범위의 픽셀만 문자 전경으로 유지됩니다

        /* 다음 필드들은 algorithm이 Hash인 경우에만 유효합니다. */
        // 이 알고리즘은 아직 미성숙하며, 특수한 경우에만 사용되므로 현재는 권장되지 않습니다.
        // Todo

        /* 다음 필드는 알고리즘이 FeatureMatch인 경우에만 유효합니다. */

        "template": "xxx.png",              // 선택 사항. 일치시킬 이미지 파일 이름. 문자열 또는 문자열 목록일 수 있습니다.
                                            // 기본값: "taskname.png"

        "count": 4,                         // 일치시킬 특징점 수(임계값), 기본값: 4

        "ratio": 0.6,                       // KNN 일치 알고리즘의 거리 비율, [0 - 1.0]. 비율이 클수록 일치가 느슨해지고 연결하기가 더 쉽습니다. 기본값: 0.6

        "detector": "SIFT",                 // 특징점 검출기 유형, 선택 사항: SIFT, ORB, BRISK, KAZE, AKAZE, SURF; 기본값: SIFT
                                            // SIFT: 높은 계산 복잡도, 스케일 불변성, 회전 불변성. 최상의 효과.
                                            // ORB: 매우 빠른 계산 속도, 회전 불변성. 하지만 스케일 불변성은 없습니다.
                                            // BRISK: 매우 빠른 계산 속도, 스케일 불변성, 회전 불변성을 가집니다.
                                            // KAZE: 2D 및 3D 이미지에 적용 가능하며, 스케일 불변성과 회전 불변성을 가집니다.
                                            // AKAZE: 빠른 계산 속도, 스케일 불변성과 회전 불변성을 가집니다.
    }
}
```

## 특수 작업 유형

### 템플릿 작업(`@` 유형 작업)

템플릿 작업과 기반 작업을 함께 **템플릿 작업**이라고 합니다.

작업 "A"를 템플릿으로 사용하고 "B@A"로 표기하여 "A"를 통해 생성된 작업을 나타낼 수 있습니다.

- 작업 "B@A"가 `tasks.json`에 명시적으로 정의되어 있지 않으면 `sub`, `next`, `onErrorNext`, `exceededNext`, `reduceOtherTimes` 필드에 "B@" 접두사를 추가합니다(또는 작업 이름이 `#`로 시작하는 경우 "B"로 접두사를 추가

합니다). 나머지 매개변수는 "A" 작업과 동일합니다. 즉, 작업 "A"에 다음과 같은 매개변수가 있는 경우:

```json
    "A": {
        "template": "A.png",
        ...,
        "next": [ "N1", "N2" ]
    }
```

이는 다음과 같이 정의한 것과 동일합니다.

```json
    "B@A": {
        "template": "A.png",
        ...,
        "next": [ "B@N1", "B@N2" ]
    }
```

- 작업 "B@A"가 `tasks.json`에 정의되어 있다면

  1. `B@A`의 `algorithm` 필드가 `A`의 `algorithm`과 다른 경우, 파생된 클래스 매개변수는 상속되지 않습니다(단지 `TaskInfo`로 정의된 매개변수만 상속됨).
  2. 이미지 매칭 작업의 경우, 명시적으로 정의되지 않으면 `template`은 `B@A.png`가 됩니다(단지 "A" 작업의 `template` 이름을 상속하지 않음). 그렇지 않은 경우 다른 파생된 클래스 매개변수는 "A" 작업에서 직접 상속됩니다.
  3. `TaskInfo` 기본 클래스에 정의된 매개변수 (모든 유형의 작업에 해당하는 매개변수, 예를 들어 algorithm, roi, next 등)가 `B@A` 내에서 명시적으로 정의되지 않은 경우, 앞에서 언급한 `sub` 등 다섯 개의 필드를 제외한 나머지 매개변수는 `B@` 접두어가 추가된 상속시 추가됩니다. 그 외의 매개변수는 `A` 작업의 매개변수를 직접 상속합니다.

  > _참고: `"XXX#self"`는 `"#self"`와 동일한 의미를 갖습니다._
  >

### 기반 작업

기반 작업과 템플릿 작업을 함께 **템플릿 작업**이라고 합니다.

`baseTask` 필드가 있는 작업은 기반 작업입니다.

기반 작업은 템플릿 작업보다 우선합니다. 즉, `"B@A": { "baseTask": "C", ... }`는 작업 A와 관련이 없습니다.

명시적으로 정의되지 않은 모든 매개변수는 해당 작업이 접두사 없이 기반 작업의 매개변수를 사용하며, `template`은 명시적으로 정의되지 않은 경우에는 여전히 `"작업이름.png"`입니다.

#### 멀티 파일 Task

나중에 로드된 작업 파일 (예: 글로벌 서버의 `tasks.json` 파일)에서 정의된 작업이 먼저 로드된 작업 파일 (예: CN 서버의 `tasks.json` 파일)에서도 동일한 이름의 작업이 정의된 경우 다음과 같습니다:

- 파일 2에 작업에 `baseTask` 필드가 없으면 파일 1에서 동일한 이름의 작업의 필드를 직접 상속합니다
- 파일 2에 작업에 `baseTask` 필드가 있으면 파일 1에서 동일한 이름의 작업의 필드를 직접 상속하는 대신에 덮어씁니다.

### 가상 작업

가상 작업은 또한 숫자 기호(`#` 유형 작업)로 불립니다.

이름에 `#`이 포함된 작업은 가상 작업입니다. `#` 다음에는 `next`, `back`, `self`, `sub`, `on_error_next`, `exceeded_next`, `reduce_other_times`가 따라옵니다.

| 가상 작업 유형 |              의미              |                                                                       간단한 예시                                                                       |
| :------------: | :----------------------------: | :-----------------------------------------------------------------------------------------------------------------------------------------------------: |
|      self      |         부모 작업 이름         | `"A": {"next": "#self"}`에서 `"#self"`는 `"A"`로 해석됩니다.`<br>"B": {"next": "A@B@C#self"}`에서 `"A@B@C#self"`은 `"B"`로 해석됩니다.`<sup>`1 `</sup>` |
|      back      |         # 앞 작업 이름         |                                   `"A@B#back"`은 `"A@B"`로 해석됩니다.`<br>`직접 나타날 경우 `"#back"`은 무시됩니다.                                    |
|  next, sub 등  | 이전 작업 이름에 대응하는 필드 |                  `next`를 예로 들면:`<br>"A#next"`는 `Task.get("A")->next`로 해석됩니다.`<br>`직접 나타날 경우 `"#next"`은 무시됩니다.                  |

_참고 `<sup>`1 `</sup>`: `"XXX#self"`는 `"#self"`와 같은 의미를 갖습니다._

#### 간단한 예시

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

다음 형식도 가능합니다.

```cpp
Task.get("C")->next = { "B@N1", "B@N2" };

Task.get("B@Loading")->next = { "B@Loading", "Other", "B" };
Task.get("Loading")->next = { "Loading" };
Task.get_raw("B@Loading")->next = { "B#self", "B#next", "B#back" };
```

#### 몇 가지 사용 예시

- `"next": ["#back"]`와 같이 여러 작업이 주어진 경우, `"Task1@Task2@Task3"`은 `Task3`, `Task2`, `Task1` 순으로 순차적으로 실행됨을 나타냅니다.

#### 기타 관련 정보

```json
{
    "A": { "next": ["N0"] },
    "B": { "next": ["A#next"] },
    "C@A": { "next": ["N1"] }
}
```

위의 예시에서, `"C@B" -> next` (즉, `C@A#next`)는 `["N1"]`이 아닌 `["C@N0"]`입니다.

## 런타임에서 작업 수정

- `Task.lazy_parse()`는 런타임에서 json 작업 구성 파일을 로드할 수 있습니다. lazy_parse 규칙은 [멀티파일 Task](#멀티-파일-task)와 동일합니다.
- `Task.set_task_base()`를 사용하여 작업의 `baseTask` 필드를 수정할 수 있습니다.

### 사용 예시

다음과 같은 작업 구성 파일이 있다고 가정합니다.

```json
{
  "A": {
    "baseTask": "A_default"
  },
  "A_default": {
    "next": ["xxx"]
  },
  "A_mode1": {
    "next": ["yyy"]
  },
  "A_mode2": {
    "next": ["zzz"]
  }
}
```

다음 코드는 `mode` 값에 따라 작업 "A"를 변경하며, "A"에 의존하는 다른 작업인 "B@A" 등도 변경됩니다:

```cpp
switch (mode) {
case 1:
    Task.set_task_base("A", "A_mode1");  // 이것은 사실상 A를 A_mode1의 내용으로 대체하는 것과 같습니다. 아래와 같이 진행됩니다.
    break;
case 2:
    Task.set_task_base("A", "A_mode2");
    break;
default:
    Task.set_task_base("A", "A_default");
    break;
}
```

## 표현식 계산

|       기호        |                                   의미                                   |                  예시                  |
| :---------------: | :----------------------------------------------------------------------: | :------------------------------------: |
|        `@`        |                               템플릿 작업                                |            `Fight@ReturnTo`            |
| `#` (unary/단항)  |                                가상 작업                                 |                `#self`                 |
| `#` (binary/이항) |                                가상 작업                                 |          `StartUpThemes#next`          |
|        `*`        |                                작업 반복                                 | `(ClickCornerAfterPRTS+ClickCorner)*5` |
|        `+`        |                              작업 목록 병합                              |                 `A+B`                  |
|        `^`        | 작업 목록 차집합 (앞쪽에는 있지만 뒤쪽에는 없는 작업으로, 순서는 유지됨) |      `(A+A+B+C)^(A+B+D)` (= `C`)       |

`@`, `#`, `*`, `+`, `^` 연산자의 우선순위는 다음과 같습니다: `#` (단항) > `@` = `#` (이항) > `*` > `+` = `^`.

## 스키마 확인

이 프로젝트는 `tasks.json`에 대한 JSON 스키마 확인을 설정하며, 스키마 파일은 `docs/maa_tasks_schema.json`입니다.

### Visual Studio

`MaaCore.vcxproj`에 구성되어 있으며 즉시 사용할 수 있습니다. 힌트 효과는 상대적으로 불명확하며 일부 정보가 누락됩니다.

### Visual Studio Code

`.vscode/settings.json`에 구성되어 있으며, 해당 **프로젝트 폴더**를 vscode로 열고 사용할 수 있습니다. 힌트가 더 잘 작동합니다.
