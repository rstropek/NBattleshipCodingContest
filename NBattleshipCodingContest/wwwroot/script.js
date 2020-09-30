(async () => {
    const player1Select = document.getElementById('player1Select');
    const player2Select = document.getElementById('player2Select');
    const start = document.getElementById('start');
    const p1table = document.getElementById('p1table');
    const p2table = document.getElementById('p2table');

    function createOption(value, text) {
        var opt = document.createElement('option');
        opt.appendChild(document.createTextNode(text));
        opt.value = value;
        return opt;
    }

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

    const playersResponse = await fetch('/api/players');
    const players = await playersResponse.json();
    for (let i = 0; i < players.length; i++) {
        player1Select.appendChild(createOption(i, players[i]));
        player2Select.appendChild(createOption(i, players[i]));
    }

    start.disabled = false;
    start.onclick = async () => {
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
        const gameResponse = await fetch('/api/tournaments', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                player1Index: p1,
                player2Index: p2
            }) 
        });
        const gameResult = await gameResponse.json();

        for (let i = 0; i < gameResult.length; i++)  {
            const colIx = gameResult[i].location.charCodeAt(0) - 'A'.charCodeAt(0);
            const rowIx = parseInt(gameResult[i].location.substring(1)) - 1;
            const pTable = gameResult[i].shooter === p1 ? p1table : p2table;
            const td = pTable.childNodes[0].childNodes[rowIx].childNodes[colIx];
            td.classList.add(gameResult[i].shotResult === 'Water' ? 'water' : 'hit');
        }
    };
})();
