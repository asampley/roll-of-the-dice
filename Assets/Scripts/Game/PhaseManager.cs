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
    Fight,
}

public enum PhaseStepResult {
    // it will be removed from the current state listeners
    Done,
    // you can transition or push states, but if there is no transition it will run again
    CanContinue,
    // you can push new states, but you shouldn't transition
    ShouldContinue,
}

public interface PhaseListener {
    string name { get; }
    MonoBehaviour Self { get; }

    // called once when the phase is entered
    bool OnPhaseEnter(Phase phase);

    // async function that is called again once all listeners have completed a step in the phase
    // only called if the item has been added to phase processing
    UniTask<PhaseStepResult> OnPhaseUpdate(Phase phase, CancellationToken token);
}

public class PhaseData {
    public Phase phase;
    public HashSet<PhaseListener> phaseUpdate = new HashSet<PhaseListener>();
    public PhaseStepResult[] results = new PhaseStepResult[] {};

    public PhaseData(Phase phase) {
        this.phase = phase;
    }

    override public string ToString() {
        return phase + Utilities.EnumerableString(phaseUpdate);
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

    public void Clear() {
        while (phaseStack.Count > 0) {
            Pop();
        }
    }

    public void Transition(Phase phase) {
        phaseStack.RemoveAt(phaseStack.Count - 1);

        PhaseData phaseData = new PhaseData(phase);
        phaseStack.Add(phaseData);

        foreach (var listener in AllPhaseListeners) {
            if (listener.OnPhaseEnter(phase)) {
                AddPhaseProcessing(listener);
            }
        }

        Debug.Log("PhaseManager new stack " + Utilities.EnumerableString(phaseStack));
    }

    public void Push(Phase phase) {
        PhaseData phaseData = new PhaseData(phase);
        phaseStack.Add(phaseData);

        foreach (var listener in AllPhaseListeners) {
            if (listener.OnPhaseEnter(phase)) {
                AddPhaseProcessing(listener);
            }
        }

        Debug.Log("PhaseManager new stack " + Utilities.EnumerableString(phaseStack));
    }

    public void Pop() {
        phaseStack.RemoveAt(phaseStack.Count - 1);

        Debug.Log("PhaseManager new stack " + Utilities.EnumerableString(phaseStack));
    }

    public async UniTask PhaseUpdate() {
        // clean up anyone destroyed
        Current.phaseUpdate.RemoveWhere(l => l.Self == null);

        // copy list to protect from manipulation in the middle of processing
        List<PhaseListener> toUpdate = new List<PhaseListener>(Current.phaseUpdate);

        List<UniTask<PhaseStepResult>> tasks = new List<UniTask<PhaseStepResult>>();

        foreach (var l in toUpdate) {
            tasks.Add(SafePhaseUpdate(l, l.Self.GetCancellationTokenOnDestroy()));
        }

        if (tasks.Count > 0) {
            var results = await UniTask.WhenAll(tasks);

            // for each result that is Done, remove it from listeners
            for (int i = results.Length - 1; i >= 0; --i) {
                if (results[i] == PhaseStepResult.Done) {
                    Current.phaseUpdate.Remove(toUpdate[i]);
                }
            }

            Current.results = results;
        } else {
            await UniTask.DelayFrame(1);

            Current.results = new PhaseStepResult[] {};
        }
    }

    public PhaseStepResult[] CurrentPhaseResults() {
        return Current.results;
    }

    private async UniTask<PhaseStepResult> SafePhaseUpdate(PhaseListener listener, CancellationToken token) {
        try {
            return await listener.OnPhaseUpdate(Current.phase, token);
        } catch (MissingReferenceException) {
            return PhaseStepResult.Done;
        } catch (OperationCanceledException e) {
            Debug.Log(e);
            return PhaseStepResult.Done;
        } catch (Exception e) {
            Debug.LogError(e);
            return PhaseStepResult.Done;
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
