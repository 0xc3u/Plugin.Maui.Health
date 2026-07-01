# [1.4.0](https://github.com/0xc3u/Plugin.Maui.Health/compare/v1.3.0...v1.4.0) (2026-07-01)


### Features

* add batched CheckPermissionsAsync and a Permissions demo screen ([1c46ec9](https://github.com/0xc3u/Plugin.Maui.Health/commit/1c46ec95c0801d1dfcb88d2cccfcaf8297e2d4bd))

# [1.3.0](https://github.com/0xc3u/Plugin.Maui.Health/compare/v1.2.0...v1.3.0) (2026-07-01)


### Bug Fixes

* persist and read iOS workout distance & energy (iOS 18+) ([d2855a9](https://github.com/0xc3u/Plugin.Maui.Health/commit/d2855a9b37b8f95c196a9b99522691422feace75))


### Features

* add sleep data support with sample screen ([73d4d39](https://github.com/0xc3u/Plugin.Maui.Health/commit/73d4d3934d9c4fd4565eb74a2bdc2088afd8b15d))
* **sample:** add Heart Rate time-series screen ([182461f](https://github.com/0xc3u/Plugin.Maui.Health/commit/182461fa7f53bd46d0d5fd5bbd4952e566900837))
* **sample:** add Workouts screen with GPS route map ([748fc01](https://github.com/0xc3u/Plugin.Maui.Health/commit/748fc0148f423261a8b7be669047aa51ab36abf1))

# [1.2.0](https://github.com/0xc3u/Plugin.Maui.Health/compare/v1.1.0...v1.2.0) (2026-07-01)


### Features

* lower Android minimum to API 28 (Android 9) ([43bc64a](https://github.com/0xc3u/Plugin.Maui.Health/commit/43bc64a38a4b56334dcec422505cd72648defe8b))

# [1.1.0](https://github.com/0xc3u/Plugin.Maui.Health/compare/v1.0.0...v1.1.0) (2026-07-01)


### Bug Fixes

* batch permission requests and stop the iOS sample spinner hanging ([bdb75f7](https://github.com/0xc3u/Plugin.Maui.Health/commit/bdb75f7a80bab9940cc92fec016b8f8b535b2a03))
* crash on Android 14+ in ReadAllAsync/ReadLatestAvailableAsync (JNI ref) ([9711631](https://github.com/0xc3u/Plugin.Maui.Health/commit/9711631c2fad4ad3b95369c08ff91d0fff835e80))
* prevent iOS HealthKit callbacks from hanging the caller ([1a97568](https://github.com/0xc3u/Plugin.Maui.Health/commit/1a9756894e2f352e504964db261b9676a083ea5b))
* surface clear errors and stop ReadRecords hang on Android ([d2eb7ed](https://github.com/0xc3u/Plugin.Maui.Health/commit/d2eb7ed4b51caaa05b6f50a1264cb22de9cf108a))


### Features

* **sample:** card-based UI with MAUI.Graphics charts ([0d525f7](https://github.com/0xc3u/Plugin.Maui.Health/commit/0d525f79d7da1b1c60c5c2c23c53c8fd5e9bcdf2))
* **sample:** seed and read real health data on Android and iOS ([b1b7cb4](https://github.com/0xc3u/Plugin.Maui.Health/commit/b1b7cb454a4bfb73bfba6a74679a0dc154dd37a7))

# 1.0.0 (2026-06-30)


### Bug Fixes

* added support for V2Omax ([126915f](https://github.com/0xc3u/Plugin.Maui.Health/commit/126915f822bdc03b094d4bbbf86086e931377c11))
* Added support to provide unit when writing  values ([24cd691](https://github.com/0xc3u/Plugin.Maui.Health/commit/24cd6919a429ebd964920193e3eaa4a7cf114e18))


### Features

* add workout permission check and improve ios workout handling ([47412cd](https://github.com/0xc3u/Plugin.Maui.Health/commit/47412cd6373895f6ece3fa0653d33f3eb75d4e9f))
* added ReadLatestAvailableAsync ([8c30bfb](https://github.com/0xc3u/Plugin.Maui.Health/commit/8c30bfbc9a80e7106f78b4f5db1d48642f74e628))
* cross-platform permission requests, Android 14+ consent fix, .NET 10 tooling ([ed7c2fe](https://github.com/0xc3u/Plugin.Maui.Health/commit/ed7c2fe51fa025b3211a7043c19ec68d656ba8b7))
* implement android health connect and migrate to .net 10 ([b40aab1](https://github.com/0xc3u/Plugin.Maui.Health/commit/b40aab14983cb48ccade3249fa7faab56fd5e918))
* migration to .net9 ([ed3b3b6](https://github.com/0xc3u/Plugin.Maui.Health/commit/ed3b3b67f086052d836f572813b4df5e0ef9b484))
