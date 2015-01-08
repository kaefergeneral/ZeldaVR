using UnityEngine;
using System.Collections;
using Immersio.Utility;


public class Music : Singleton<Music>
{

    public AudioClip intro, overworld_open, overworld_loop, labyrinth, deathMountain, ending;

    public void PlayIntro() { audio.loop = true; Play(intro); }
    public void PlayOverworld() { PlayOpeningThenLoop(overworld_open, overworld_loop); }
    public void PlayLabyrinth() { audio.loop = true; Play(labyrinth); }
    public void PlayDeathMountain() { audio.loop = true; Play(deathMountain); }
    public void PlayEnding() { audio.loop = false; Play(ending); }

    public void Play(AudioClip clip, ulong delay = 0) 
    {
        if (!_isEnabled) { return; }
        if (audio.clip == clip && audio.isPlaying) { return; }
        audio.clip = clip; 
        audio.Play(delay); 
    }
    public void PlayOpeningThenLoop(AudioClip openingClip, AudioClip loopClip) 
    {
        if (!_isEnabled) { return; }
        if (audio.isPlaying && (audio.clip == openingClip || audio.clip == loopClip)) { return; }

        audio.loop = false;
        Play(openingClip);

        StartCoroutine("WaitThenPlay", loopClip);
    }
    public IEnumerator WaitThenPlay(AudioClip loopClip)
    {
        while (IsPlaying) { yield return new WaitForSeconds(0.01f); }

        audio.loop = true;
        Play(loopClip);
    }
    public void Stop() { audio.Stop(); StopCoroutine("WaitThenPlay"); }
    public void Pause() { audio.Pause(); }
    public void Resume() { audio.Play(); }

    public bool IsPlaying { get { return audio.isPlaying; } }
    public AudioClip ActiveSong { get { return audio.clip; } }


    bool _isEnabled = true;
    public bool IsEnabled
    {
        get { return _isEnabled; }
        set
        {
            _isEnabled = value;
            if (_isEnabled)
            {
                PlayAppropriateMusic();
            }
            else
            {
                Stop();
            }
        }
    }

    public void ToggleEnabled()
    {
        IsEnabled = !IsEnabled;
    }

    void OnLevelWasLoaded(int level)
    {
        PlayAppropriateMusic();
    }


    public void PlayAppropriateMusic()
    {
        if (!_isEnabled) { return; }

        WorldInfo w = WorldInfo.Instance;
        if (w.IsTitleScene)
        {
            PlayIntro();
        }
        else if (w.IsOverworld)
        {
            PlayOverworld(); 
        }
        else if (w.IsInDungeon)
        {
            if (w.DungeonNum == 9)
            {
                PlayDeathMountain();
            }
            else
            {
                PlayLabyrinth();
            }
        }
    }

}