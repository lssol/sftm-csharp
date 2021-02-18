using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using AngleSharp.Common;
using MoreLinq;
using Nest;

namespace tree_matching_csharp
{
    public class Metropolis
    {
        public class Parameters
        {
            public float Gamma        { get; set; }
            public float Lambda       { get; set; }
            public int   NbIterations { get; set; }
            public bool MetropolisNormalisation { get; set; }
        }

        private readonly Dictionary<Node, HashSet<Edge>>        _nodeToEdges;
        private readonly Parameters                             _params;
        private readonly IEnumerable<Edge>                      _edges;
        private readonly Random                                 _rand;
        private readonly int                                    _nbNodes;
        private readonly int                                    _maxNeighbors;
        private          Dictionary<Edge, LinkedListNode<Edge>> _linkedListNodes;
        private List<Edge> _newMatching;
        private LinkedList<Edge> _linkedEdges;
        private HashSet<Edge> _adjacentEdges;

        public Metropolis(Parameters parameters, IEnumerable<Edge> edges, int nbNodes, int maxNeighbors)
        {
            _params          = parameters;
            _nbNodes         = nbNodes;
            _maxNeighbors    = maxNeighbors;
            _adjacentEdges = new HashSet<Edge>(_maxNeighbors *2);
            _edges = edges.ToList();
            _nodeToEdges     = ComputeNodeToEdgesDic();
            _edges           = _edges.OrderByDescending(edge => _params.MetropolisNormalisation ? edge.NormalizedScore : edge.Score).ToList();
            _linkedListNodes = new Dictionary<Edge, LinkedListNode<Edge>>(_edges.Count());
            _rand            = new Random();
            _newMatching = new List<Edge>(_nbNodes + 10);
            _linkedEdges = new LinkedList<Edge>();
        }

        public List<Edge> Run()
        {
            var currentMatching  = SuggestMatching(new List<Edge>());
            var currentObjective = ComputeObjective(currentMatching);

            var maxObjective = currentObjective;
            var bestMatching = currentMatching;

            for (var i = 0; i < _params.NbIterations; i++)
            {
                var matching        = SuggestMatching(currentMatching);
                var objective       = ComputeObjective(matching);
                var acceptanceRatio = objective / currentObjective;
                if (!(_rand.NextDouble() <= acceptanceRatio)) continue;
                currentMatching  = matching;
                currentObjective = objective;
                if (!(currentObjective > maxObjective)) continue;
                maxObjective = currentObjective;
                bestMatching = currentMatching;
            }

            return bestMatching;
        }

        private Dictionary<Node, HashSet<Edge>> ComputeNodeToEdgesDic()
        {
            var nodeToEdges = new Dictionary<Node, HashSet<Edge>>(_nbNodes + 10);
            MoreEnumerable.ForEach(_edges, e =>
            {
                if (e.Source == null)
                    nodeToEdges.PushAt(e.Target, e);
                else if (e.Target == null)
                    nodeToEdges.PushAt(e.Source, e);
                else
                    nodeToEdges.PushAt(e.Source, e).PushAt(e.Target, e);
            });

            return nodeToEdges;
        }

        private List<Edge> GetAdjacentEdges(Edge edge)
        {
            _adjacentEdges.Clear();
            if (edge.Source != null)
                _adjacentEdges.UnionWith(_nodeToEdges[edge.Source]);
            if (edge.Target != null)
                _adjacentEdges.UnionWith(_nodeToEdges[edge.Target]);

            _adjacentEdges.Remove(edge);
            return new List<Edge>(_adjacentEdges);
        }

        private double ComputeObjective(IEnumerable<Edge> matching)
        {
            var score = matching.Average(e => _params.MetropolisNormalisation ? e.NormalizedScore : e.Score);
            return score;
        }

        private List<Edge> SuggestMatching(List<Edge> previousMatching)
        {
            _newMatching.Clear();
            _linkedEdges.Clear();
            _linkedListNodes.Clear();
            foreach (var edge in _edges)
                _linkedListNodes.Add(edge, _linkedEdges.AddLast(edge));

            void KeepEdge(Edge edge)
            {
                _newMatching.Add(edge);
                var adjacentEdges = GetAdjacentEdges(edge); // _inMemoryAdjacentEdges[edge];
                foreach (var e in adjacentEdges) _linkedEdges.RemoveIfExist(_linkedListNodes[e]);
                _linkedEdges.Remove(_linkedListNodes[edge]);
            }

            var p = _rand.Next(0, previousMatching.Count);
            Enumerable.Range(0, p).ForEach(i => KeepEdge(previousMatching[i]));

            while (_linkedEdges.Count > 0)
                foreach (var edge in _linkedEdges)
                {
                    if (_rand.NextDouble() >= _params.Gamma)
                        continue;
                    KeepEdge(edge);
                    break;
                }

            return _newMatching;
        }
    }
}