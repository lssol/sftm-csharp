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
            public bool InMemoryAdjacentEdges { get; set; }
        }

        private readonly Dictionary<Node, HashSet<Edge>> _nodeToEdges;
        private readonly Parameters                      _params;
        private readonly IEnumerable<Edge>               _edges;
        private readonly Random                          _rand;
        private readonly int                             _nbNodes;
        private          Dictionary<Edge, HashSet<Edge>> _adjacentEdges;

        public Metropolis(Parameters parameters, IEnumerable<Edge> edges, int nbNodes)
        {
            _params        = parameters;
            _nbNodes       = nbNodes;
            _edges         = edges.OrderByDescending(e => e.Cost).ToList(); // Without the ToList(), the edges.foreach(e => ...) changes the reference of e
            _nodeToEdges   = ComputeNodeToEdgesDic();
            _rand          = new Random();
            
            if (_params.InMemoryAdjacentEdges)
                _adjacentEdges = _edges.ToDictionary(e => e, GetAdjacentEdges);
        }

        public List<Edge> Run()
        {
            var currentMatching  = SuggestMatching(new List<Edge>());
            var currentObjective = ComputeObjective(currentMatching);

            var maxObjective = currentObjective;
            var bestMatching = currentMatching;

            Enumerable.Range(0, _params.NbIterations).ForEach(_ =>
            {
                var matching        = SuggestMatching(currentMatching);
                var objective       = ComputeObjective(matching);
                var acceptanceRatio = objective / currentObjective;
                if (!(_rand.NextDouble() <= acceptanceRatio)) return;
                currentMatching  = matching;
                currentObjective = objective;
                if (!(currentObjective > maxObjective)) return;
                maxObjective = currentObjective;
                bestMatching = currentMatching;
            });

            return bestMatching;
        }

        private Dictionary<Node, HashSet<Edge>> ComputeNodeToEdgesDic()
        {
            var nodeToEdges = new Dictionary<Node, HashSet<Edge>>(_nbNodes + 10);
            _edges.ForEach(e =>
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

        private HashSet<Edge> GetAdjacentEdges(Edge edge)
        {
            var result = new HashSet<Edge>();
            if (edge.Source != null)
                result.UnionWith(_nodeToEdges[edge.Source]);
            if (edge.Target != null)
                result.UnionWith(_nodeToEdges[edge.Target]);

            return result;
        }

        private double ComputeObjective(IEnumerable<Edge> matching)
        {
            var cost = matching.Average(e => e.Cost) ?? 0;
            return Math.Exp(-_params.Lambda * cost);
        }

        private List<Edge> SuggestMatching(List<Edge> previousMatching)
        {
            var newMatching = new List<Edge>(_nbNodes + 10);

            var edges    = new LinkedList<Edge>();
            var edgesDic = new Dictionary<Edge, LinkedListNode<Edge>>(_edges.Count());
            _edges.ForEach(e => { edgesDic.Add(e, edges.AddLast(e)); });

            void KeepEdge(Edge edge)
            {
                newMatching.Add(edge);
                var adjacentEdges = _adjacentEdges?[edge] ?? GetAdjacentEdges(edge);
                adjacentEdges.ForEach(e => edges.RemoveIfExist(edgesDic[e]));
            }

            var p = _rand.Next(0, previousMatching.Count);
            Enumerable.Range(0, p).ForEach(i => KeepEdge(previousMatching[i]));

            while (edges.Count > 0)
                foreach (var edge in edges)
                {
                    if (_rand.NextDouble() >= _params.Gamma)
                        continue;
                    KeepEdge(edge);
                    break;
                }

            return newMatching;
        }
    }
}