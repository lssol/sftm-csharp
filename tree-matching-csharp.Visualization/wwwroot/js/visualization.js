const settings = {
    url: 'http://localhost:4678/bolzano'
}
const generateId = () => {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        let r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
};

class Cyto {
    constructor() {
        const t2Color = '#58a658';
        const selectedColor = '#586ea6';
        this.Instance = cytoscape({
            container: document.getElementById('cy'), // container to render in
            style: [
                {
                    selector: 'node',
                    style: {
                        'label': 'data(label)',
                    }
                },
                {
                    selector: '[?t2]',
                    style: {
                        'label': 'data(label)',
                        'line-color': t2Color,
                        'background-color': t2Color
                    }
                },
                {
                    selector: 'node:selected',
                    style: {
                        'background-color': selectedColor
                    }
                },
                {
                    selector: 'edge:selected',
                    style: {
                        'line-color': selectedColor,
                        'line-style': 'solid',
                        'width': 1.3
                    }
                },
                {
                    selector: "edge[?matchingEdge]",
                    style: {
                        'line-color': el => el.data('cost').total == 0 ? 'green' : 'red',
                        'width': el => 0.5 + 0.1 * Math.log(1 + el.data('cost').total),
                        'line-style': 'dashed',
                        'label': el => el.data('cost').total,
                        'font-size': '6'
                    }
                },
            ]
        })
    }
    LoadNewData(elements) {
        this.Instance.json({elements: elements})
        this.Instance.elements().not('edge[?matchingEdge]').layout({
            name: 'dagre'
        }).run();
    }
    Select(t) {
        const options = new Map([['t1', false], ['t2', true]]);
        if (!options.has(t))
            throw "Unknown tree option"
        
        if (options.get(t))
            this.Instance.$('[?t2]').select()
        else
            this.Instance.$('[!t2]').select()
    }
}

const getData = async (index, matcher) => {
    const result = await axios.get(settings.url, {
        params: {
            index: index,
            matcherName: matcher
        }
    })
    return result.data
}
const dataToElements = async (graphData) => {
    const {nodes: graph1, map: map1} = compoundToGraph(graphData.tree1)
    const {nodes: graph2, map: map2} = compoundToGraph(graphData.tree2, true)

    const matches = graphData.matching.matches

    const matchingEdges = matches
        .filter(m => m.id1 && m.id2)
        .map(m => matchingToEdge(map1.get(m.id1), map2.get(m.id2), m.cost))
    
    return [...graph1, ...graph2, ...matchingEdges]
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
const compoundToGraph = (elements, t2) => {
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
                t2: t2
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
                target: map.get(node.data.id),
                t2: t2
            }
        }

        nodes.push(edgeDef)
    })

    return {nodes, map}
}

(async () => {
    const cy = new Cyto()
    const load = async () => {
        const index = document.querySelector("#index").value
        const matcher = document.querySelector("#matcher").value
        
        if (index == null || matcher == null) {
            console.warn("The form hasn't been filled")
            return
        }
        
        let graphData = await getData(index, matcher)
        let elements = await dataToElements(graphData)
        cy.LoadNewData(elements)
        
        document.querySelector("#ftmCost").innerHTML = `relCost = ${graphData.matching.relativeCost.toFixed(2)}, anc = ${graphData.matching.cost.ancestry}, sib = ${graphData.matching.cost.ancestry}, rel = ${graphData.matching.cost.relabel}, nomatch = ${graphData.matching.cost.noMatch}`
    }
    
    load()
    document.querySelector("#send").addEventListener("click", load)
    document.querySelector("#t1").addEventListener("click", () => cy.Select('t1'))
    document.querySelector("#t2").addEventListener("click", () => cy.Select('t2'))
})()

