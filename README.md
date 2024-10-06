# Similarity-based Flexible Tree Matching (SFTM)

SFTM is an algorithm allowing to match web two trees. It has been mostly tested on web pages.
To our knowledge, SFTM is the most efficient existing algorithm to match nodes from two websites (c.f. benchmark)

SFTM makes full use of the information contained in the nodes of the trees to match. 
In the case of HTML, it means tags, attributes and their values.

If you want to understand SFTM, please read the [associated scientific paper](https://arxiv.org/abs/2004.12821).
If you use it for academic purposes, please don't forget to cite us.

For a solution in the java exosystem you can also see the [implementation in Kotlin](https://github.com/lssol/sftm_tree_matching) which is deployed as a package in the official MAVEN repository.

## Projects

There are 4 projects:
- `tree-matching-csharp` the algorithm as a c# library
- `tree-matching-csharp.Test` the unit tests for the different parts of the algorithm 
- `tree-matching-csharp.Benchmark` (messy) allows to run different benchmark around the SFTM algorithm (and competitors)
- `tree-matching-csharp.Visualization` (messy) allows to visualize some of the results

## tree-matching-csharp

That's where the algorithm is, written as a c# library.
Structure:
- `DOM.cs` contains methods to transform a webpage `string` into the tree structure SFTM uses
- `FtmCost.cs` exposes the method to compute the FTM cost of a given matching (see FTM paper). 
This is not directly part of the SFTM algorithm but allows to compute a "confidence" metric on each match.
- `InMemoryIndexer.cs` Methods to create an index of a given set of documents that can be queried fast
- `ITreeMatcher`the interface that SFTM implements
- `Metropolis.cs` implementation of the metropolis algorithm applied to SFTM
- `Neighbors.cs` contains utility methods to manipulate list of node's neighbors
- `SftmTreeMatcher.cs` contains the core of the algorithm
- `SimilarityPopagation.cs` contains methods used to propagate the similarity
- `Utils.cs` general utilities
- `Types.cs` Contains definitions for a node and an edge

You can refer to the paper for a more theoretical explanation.

#### Usage

The SFTM algorithm itself takes two websites of type `Node` (in `Types.cs`).

```c#
TreeMatcherResponse matching = await _matcher.MatchTrees(sourceNodes, targetNodes);
```

Where `sourceNodes` and `targetNodes` are of types `IEnumerable<Node>`.
To directly match two websites without having to transform the website into `IEnumerable<Node>`, you can use the `DOM` methods:

```c#
IEnumerable<Node> sourceNodes = await DOM.WebpageToTree(source);
IEnumerable<Node> targetNodes = await DOM.WebpageToTree(target);
```

A direct example of such usage can be found in:
`tree-matching-csharp.Test/TreeMatchingTest.cs/TestTreeMatching`

Serializing the results of the matching to interact with external applications can be challenging.
Let us say you want to create an API out of SFTM that match two websites, you have two options:
1. Return the matching as a list of tuples `(id1, id2)` where `id1/2` are the ids of the nodes when traversing it the same order (e.g. post traversal)
2. Require input websites to contain a `signature` attribute for each node (whose value uniquely identifies the node). 
Then return a list of tuples `(signature_1, signature_2)`.
We found solution 1. to be quite impractical since different parsers often don't exactly parse the nodes the same way which makes order-based ids very fragile.
An similar solution to 2. has been implemented in:
`tree-matching-csharp.Visualization/Controllers/HomeController.cs/MatchWebsites`

