# NativeBridge
- NativeBridge는 [UAAL(Unity as a Library)](https://github.com/Unity-Technologies/uaal-example) 기반 프로젝트에서 Unity와 Native간 메세지 통신을 위해 만들어진 모듈입니다.
- Native(Android,iOS,Windows) 플러그인은 제공되지 않습니다.
<br><br>
## Unity 버전
- 2020.3.32f1 or higher
<br><br>
## 지원 플랫폼
- [x] Editor
- [x] Android
- [x] iOS
- [x] Windows (x86, x64)
- [ ] macOS
<br><br>
## 통신 규격
- 메세지 통신은 Event Name과 JSON string 데이터를 Native 및 Unity에서 정해진 규칙에 따라 송수신합니다.

### Event Name
- Native 및 Unity 간 메세지 통신 시 사용할 Event Name의 규격을 정의합니다.
- Event Name은 아래 정의된 규칙에 맞게 작성하고, 구분자는 `_` 를 사용하여 구분합니다.
- `{호출 및 수신 위치}_{클래스 명}_{메소드 명}`

|구분|설명|예시|
|------|---|---|
|호출 및 수신 위치|메세지를 전달 및 수신하는 위치를 나타냅니다.|N2U(Native to Unity) <br>U2N(Unity to Native)|
|클래스 명|메세지를 수신하는 클래스의 이름을 나타냅니다.|ApplicationController|
|메소드 명|메세지를 실행하는 메소드의 이름을 나타냅니다.|StartApplication|

#### Event Name 정의 예시
```
// Native -> Unity로 앱 시작 요청.
N2U_REQ_ApplicationController_StartApplication,
// Unity -> Native로 앱 시작 응답.
U2N_ACK_ApplicationController_StartApplication,

// Unity -> Native로 유저 데이터 요청.
U2N_REQ_ApplicationController_UserData,
// Native -> Unity로 유저 데이터 응답.
N2U_ACK_ApplicationController_UserData,
```

## 메세지 처리 방식
* Native에서 전달받은 메세지를 Unity에서 처리할 때 Unity Main Thread에서 처리 하기 위해 Native에서 메세지 수신 후 메세지 Queue에 Enqueue, Update Loop에서 Dequeue하여 메세지를 처리합니다.
```
[AOT.MonoPInvokeCallback(typeof(MessageCallback))]
private static void OnReceiveMessage([MarshalAs(UnmanagedType.HString)] string eventName, [MarshalAs(UnmanagedType.HString)] string jsonData)
{
    // parse message.
    string[] events = eventName.Split(INTERFACE_SEPARATOR);
    string className = events[2];
    string methodName = events[3];

    // enqueue.
    messageQueue.Enqueue(() => { OnProcessEvent(eventName, className, methodName, jsonData); });
}
```
```
public void Update()
{
    while (messageQueue.Count > 0)
    {
        if (messageQueue.TryDequeue(out var action))
        {
            action.Invoke();
        }
    }
}
```

### JSON 데이터 규격
- Native 및 Unity 간 메세지 통신 시 사용할 JSON 데이터의 규격을 정의합니다.
- Event 처리의 성공 유무와 관계 없이 기본적으로 동일 포맷을 사용하나, 성공/실패 케이스에 따라 데이터를 전송합니다.
```
// 성공 시 JSON 데이터 규격
{
  "errorCode": "",
  "errorMsg": "",
  "data": {
    "userList": [
        {
            "userId": 1,
            "nickname": "유저01",
            "type": 1,
            "state": 0,
        },
        {
            "userId": 2,
            "nickname": "유저02",
            "type": 1,
            "state": 0,
        },
        {
            "userId": 3,
            "nickname": "유저03",
            "type": 1,
            "state": 0,
        },
    ]
  }
}

// 에러 발생 시 JSON 데이터 규격
{
  "errorCode": "010101",
  "errorMsg": "NULL EXCEPTION",
  "data": {
  }
}
```