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
        EventManager.AddListener("Move", _onMove);
        DieManager.ABeatsB += OnABeatsB;
        DieManager.Draw += OnDraw;
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("Move", _onMove);
        DieManager.ABeatsB -= OnABeatsB;
        DieManager.Draw -= OnDraw;
    }

    private void OnABeatsB(DieManager a, DieManager b) {
        switch (a.state) {
            case DiceState.King: PlaySound(kingWins); break;
            case DiceState.Lich: PlaySound(lichWins); break;
            case DiceState.Rock: PlaySound(rockWins); break;
            case DiceState.Paper: PlaySound(paperWins); break;
            case DiceState.Scissors: PlaySound(scissorsWins); break;
        }
    }

    private void OnDraw(DieManager a, DieManager b) {
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
