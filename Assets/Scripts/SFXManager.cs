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
    public AudioClip lichWins;
    public AudioClip kingWins;
    public AudioClip draw;
    public AudioClip move;

    private void OnEnable()
    {
        UnitManager.MoveTile += OnMove;
        UnitManager.ABeatsB += OnABeatsB;
        UnitManager.Draw += OnDraw;
    }

    private void OnDisable()
    {
        UnitManager.MoveTile -= OnMove;
        UnitManager.ABeatsB -= OnABeatsB;
        UnitManager.Draw -= OnDraw;
    }

    private void OnABeatsB(UnitManager a, UnitManager b) {
        switch (a.State) {
            case DiceState.King: PlaySound(kingWins); break;
            case DiceState.Lich: PlaySound(lichWins); break;
            case DiceState.Rock: PlaySound(rockWins); break;
            case DiceState.Paper: PlaySound(paperWins); break;
            case DiceState.Scissors: PlaySound(scissorsWins); break;
        }
    }

    private void OnDraw(UnitManager a, UnitManager b) {
        PlaySound(draw);
    }

    private void OnMove()
    {
        PlaySound(move);
    }

    private void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
