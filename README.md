# ✨ 별무리 (Star Cluster)
> **빛과 별자리를 잇는 VR 퍼즐 어드벤처** > *VR Puzzle Adventure connecting light and constellations*

[![HCI Award](https://img.shields.io/badge/Award-HCI%20CA%20Excellence%20Award-gold?style=for-the-badge&logo=trophy)](YOUR_AWARD_LINK_OR_CERTIFICATE_IMAGE)
![Unity](https://img.shields.io/badge/Unity-2022.3.LTS-black?style=for-the-badge&logo=unity)
![Meta Quest 3](https://img.shields.io/badge/Device-Meta%20Quest%203-blue?style=for-the-badge&logo=oculus)
![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20VR-green?style=for-the-badge)

<br/>

## 🏆 Award & Recognition
**202X HCI Korea Creative Award (CA) 우수상 수상**
> "가상 현실에서의 몰입감 넘치는 상호작용 설계와 독창적인 퍼즐 메카닉스"

<br/>

## 📺 Demo Video
| Gameplay Trailer | Core Interaction |
| :---: | :---: |
| [![Video Label](http://img.youtube.com/vi/YOUR_VIDEO_ID/0.jpg)](https://youtu.be/YOUR_VIDEO_ID) | ![Interaction GIF](LINK_TO_YOUR_GIF) |

<br/>

## 📖 Project Overview
**별무리(Star Cluster)**는 망가진 별자리를 복구하기 위해 빛을 반사하고 경로를 연결하는 VR 퍼즐 게임입니다.
Meta Quest 3의 기능을 활용하여, 플레이어는 우주선 콕핏에서 직접 레버를 당기고 버튼을 조작하며 실제 우주를 항해하는 듯한 경험을 할 수 있습니다.

### 🎮 Key Features
- **Light Reflection Puzzle:** 거울과 프리즘을 배치하여 빛을 목표 지점까지 유도하는 3D 퍼즐.
- **Realistic Cockpit Interaction:** 물리적인 버튼, 레버, 스로틀 조작을 통한 우주선 제어.
- **Immersive Audio-Visual:** 우주 공간의 광활함을 표현한 쉐이더와 반응형 사운드 디자인.

<br/>

## 💡 HCI & Technical Highlights
이 프로젝트는 **사용자의 감각적 몰입(Sensory Immersion)**을 극대화하기 위해 다음과 같은 기술적 시도를 적용했습니다.

### 1. Non-linear Haptic Feedback (비선형 햅틱 피드백)
단순한 진동이 아닌, 조작감의 깊이를 더하기 위해 레버의 위치와 상태에 따른 **가변형 햅틱 패턴**을 구현했습니다.
- **Ratcheting Effect:** 레버를 당길 때 기어 톱니가 맞물리는 듯한 물리적 저항감을 진동으로 표현.
- **Animation Curve:** `AnimationCurve`를 활용하여 구간별 진동 세기를 정교하게 매핑.


