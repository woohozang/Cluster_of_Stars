# ✨ 별무리 (Constellations)
> **빛반사를 통해 별자리를 잇는 VR 퍼즐 어드벤처** > *VR Puzzle Adventure connecting light and constellations*
<img width="624" height="359" alt="image" src="https://github.com/user-attachments/assets/5b1f597e-a491-49f4-989d-939415419171" />

[![HCI Award](https://img.shields.io/badge/Award-HCI%20CA%20Excellence%20Award-gold?style=for-the-badge&logo=trophy)](YOUR_AWARD_LINK_OR_CERTIFICATE_IMAGE)
![Unity](https://img.shields.io/badge/Unity-2022.3.LTS-black?style=for-the-badge&logo=unity)
![Meta Quest 3](https://img.shields.io/badge/Device-Meta%20Quest%203-blue?style=for-the-badge&logo=oculus)
![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20VR-green?style=for-the-badge)

<br/>

## 🏆 Award & Recognition
**202X HCI Korea Creative Award (CA) 우수상 수상**
> "가상 현실에서의 몰입감 넘치는 상호작용 설계와 독창적인 퍼즐 메카닉스"



## 📷 Preview & Gameplay

| 🚀 콕핏 조작 (Interaction) | 🧩 빛 반사 퍼즐 (Puzzle) | 👐양손 조작 |
| :---: | :---: | :---: |
|<img width="362" height="277" alt="image" src="https://github.com/user-attachments/assets/288d2491-2b25-4557-82b9-a4fb2e0c9280" />|<img width="352" height="239" alt="image" src="https://github.com/user-attachments/assets/41902ed4-4933-4e05-858d-ffd176974452" />|<img width="488" height="292" alt="image" src="https://github.com/user-attachments/assets/ab611f11-209e-43df-89b0-aaf697116fc5" />|
| **직관적인 레버 조작** | **거울을 이용한 빛 경로 설계** | **회전 인터랙션**|

<br/>



## 📺 Demo Video
| Gameplay Trailer |
| :---: |
| <video src="https://github.com/user-attachments/assets/0fdbe0ab-4088-4c2a-b36f-a614f8a4e03a" width="100%"></video> |

<br/>

## 📖 Project Overview
별무리(Star Cluster)는 망가진 별자리를 복구하기 위해 빛을 반사하고 경로를 연결하는 VR 퍼즐 게임입니다.
Meta Quest 3의 기능을 활용하여, 플레이어는 우주선 콕핏에서 직접 레버를 당기고 버튼을 조작하며 실제 우주를 항해하는 듯한 경험을 할 수 있습니다.

### 🎮 Key Features
- **Light Reflection Puzzle:** 거울과 프리즘을 배치하여 빛을 목표 지점까지 유도하는 3D 퍼즐.
- **Realistic Cockpit Interaction:** 물리적인 버튼, 레버, 스로틀 조작을 통한 우주선 제어.
- **Immersive Audio-Visual:** 우주 공간의 광활함을 표현한 쉐이더와 반응형 사운드 디자인.

<br/>

## 💡 HCI & Technical Highlights
이 프로젝트는 사용자의 감각적 몰입(Sensory Immersion)을 극대화하기 위해 다음과 같은 기술적 시도를 적용했습니다.

### 1. Non-linear Haptic Feedback (비선형 햅틱 피드백)
단순한 진동이 아닌, 조작감의 깊이를 더하기 위해 레버의 위치와 상태에 따른 **가변형 햅틱 패턴**을 구현했습니다.
- **Ratcheting Effect:** 레버를 당길 때 기어 톱니가 맞물리는 듯한 물리적 저항감을 진동으로 표현.
- **Animation Curve:** `AnimationCurve`를 활용하여 구간별 진동 세기를 정교하게 매핑.
- **Control/Display Ratio:** 시각적 오프셋을 적용하여 각각 다른 무게감의 별 오브젝트 조작
- **Haptic Feedback Model:** Role이 다른 별 오브젝트 조작 간 차별화된 햅틱 피드백 제공
  
  <img width="401" height="235" alt="image" src="https://github.com/user-attachments/assets/f4ecd049-be6f-4751-949a-814b46e3a5bd" />
  <img width="489" height="232" alt="image" src="https://github.com/user-attachments/assets/a400b8f6-a11c-413f-ac14-d4a9b5d0f57d" />

### 2. 3D Spatial Audio (공간 음향 기술)
사용자의 위치와 시선에 따라 소리의 방향성과 거리감이 실시간으로 변화하는 스테레오 환경을 구축하였습니다.
<img width="309" height="237" alt="image" src="https://github.com/user-attachments/assets/2dc119ac-e38a-480d-8103-ec6334d78b88" />
- 빛이 닿는 신호를 공간 음향을 통해 인지 제공

### 3. Visual Component (시각적 요소)
Unity : **LineRenderer** , C# : **Vector3.Reflect** , Unity : **Particle System** 적용하여 시각적 인지 제공

- LineRenderer : 기본적인 Emitter 역할 수행, PostProcessing -> Bloom, Vignette 적용하여 빛의 느낌 제공
- Vector3.Reflect : 물리적인 반사 각도 계산 알고리즘
- Particle System : 별 오브젝트에 빛이 닿으면 활성화하여 사용자에게 인지 제공
<img width="489" height="262" alt="image" src="https://github.com/user-attachments/assets/8d83f300-9069-4a1d-b5e3-1d4036a87e98" />


