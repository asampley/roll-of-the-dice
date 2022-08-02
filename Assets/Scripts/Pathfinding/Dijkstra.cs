using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// iterative dijkstra that can be called multiple times, to fill out the graph
// simple implementation with cost of 1 between nodes
public class Dijkstra<V> where V : IEquatable<V>
{
    private readonly Func<V, IEnumerable<V>> _adjacent;
    private readonly Dictionary<V, int> _cost = new();
    private readonly PriorityQueue<V, int> _open = new();

    public Dijkstra(Func<V, IEnumerable<V>> adjacent, IEnumerable<V> goals) {
        this._adjacent = adjacent;

        foreach (V goal in goals) {
            this._cost.TryAdd(goal, 0);
            this._open.Enqueue(goal, 0);
        }
    }

    public int Cost(V vertex) {
        while (!_cost.ContainsKey(vertex)) {
            // return if there's nothing left
            if (_open.Count == 0) return Int32.MaxValue;

            V current = _open.Dequeue();

            if (current.Equals(vertex)) {
                break;
            }

            int currentCost = _cost[current];

            foreach (V adjacent in _adjacent(current)) {
                int cost = currentCost + 1;

                if (!_cost.ContainsKey(adjacent)) {
                    _cost.Add(adjacent, cost);
                    _open.Enqueue(adjacent, cost);
                } else if (cost < _cost[adjacent]) {
                    _cost[adjacent] = cost;
                    _open.Enqueue(adjacent, cost);
                }
            }
        }

        return _cost[vertex];
    }

    public bool TryCost(V vertex, out int cost) {
        return _cost.TryGetValue(vertex, out cost);
    }
}
