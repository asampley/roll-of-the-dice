using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public AudioSource audioSource;

    //AudioClips
    public AudioClip rockWins;
    public AudioClip paperWins;
    public AudioClip scissorsWins;
    public AudioClip nuclearWins;
    public AudioClip kingWins;
    public AudioClip draw;
    public AudioClip move;


    private void OnEnable()
    {
        EventManager.AddListener("Move", _onMove);
        foreach (var val in Enum.GetValues(typeof(DiceState))) {
            EventManager.AddListener("AllyRockBeats" + val, _onRockWin);
            EventManager.AddListener("Ally" + val + "BeatenByRock", _onRockWin);
        }
        foreach (var val in Enum.GetValues(typeof(DiceState))) {
            EventManager.AddListener("AllyPaperBeats" + val, _onPaperWin);
            EventManager.AddListener("Ally" + val + "BeatenByPaper", _onPaperWin);
        }
        foreach (var val in Enum.GetValues(typeof(DiceState))) {
            EventManager.AddListener("AllyScissorsBeats" + val, _onScissorsWin);
            EventManager.AddListener("Ally" + val + "BeatenByScissors", _onScissorsWin);
        }
        foreach (var val in Enum.GetValues(typeof(DiceState))) {
            EventManager.AddListener("AllyNuclearBeats" + val, _onNuclearWin);
            EventManager.AddListener("Ally" + val + "BeatenByNuclear", _onNuclearWin);
        }
        foreach (var val in Enum.GetValues(typeof(DiceState))) {
            EventManager.AddListener("AllyKingBeats" + val, _onKingWin);
            EventManager.AddListener("Ally" + val + "BeatenByKing", _onKingWin);
        }
        EventManager.AddListener("Draw", _onDraw);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("Move", _onMove);
        foreach (var val in Enum.GetValues(typeof(DiceState))) {
            EventManager.RemoveListener("AllyRockBeats" + val, _onRockWin);
            EventManager.RemoveListener("Ally" + val + "BeatenByRock", _onRockWin);
        }
        foreach (var val in Enum.GetValues(typeof(DiceState))) {
            EventManager.RemoveListener("AllyPaperBeats" + val, _onPaperWin);
            EventManager.RemoveListener("Ally" + val + "BeatenByPaper", _onPaperWin);
        }
        foreach (var val in Enum.GetValues(typeof(DiceState))) {
            EventManager.RemoveListener("AllyScissorsBeats" + val, _onScissorsWin);
            EventManager.RemoveListener("Ally" + val + "BeatenByScissors", _onScissorsWin);
        }
        foreach (var val in Enum.GetValues(typeof(DiceState))) {
            EventManager.RemoveListener("AllyNuclearBeats" + val, _onNuclearWin);
            EventManager.RemoveListener("Ally" + val + "BeatenByNuclear", _onNuclearWin);
        }
        foreach (var val in Enum.GetValues(typeof(DiceState))) {
            EventManager.RemoveListener("AllyKingBeats" + val, _onKingWin);
            EventManager.RemoveListener("Ally" + val + "BeatenByKing", _onKingWin);
        }
        EventManager.RemoveListener("Draw", _onDraw);
    }

    private void _onKingWin() {
        PlaySound(kingWins);
    }

    private void _onNuclearWin() {
        PlaySound(nuclearWins);
    }

    private void _onRockWin() {
        PlaySound(rockWins);
    }

    private void _onPaperWin() {
        PlaySound(paperWins);
    }

    private void _onScissorsWin() {
        PlaySound(scissorsWins);
    }

    private void _onDraw()
    {
        PlaySound(draw);
    }

    private void _onMove()
    {
        PlaySound(move);
    }

    private void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
