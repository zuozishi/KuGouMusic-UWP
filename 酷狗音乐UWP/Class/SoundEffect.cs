using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.Devices;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;

namespace 酷狗音乐UWP.Class
{
    public class SoundEffect
    {
        public class EQ
        {
            public enum EQInitialize
            {
                无,重低音强化,低音强化,高音强化,古典音乐,响度,嘻哈,声音强化,摇滚,流行音乐,爵士,电子,舞曲,节奏布鲁斯,语音,音响效果
            }

            public static void GetEQCurrent(EQInitialize eqinit)
            {
                var audio = AudioEffectsManager.CreateAudioRenderEffectsManager(MediaDevice.GetDefaultAudioRenderId(AudioDeviceRole.Default), Windows.Media.Render.AudioRenderCategory.Media);
                var asd= new AudioEffectDefinition("sadawd");
                var supportedEncodingProperties = new List<AudioEncodingProperties>();
                AudioEncodingProperties encodingProps1 = AudioEncodingProperties.CreatePcm(44100, 1, 32);
                encodingProps1.Subtype = MediaEncodingSubtypes.Float;
                AudioEncodingProperties encodingProps2 = AudioEncodingProperties.CreatePcm(48000, 1, 32);
                encodingProps2.Subtype = MediaEncodingSubtypes.Float;
                supportedEncodingProperties.Add(encodingProps1);
                supportedEncodingProperties.Add(encodingProps2);
                switch (eqinit)
                {
                    case EQInitialize.无:
                        break;
                    case EQInitialize.重低音强化:
                        break;
                    case EQInitialize.低音强化:
                        break;
                    case EQInitialize.高音强化:
                        break;
                    case EQInitialize.古典音乐:
                        break;
                    case EQInitialize.响度:
                        break;
                    case EQInitialize.嘻哈:
                        break;
                    case EQInitialize.声音强化:
                        break;
                    case EQInitialize.摇滚:
                        break;
                    case EQInitialize.流行音乐:
                        break;
                    case EQInitialize.爵士:
                        break;
                    case EQInitialize.电子:
                        break;
                    case EQInitialize.舞曲:
                        break;
                    case EQInitialize.节奏布鲁斯:
                        break;
                    case EQInitialize.语音:
                        break;
                    case EQInitialize.音响效果:
                        break;
                    default:
                        break;
                }
            }

        }
    }
}
