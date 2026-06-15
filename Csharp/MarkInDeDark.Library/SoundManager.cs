using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;

namespace MarkInDeDark.Library
{
    public class SoundManager
    {
        private readonly AssetsLoader _assetsLoader;
        private readonly Dictionary<string, SoundEffectInstance> _sounds;

        public SoundManager(AssetsLoader assetsLoader)
        {
            _assetsLoader = assetsLoader;

            _sounds = new Dictionary<string, SoundEffectInstance>();
            _sounds.Add("TaurusStartingCharge", assetsLoader.Sounds["TaurusStartingCharge"].CreateInstance());
            _sounds.Add("PatrolSeenYou", assetsLoader.Sounds["PatrolSeenYou"].CreateInstance());
        }

        private int RegisterMonsterSound(
            string entryName,
            bool isLooped)
        {
            //++_currentLastSoundAddedIndex;
            //var sound = _assetsLoader.Sounds[entryName].CreateInstance();
            //sound.IsLooped = isLooped;
            //_monster_sounds.Add(sound);
            //_all_sounds.Add(sound);

            //return _currentLastSoundAddedIndex;
            return 0;
        }

        //public void PlayIndexedSound(int Index)
        //{
        //    if (_monster_sounds.Count > Index)
        //        _monster_sounds[Index].Play();
        //}

        //public void StopIndexedSound(int Index)
        //{
        //    if (_monster_sounds.Count > Index)
        //        _monster_sounds[Index].Stop();
        //}

        public float ComputeIndexSoundsVolume(int screamIndex, int attackIndex, float Distance)
        {
            //float v = GetVolumeFromDistance(Distance);
            //if (_monster_sounds.Count > screamIndex)
            //    _monster_sounds[screamIndex].Volume = v;
            //if (_monster_sounds.Count > attackIndex)
            //    _monster_sounds[attackIndex].Volume = v;

            //return v;

            return 0f;
        }

        //private float GetVolumeFromDistance(float distance)
        //{
        //    // Scelgo il massimo: 80 pixel per cella, ne prendo 4, 320pixel di distanza => al quadrato 102400
        //    return distance > 102400 ? 0 : ((distance / -102400) + 1);
        //}

        public void Play(string name)
            => _sounds[name].Play();

        public void StopAll()
        {
            foreach (var s in _sounds.Values)
            {
                if (s.State != SoundState.Stopped)
                    s.Stop();
            }
        }

        public void PauseAll()
        {
            foreach (var s in _sounds.Values)
            {
                if (s.State == SoundState.Playing)
                    s.Pause();
            }
        }

        public void ResumeAll()
        {
            foreach (var s in _sounds.Values)
            {
                if (s.State == SoundState.Paused)
                    s.Play();
            }
        }
    }
}