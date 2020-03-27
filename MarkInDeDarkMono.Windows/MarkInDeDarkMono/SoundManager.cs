using MarkInDeDarkMono.Assets;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace MarkInDeDarkMono
{
    public class SoundManager
    {
        private SoundEffectInstance _player_fire_sound;
        private SoundEffectInstance _player_walking_sound;
        private SoundEffectInstance _player_falling_sound;
        private SoundEffectInstance _enlight_torch;

        private SoundEffectInstance _ladder_sound;
        private SoundEffectInstance _key_found_sound;
        private SoundEffectInstance _blood_sound;
        private SoundEffectInstance _put_candle_sound;
        private SoundEffectInstance _break_wall_sound;

        private SoundEffectInstance _taurus_starting_charge_sound;

        private List<SoundEffectInstance> _all_sounds;

        private List<SoundEffectInstance> _monster_sounds;
        private int _currentLastSoundAddedIndex;
        private readonly SoundEffectInstance _patrol_seen_you_sound;

        private readonly AssetsLoader _assetsLoader;

        public SoundManager(AssetsLoader assetsLoader)
        {
            _assetsLoader = assetsLoader;

            _all_sounds = new List<SoundEffectInstance>();
            _monster_sounds = new List<SoundEffectInstance>();
            _currentLastSoundAddedIndex = -1;

            _player_fire_sound = assetsLoader.Sounds["fuoco"].CreateInstance();
            _player_fire_sound.IsLooped = true;
            _all_sounds.Add(_player_fire_sound);

            _player_walking_sound = assetsLoader.Sounds["player_walk"].CreateInstance();
            _player_walking_sound.IsLooped = true;
            _all_sounds.Add(_player_walking_sound);


            _key_found_sound = assetsLoader.Sounds["key"].CreateInstance();
            _all_sounds.Add(_key_found_sound);
            _put_candle_sound = assetsLoader.Sounds["candela"].CreateInstance();
            _all_sounds.Add(_put_candle_sound);
            _enlight_torch = assetsLoader.Sounds["accendi_fiamma"].CreateInstance();
            _all_sounds.Add(_enlight_torch);
            _blood_sound = assetsLoader.Sounds["blood"].CreateInstance();
            _all_sounds.Add(_blood_sound);
            _ladder_sound = assetsLoader.Sounds["ladder"].CreateInstance();
            _all_sounds.Add(_ladder_sound);
            _player_falling_sound = assetsLoader.Sounds["fall"].CreateInstance();
            _all_sounds.Add(_player_falling_sound);
            _taurus_starting_charge_sound = assetsLoader.Sounds["taurus_starting_charge"].CreateInstance();
            _all_sounds.Add(_taurus_starting_charge_sound);
            _break_wall_sound = assetsLoader.Sounds["break_wall"].CreateInstance();
            _all_sounds.Add(_break_wall_sound);
            _patrol_seen_you_sound = assetsLoader.Sounds["patrol_seen_you"].CreateInstance();
            _all_sounds.Add(_patrol_seen_you_sound);
        }

        private int RegisterMonsterSound(
            string entryName,
            bool isLooped)
        {
            ++_currentLastSoundAddedIndex;
            var sound = _assetsLoader.Sounds[entryName].CreateInstance();
            sound.IsLooped = isLooped;
            _monster_sounds.Add(sound);
            _all_sounds.Add(sound);

            return _currentLastSoundAddedIndex;
        }

        public void PlayIndexedSound(int Index)
        {
            if (_monster_sounds.Count > Index)
                _monster_sounds[Index].Play();
        }

        public void StopIndexedSound(int Index)
        {
            if (_monster_sounds.Count > Index)
                _monster_sounds[Index].Stop();
        }

        public int RegisterZombieScreamSound()
        {
            return RegisterMonsterSound("zombie_voice", false);
        }
        
        public int RegisterGhostScreamSound()
        {
            // TODO Suono tipo fantasmino
            return RegisterMonsterSound("skeletron_voice", false);
        }

        public int RegisterGoblinScreamSound()
        {
            return RegisterMonsterSound("goblin_voice", false);
        }

        public int RegisterSkeletronScreamSound()
        {
            return RegisterMonsterSound("skeletron_voice", false);
        }

        public int RegisterWerewolfScreamSound()
        {
            return RegisterMonsterSound("wolf_voice", false);
        }

        public int RegisterTaurusScreamSound()
        {
            return RegisterMonsterSound("wolf_voice", false);
        }

        public int RegisterBubuScreamSound()
        {
            return RegisterMonsterSound("skeletron_voice", false);
        }

        public int RegisterZombieAttackSound()
        {
            return RegisterMonsterSound("attack1", true);
        }

        public int RegisterGhostAttackSound()
        {
            return RegisterMonsterSound("attack1", true);
        }

        public int RegisterWerewolfAttackSound()
        {
            return RegisterMonsterSound("attack2", true);
        }

        public int RegisterTaurusAttackSound()
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

        public  int RegisterBubuAttackSound()
        {
            return RegisterMonsterSound("attack1", true);
        }


        public float ComputeIndexSoundsVolume(int screamIndex, int attackIndex, float Distance)
        {
            float v = GetVolumeFromDistance(Distance);
            if (_monster_sounds.Count > screamIndex)
                _monster_sounds[screamIndex].Volume = v;
            if (_monster_sounds.Count > attackIndex)
                _monster_sounds[attackIndex].Volume = v;

            return v;
        }

        private float GetVolumeFromDistance(float distance)
        {
            // Scelgo il massimo: 80 pixel per cella, ne prendo 4, 320pixel di distanza => al quadrato 102400
            return distance > 102400 ? 0 : ((distance / -102400) + 1);
        }

        public void PlayPlayerWalking()
        {
            if (_player_walking_sound.State != SoundState.Playing)
                _player_walking_sound.Play();
        }

        public void StopPlayerWalking()
        {
            _player_walking_sound.Stop();
        }

        public void PlayPlayerTorch()
        {
            _player_fire_sound.Play();
        }

        public void StopPlayerTorch()
        {
            _player_fire_sound.Stop();
        }

        public void PlayPutCandle()
        {
            _put_candle_sound.Play();
        }

        public void PlayBlood()
        {
            _blood_sound.Play();
            _player_fire_sound.Stop();
        }

        public void PlayPatrolSeenYou()
        {
            _patrol_seen_you_sound.Play();
        }

        public void PlayBreakWallSound()
        {
            _break_wall_sound.Play();
        }

        public void PlayEnlightTorch()
        {
            _enlight_torch.Play();
        }

        public void PlayLadder()
        {
            StopSounds();
            _ladder_sound.Play();
        }

        public void PlayFall()
        {
            StopSounds();
            _player_falling_sound.Play();
        }

        public void PlayCollectObject()
        {
            _key_found_sound.Play();
        }

        public void PlayTaurusStargingCharge()
        {
            _taurus_starting_charge_sound.Play();
        }

        public void StopSounds()
        {
            foreach (SoundEffectInstance s in _all_sounds)
            {
                s.Stop();
            }
        }

        public void PauseAll()
        {
            foreach (SoundEffectInstance s in _all_sounds)
            {
                if (s.State == SoundState.Playing)
                    s.Pause();
            }
        }

        public void ResumeAll()
        {
            foreach (SoundEffectInstance s in _all_sounds)
            {
                if (s.State == SoundState.Paused)
                    s.Play();
            }
        }
    }
}