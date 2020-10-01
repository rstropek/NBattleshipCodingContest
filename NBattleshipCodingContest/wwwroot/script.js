(async () => {
    // Get HTML elements by ID
    const player1Select = document.getElementById('player1Select');
    const player2Select = document.getElementById('player2Select');
    const start = document.getElementById('start');
    const p1table = document.getElementById('p1table');
    const p2table = document.getElementById('p2table');
    const winner = document.getElementById('winner');
    const next = document.getElementById('next');
    const prev = document.getElementById('prev');
    const first = document.getElementById('first');
    const last = document.getElementById('last');

    next.onclick = onNext;
    prev.onclick = onPrev;
    first.onclick = onFirst;
    last.onclick = onLast;

    // Create an option element for a select
    function createOption(value, text, selected) {
        var opt = document.createElement('option');
        if (selected) {
            opt.setAttribute('selected', '');
        }

        opt.appendChild(document.createTextNode(text));
        opt.value = value;
        return opt;
    }

    // Create a table with 10 x 10 squares
    function createBattleshipTable() {
        const tbody = document.createElement('tbody');
        for (var row = 0; row < 10; row++) {
            const rowElem = document.createElement('tr');
            for (var col = 0; col < 10; col++) {
                const colElem = document.createElement('td');
                rowElem.appendChild(colElem);
            }

            tbody.appendChild(rowElem);
        }

        return tbody;
    }

    async function runGame() {
        if (p1table.childNodes.length > 0) {
            p1table.removeChild(p1table.childNodes[0]);
        }
        p1table.appendChild(createBattleshipTable());
        p1table.hidden = false;

        if (p2table.childNodes.length > 0) {
            p2table.removeChild(p2table.childNodes[0]);
        }
        p2table.appendChild(createBattleshipTable());
        p2table.hidden = false;

        const p1 = parseInt(player1Select.value);
        const p2 = parseInt(player2Select.value);
        const gameResponse = await fetch('/api/games/start', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                player1Index: p1,
                player2Index: p2
            })
        });
        const result = await gameResponse.json();

        next.hidden = false;
        prev.hidden = false;
        first.hidden = false;
        last.hidden = false;

        return result;
    }

    let step;
    let gameResult;

    function onFirst() {
        if (gameResult) {
            step = 0;
            processGameResult();
        }
    }

    function onNext() {
        if (gameResult && step < gameResult.log.length) {
            step += 2;
            processGameResult();
        }
    }

    function onPrev() {
        if (gameResult && step > 0) {
            step -= 2;
            processGameResult();
        }
    }

    function onLast() {
        if (gameResult) {
            step = gameResult.log.length;
            processGameResult();
        }
    }

    function processGameResult() {
        const ptables = [p1table, p2table];
        const boards = [gameResult.board2, gameResult.board1];
        for (let j = 0; j < 2; j++) {
            for (let i = 0; i < 100; i++) {
                const colIx = i % 10;
                const rowIx = Math.floor(i / 10);
                const td = ptables[j].childNodes[0].childNodes[rowIx].childNodes[colIx];
                td.className = '';

                if (boards[j][i] === 'S') {
                    td.classList.add('ship');
                }

                if (boards[j][i] === 'S') {
                    td.classList.add('ship');
                }
            }
        }

        for (let i = 0; i < step; i++) {
            const log = gameResult.log[i];
            const colIx = log.location.charCodeAt(0) - 'A'.charCodeAt(0);
            const rowIx = parseInt(log.location.substring(1)) - 1;
            const pTable = i % 2 === 0 ? p1table : p2table;
            const td = pTable.childNodes[0].childNodes[rowIx].childNodes[colIx];
            td.classList.add(log.shotResult === 'Water' ? 'water' : 'hit');
        }

        winner.innerText = `The winner is player ${gameResult.winner}`;
    }

    // Get all players and fill selects with options
    const playersResponse = await fetch('/api/players');
    const players = await playersResponse.json();
    for (let i = 0; i < players.length; i++) {
        player1Select.appendChild(createOption(i, players[i], i === 0));
        player2Select.appendChild(createOption(i, players[i], i === 1));
    }

    // Enable start button because now we are ready
    start.disabled = false;

    start.onclick = async () => {
        gameResult = await runGame();
        step = gameResult.log.length;
        processGameResult();
    };
})();
