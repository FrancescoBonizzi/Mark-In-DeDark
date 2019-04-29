#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO.IsolatedStorage;
using System.Diagnostics;

#endregion

namespace Mark.Engine.SoundEngine
{
    public class SoundManager : IDisposable
    {
        #region Dichiarazioni

        private SoundEffectInstance menu_music;

        private List<SoundEffectInstance> random_ambience_sounds_ = new List<SoundEffectInstance>();

        private SoundEffectInstance player_fire_sound_;
        private SoundEffectInstance player_walking_sound_;
        private SoundEffectInstance enlight_torch_;

        private SoundEffectInstance win_sound_;
        private SoundEffectInstance key_found_sound_;
        private SoundEffectInstance blood_sound_;
        private SoundEffectInstance put_candle_sound_;

        private List<SoundEffectInstance> all_sounds_ = new List<SoundEffectInstance>();
        private List<SoundEffectInstance> monster_sounds_ = new List<SoundEffectInstance>();

        private bool sound_on_ = true;
        private int current_index_;

        #endregion

        public SoundManager()
        {
        }

        public void Initialize()
        {
            all_sounds_ = new List<SoundEffectInstance>();
            monster_sounds_ = new List<SoundEffectInstance>();
            current_index_ = -1;

            #region Effects

            player_fire_sound_ = Loader.sounds_["fuoco"].CreateInstance();
            player_fire_sound_.IsLooped = true;
            all_sounds_.Add(player_fire_sound_);

            player_walking_sound_ = Loader.sounds_["player_walk"].CreateInstance();
            player_walking_sound_.IsLooped = true;
            all_sounds_.Add(player_walking_sound_);

            key_found_sound_ = Loader.sounds_["key"].CreateInstance();
            all_sounds_.Add(key_found_sound_);
            put_candle_sound_ = Loader.sounds_["candela"].CreateInstance();
            all_sounds_.Add(put_candle_sound_);
            enlight_torch_ = Loader.sounds_["accendi_fiamma"].CreateInstance();
            all_sounds_.Add(enlight_torch_);
            blood_sound_ = Loader.sounds_["blood"].CreateInstance();
            all_sounds_.Add(blood_sound_);
            win_sound_ = Loader.sounds_["win"].CreateInstance();
            all_sounds_.Add(win_sound_);

            #endregion
        }

        #region Metodi

        #region Monsters

        private int RegisterMonsterSound(string EntryName, bool Looped)
        {
            ++current_index_;
            var sound = Loader.sounds_[EntryName].CreateInstance();
            sound.IsLooped = Looped;
            monster_sounds_.Add(sound);
            all_sounds_.Add(sound);

            return current_index_;
        }

        public void PlayIndexedSound(int Index)
        {
            if (sound_on_)
            {
                if (monster_sounds_.Count > Index)
                    monster_sounds_[Index].Play();
            }
        }

        public void StopIndexedSound(int Index)
        {
            if (monster_sounds_.Count > Index)
                monster_sounds_[Index].Stop();
        }

        public int RegisterZombieScreamSound()
        {
            return RegisterMonsterSound("zombie_voice", false);
        }

        public int RegisterGoblinScreamSound()
        {
            return RegisterMonsterSound("goblin_voice", false);
        }

        public int RegisterSkeletronScreamSound()
        {
            return RegisterMonsterSound("skeletron_voice", false);
        }

        public int RegisterWolfScreamSound()
        {
            return RegisterMonsterSound("wolf_voice", false);
        }

        public int RegisterZombieAttackSound()
        {
            return RegisterMonsterSound("attack1", true);
        }

        public int RegisterWolfAttackSound()
        {
            return RegisterMonsterSound("attack2", true);
        }

        public int RegisterGoblinAttackSound()
        {
            return RegisterMonsterSound("attack3", true);
        }

        public int RegisterSkeletronAttackSound()
        {
            return RegisterMonsterSound("attack4", true);
        }

        public float ComputeIndexSoundsVolume(int Index1, int Index2, float Distance)
        {
            if (sound_on_)
            {
                float v = GetVolumeFromDistance(Distance);
                if (monster_sounds_.Count > Index1)
                    monster_sounds_[Index1].Volume = v;
                if (monster_sounds_.Count > Index2)
                    monster_sounds_[Index2].Volume = v;
                return v;
            }

            return 0.0f;
        }

        private float GetVolumeFromDistance(float distance)
        {
            // Scelgo il massimo: 80 pixel per cella, ne prendo 4, 320pixel di distanza => al quadrato 102400
            return distance > 102400 ? 0 : ((distance / -102400) + 1);
        }

        #endregion

        #region Player

        public void PlayPlayerWalking()
        {
            if (sound_on_)
            {
                if (player_walking_sound_.State != SoundState.Playing)
                    player_walking_sound_.Play();
            }
        }

        public void StopPlayerWalking()
        {
            if (sound_on_)
            {
                player_walking_sound_.Stop();
            }
        }

        public void PlayPlayerTorch()
        {
            if (sound_on_)
            {
                player_fire_sound_.Play();
            }
        }

        public void StopPlayerTorch()
        {
            if (sound_on_)
            {
                player_fire_sound_.Stop();
            }
        }

        public void PlayPutCandle()
        {
            if (sound_on_)
            {
                put_candle_sound_.Play();
            }
        }

        public void PlayBlood()
        {
            if (sound_on_)
            {
                blood_sound_.Play();
                player_fire_sound_.Stop();
            }
        }

        public void PlayEnlightTorch()
        {
            if (sound_on_)
            {
                enlight_torch_.Play();
            }
        }

        public void PlayWin()
        {
            if (sound_on_)
            {
                StopSounds();
                win_sound_.Play();
            }
        }

        public void PlayKey()
        {
            if (sound_on_)
            {
                key_found_sound_.Play();
            }
        }

        #endregion

        #region Settings

        public void SetSound(bool OnOff)
        {
            if (OnOff)
            {
                sound_on_ = true;
            }
            else
            {
                StopSounds();
                sound_on_ = false;
            }
        }

        public void SoundOn()
        {
            sound_on_ = true;
        }

        public void StopSounds()
        {
            foreach (SoundEffectInstance s in all_sounds_)
            {
                s.Stop();
            }
        }

        public void PauseAll()
        {
            foreach (SoundEffectInstance s in all_sounds_)
            {
                if (s.State == SoundState.Playing)
                    s.Pause();
            }
        }

        public void ResumeAll()
        {
            if (sound_on_)
            {
                foreach (SoundEffectInstance s in all_sounds_)
                {
                    if (s.State == SoundState.Paused)
                        s.Play();
                }
            }
        }

        #endregion

        #endregion

        public void Dispose()
        {
            for (int i = all_sounds_.Count - 1; i >= 0; --i)
                all_sounds_[i].Dispose();
        }
    }
}