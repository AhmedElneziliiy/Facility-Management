function refreshQRPreview(val) {
    val = val.trim().toUpperCase();
    var container = document.getElementById('qrPreview');
    var label = document.getElementById('qrLabel');
    label.style.color = val ? '#2d3748' : '#a0aec0';
    label.textContent = val || 'Enter a QR code';
    container.innerHTML = '';
    if (!val) { container.innerHTML = '<i class="bi bi-qr-code" style="font-size:52px;color:#e2e8f0"></i>'; return; }
    QRCode.toCanvas(
        document.createElement('canvas'),
        val,
        { width: 160, margin: 2, color: { dark: '#1a202c', light: '#ffffff' } },
        function (err, canvas) { if (!err) container.appendChild(canvas); }
    );
}

function genQR() {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
    let code = 'AST-';
    for (let i = 0; i < 6; i++) code += chars[Math.floor(Math.random() * chars.length)];
    var inp = document.getElementById('QRCode');
    inp.value = code;
    refreshQRPreview(code);
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
