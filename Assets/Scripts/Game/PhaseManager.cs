using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public enum Phase
{
    Setup,
    Player,
    Enemy,
}

public interface PhaseListener {
    string name { get; }
    MonoBehaviour Self { get; }

    // called once when the phase changes
    bool OnPhaseChange(Phase phase);

    // async function that is called again once all listeners have completed a step in the phase
    // only called if the item has been added to phase processing
    UniTask OnPhaseUpdate(Phase phase, CancellationToken token);
}

public class PhaseData {
    public Phase phase;
    public HashSet<PhaseListener> phaseUpdate = new HashSet<PhaseListener>();

    public PhaseData(Phase phase) {
        this.phase = phase;
    }
}

public class PhaseManager {
    public readonly HashSet<PhaseListener> AllPhaseListeners = new HashSet<PhaseListener>();
    private List<PhaseData> phaseStack = new List<PhaseData>();

    private PhaseData Current {
        get { return phaseStack.Count > 0 ? phaseStack[phaseStack.Count - 1] : null; }
    }

    public Phase? CurrentPhase {
        get { return Current?.phase; }
    }

    public void Transition(Phase phase) {
        phaseStack.RemoveAt(phaseStack.Count - 1);

        PhaseData phaseData = new PhaseData(phase);

        foreach (var listener in AllPhaseListeners) {
            if (listener.OnPhaseChange(phase)) {
                phaseData.phaseUpdate.Add(listener);
            }
        }

        phaseStack.Add(phaseData);

        Debug.Log("PhaseManager new stack " + Utilities.EnumerableString(phaseStack.Select(s => s.phase)));
    }

    public void Push(Phase phase) {
        PhaseData phaseData = new PhaseData(phase);

        foreach (var listener in AllPhaseListeners) {
            if (listener.OnPhaseChange(phase)) {
                phaseData.phaseUpdate.Add(listener);
            }
        }

        phaseStack.Add(phaseData);

        Debug.Log("PhaseManager new stack " + Utilities.EnumerableString(phaseStack.Select(s => s.phase)));
    }

    public void Pop() {
        phaseStack.RemoveAt(phaseStack.Count - 1);

        foreach (var listener in AllPhaseListeners) {
            if (listener.OnPhaseChange(Current.phase)) {
                Current.phaseUpdate.Add(listener);
            } else {
                Current.phaseUpdate.Remove(listener);
            }
        }

        Debug.Log("PhaseManager new stack " + Utilities.EnumerableString(phaseStack.Select(s => s.phase)));
    }

    public async UniTask PhaseUpdate() {
        // copy list to protect from manipulation in the middle of processing
        List<PhaseListener> toUpdate = new List<PhaseListener>(Current.phaseUpdate);

        List<UniTask> tasks = new List<UniTask>();

        foreach (var l in toUpdate) {
            tasks.Add(SafePhaseUpdate(l, l.Self.GetCancellationTokenOnDestroy()));
        }

        if (tasks.Count > 0) {
            await UniTask.WhenAll(tasks);
        } else {
            await UniTask.DelayFrame(1);
        }
    }

    private async UniTask SafePhaseUpdate(PhaseListener listener, CancellationToken token) {
        try {
            await listener.OnPhaseUpdate(Current.phase, token);
        } catch (MissingReferenceException) {
            return;
        } catch (OperationCanceledException e) {
            Debug.Log(e);
            return;
        } catch (Exception e) {
            Debug.LogError(e);
            return;
        }
    }

    public void AddPhaseProcessing(PhaseListener listener) {
        Current.phaseUpdate.Add(listener);

        string str = Utilities.EnumerableString(Current.phaseUpdate.Select(e => e.name));
        Debug.Log("Still waiting for " + str);
    }

    // must be a coroutine to remove only after a frame has passed
    public void RemovePhaseProcessing(PhaseListener listener) {
        Current.phaseUpdate.Remove(listener);

        string str = Utilities.EnumerableString(Current.phaseUpdate.Select(e => e.name));
        Debug.Log("Still waiting for " + str);
    }

    public int PhaseProcessingCount() {
        return Current.phaseUpdate.Count;
    }
}
