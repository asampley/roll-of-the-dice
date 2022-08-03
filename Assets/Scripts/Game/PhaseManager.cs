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
    TileEffects,
}

public enum PhaseStepResult {
    // it will be removed from the current state listeners
    Done,
    // you can transition or push states, but if there is no transition it will run again
    Passive,
    // you can push new states, but you shouldn't transition
    // this suggests you don't need to push a new state, as nothing's changed
    Unchanged,
    // you can push new states, but you shouldn't transition
    // you should push new states as something has changed
    Changed,
}

public interface PhaseListener {
    string name { get; }
    MonoBehaviour Self { get; }

    // called once when the phase is entered
    // returns a result identifying whether it is listening or not
    PhaseStepResult OnPhaseEnter(Phase phase);

    // called when a phase is resumed after having things pushed onto the stack
    void OnPhaseResume(Phase phase) {
        return;
    }

    // async function that is called again once all listeners have completed a step in the phase
    // only called if the item has been added to phase processing
    UniTask<PhaseStepResult> OnPhaseStep(Phase phase, CancellationToken token) {
        return new UniTask<PhaseStepResult>(PhaseStepResult.Done);
    }
}

public class PhaseData {
    public Phase phase;
    public HashSet<PhaseListener> phaseStep = new HashSet<PhaseListener>();
    public PhaseStepResult[] results = new PhaseStepResult[] {};

    public PhaseData(Phase phase) {
        this.phase = phase;
    }

    override public string ToString() {
        return phase + Utilities.EnumerableString(phaseStep);
    }
}

public class PhaseManager {
    public readonly HashSet<PhaseListener> AllPhaseListeners = new();
    private readonly List<PhaseData> phaseStack = new();

    private PhaseData Current {
        get { return phaseStack.Count > 0 ? phaseStack[phaseStack.Count - 1] : null; }
    }

    public Phase? CurrentPhase {
        get { return Current?.phase; }
    }

    public void Clear() {
        phaseStack.Clear();
    }

    public void Transition(Phase phase) {
        if (GameManager.Instance.WinState == Win.None && GameManager.Instance.phaseManager.CurrentPhase != Phase.Setup)
            DataHandler.SaveGameData();
        phaseStack.RemoveAt(phaseStack.Count - 1);

        PhaseData phaseData = new(phase);
        phaseStack.Add(phaseData);

        var listeners = AllPhaseListeners.ToArray();
        var results = listeners.Select(l => l.OnPhaseEnter(phase)).ToArray();

        for (var i = 0; i < listeners.Length; ++i) {
            if (results[i] != PhaseStepResult.Done) {
                AddPhaseProcessing(listeners[i]);
            }
        }

        Current.results = results;

        Logging.LogNotification(("PhaseManager new stack " + Utilities.EnumerableString(phaseStack)).ToString(), LogType.PHASES);
    }

    public void Push(Phase phase) {
        PhaseData phaseData = new(phase);
        phaseStack.Add(phaseData);

        var listeners = AllPhaseListeners.ToArray();
        var results = listeners.Select(l => l.OnPhaseEnter(phase)).ToArray();

        for (var i = 0; i < listeners.Length; ++i) {
            if (results[i] != PhaseStepResult.Done) {
                AddPhaseProcessing(listeners[i]);
            }
        }

        Current.results = results;

        Logging.LogNotification(("PhaseManager new stack " + Utilities.EnumerableString(phaseStack)).ToString(), LogType.PHASES);
    }

    public void Pop() {
        phaseStack.RemoveAt(phaseStack.Count - 1);

        if (CurrentPhase == null) return;

        CleanSet(Current.phaseStep);

        foreach (var listener in Current.phaseStep) {
            listener.OnPhaseResume(CurrentPhase.Value);
        }

        Logging.LogNotification(("PhaseManager new stack " + Utilities.EnumerableString(phaseStack)).ToString(), LogType.PHASES);
    }

    public async UniTask PhaseStep(CancellationToken token) {
        // clean up anyone destroyed
        CleanSet(Current.phaseStep);

        // copy list to protect from manipulation in the middle of processing
        List<PhaseListener> toStep = new(Current.phaseStep);

        List<UniTask<PhaseStepResult>> tasks = new();
        List<CancellationTokenSource> sources = new();

        foreach (var l in toStep) {
            var source = CancellationTokenSource.CreateLinkedTokenSource(
                l.Self.GetCancellationTokenOnDestroy(),
                token
            );

            tasks.Add(SafePhaseStep(l, source.Token));
            sources.Add(source);
        }

        if (tasks.Count > 0) {
            var results = await UniTask.WhenAll(tasks);

            // for each result that is Done, remove it from listeners
            for (int i = results.Length - 1; i >= 0; --i) {
                if (results[i] == PhaseStepResult.Done) {
                    Current.phaseStep.Remove(toStep[i]);
                }
            }

            Current.results = results;
        } else {
            await UniTask.DelayFrame(1, cancellationToken: token);

            Current.results = new PhaseStepResult[] {};
        }

        foreach (var s in sources) {
            s.Dispose();
        }
    }

    private static void CleanSet(HashSet<PhaseListener> set) {
        // clean up anyone destroyed
        set.RemoveWhere(l => l.Self == null);
    }


    public PhaseStepResult[] CurrentPhaseResults() {
        return Current.results;
    }

    private async UniTask<PhaseStepResult> SafePhaseStep(PhaseListener listener, CancellationToken token) {
        try {
            return await listener.OnPhaseStep(Current.phase, token);
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
        Current.phaseStep.Add(listener);
    }

    public int PhaseProcessingCount() {
        return Current.phaseStep.Count;
    }

    public string StackString() {
        return Utilities.EnumerableString(phaseStack);
    }
}
