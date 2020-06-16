const settings = {
    url: 'http://localhost:4678/bolzano'
}

const generateId = () => {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        let r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
};

const getData = async () => {
    const result = await axios.get(settings.url)
    return result.data
}

const matchingToEdge = (id1, id2, cost) => {
    return {
        group:'edges',
        data: {
            id: generateId(),
            source: id1,
            target: id2,
            matchingEdge: true,
            cost: cost
        }
    }
}

const compoundToGraph = elements => {
    const map = new Map()
    elements.forEach(node => {map.set(node.data.id, generateId())})

    const nodes = []
    elements.forEach(node => {
        const nodeDef = {
            group: 'nodes',
            data: {
                id: map.get(node.data.id),
                value: node.data.value,
                label: node.data.label,
            }
        }
        nodes.push(nodeDef)

        if (!node.data.parent)
            return

        if (!map.get(node.data.parent) || !map.get(node.data.id))
            throw 'no source'

        const edgeDef = {
            group: 'edges',
            data: {
                id: generateId(),
                source: map.get(node.data.parent),
                target: map.get(node.data.id)
            }
        }

        nodes.push(edgeDef)
    })

    return {nodes, map}
}

(async () => {
    const graphData = await getData()
    const {nodes: graph1, map: map1} = compoundToGraph(graphData.tree1)
    const {nodes: graph2, map: map2} = compoundToGraph(graphData.tree2)

    const matches = graphData.matching.matches

    const matchingEdges = matches
        .filter(m => m.id1 && m.id2)
        .map(m => matchingToEdge(map1.get(m.id1), map2.get(m.id2), m.cost))

    const cy = cytoscape({
        container: document.getElementById('cy'), // container to render in
        elements: [...graph1, ...graph2, ...matchingEdges],
        style: [
            {
                selector: 'node',
                style: {
                    'label': 'data(label)',
                }
            },
            {
                selector: "edge[?matchingEdge]",
                style: {
                    'line-color': 'red',
                    'width': 0.6,
                    'line-style': 'dashed',
                    'label': 'data(cost)',
                    'font-size': '4'
                }
            },
        ]
    })
    cy.elements().not('edge[?matchingEdge]').layout({
        name: 'dagre'
    }).run();
})()
