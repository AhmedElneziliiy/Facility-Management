var qrCanvas = null;

function renderQR(text) {
    var container = document.getElementById('qrPreview');
    var meta = document.getElementById('qrMeta');
    var defaultCode = meta ? meta.getAttribute('data-qr-code') : '';
    container.innerHTML = '';
    document.getElementById('qrLabel').textContent = text || defaultCode;
    if (!text) return;
    QRCode.toCanvas(
        document.createElement('canvas'),
        text,
        { width: 160, margin: 2, color: { dark: '#1a202c', light: '#ffffff' } },
        function (err, canvas) {
            if (!err) { qrCanvas = canvas; container.appendChild(canvas); }
        }
    );
}

function refreshQRPreview(val) { renderQR(val.trim().toUpperCase()); }

function printQR() {
    if (!qrCanvas) return;
    var label = document.getElementById('qrLabel').textContent;
    var meta = document.getElementById('qrMeta');
    var assetName = meta ? meta.getAttribute('data-asset-name') : '';
    var win = window.open('', '_blank');
    win.document.write('<html><body style="text-align:center;font-family:monospace;padding:2rem">');
    win.document.write('<img src="' + qrCanvas.toDataURL() + '" style="width:200px"><br>');
    win.document.write('<strong style="font-size:18px">' + label + '</strong><br>');
    win.document.write('<span style="font-size:14px">' + assetName + '</span>');
    win.document.write('</body></html>');
    win.document.close();
    win.print();
}

async function loadFloors(buildingId) {
    const sel = document.getElementById('floorSelect');
    sel.innerHTML = '<option value="">— No floor —</option>';
    if (!buildingId) return;
    const data = await fetch('/Assets/FloorsFor/' + buildingId).then(r => r.json());
    data.forEach(f => {
        const o = document.createElement('option');
        o.value = f.id; o.textContent = f.label;
        sel.appendChild(o);
    });
}
