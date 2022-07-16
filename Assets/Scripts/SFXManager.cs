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
    public AudioClip draw;
    public AudioClip move;


    private void OnEnable()
    {
        EventManager.AddListener("Move", _onMove);
        EventManager.AddListener("AllyRockBeatsScissors", _onAllyRockBeatsScissors);
        EventManager.AddListener("AllyRockBeatenByPaper", _onAllyRockBeatenByPaper);
        EventManager.AddListener("AllyPaperBeatsRock", _onAllyPaperBeatsRock);
        EventManager.AddListener("AllyPaperBeatenByScissors", _onAllyPaperBeatenByScissors);
        EventManager.AddListener("AllyScissorsBeatsPaper", _onAllyScissorsBeatsPaper);
        EventManager.AddListener("AllyScissorsBeatenByRock", _onAllyScissorsBeatenByRock);
        EventManager.AddListener("Draw", _onDraw);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("Move", _onMove);
        EventManager.RemoveListener("AllyRockBeatsScissors", _onAllyRockBeatsScissors);
        EventManager.RemoveListener("AllyRockBeatenByPaper", _onAllyRockBeatenByPaper);
        EventManager.RemoveListener("AllyPaperBeatsRock", _onAllyPaperBeatsRock);
        EventManager.RemoveListener("AllyPaperBeatenByScissors", _onAllyPaperBeatenByScissors);
        EventManager.RemoveListener("AllyScissorsBeatsPaper", _onAllyScissorsBeatsPaper);
        EventManager.RemoveListener("AllyScissorsBeatenByRock", _onAllyScissorsBeatenByRock);
        EventManager.RemoveListener("Draw", _onDraw);
    }

    private void _onAllyRockBeatsScissors()
    {
        PlaySound(rockWins);
    }

    private void _onAllyRockBeatenByPaper()
    {
        PlaySound(paperWins);
    }

    private void _onAllyPaperBeatsRock()
    {
        PlaySound(paperWins);
    }

    private void _onAllyPaperBeatenByScissors()
    {
        PlaySound(scissorsWins);
    }

    private void _onAllyScissorsBeatsPaper()
    {
        PlaySound(scissorsWins);
    }

    private void _onAllyScissorsBeatenByRock()
    {
        PlaySound(rockWins);
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
