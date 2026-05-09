var qrMeta = document.getElementById('qrMeta');
var qrCode = qrMeta ? qrMeta.getAttribute('data-qr-code') : '';
var assetName = qrMeta ? qrMeta.getAttribute('data-asset-name') : '';

var qr = new QRCode(document.getElementById('qrcode'), {
    text:         qrCode,
    width:        180,
    height:       180,
    colorDark:    '#1a202c',
    colorLight:   '#ffffff',
    correctLevel: QRCode.CorrectLevel.H
});

function getDataUrl(callback) {
    setTimeout(function () {
        var canvas = document.querySelector('#qrcode canvas');
        if (canvas) {
            callback(canvas.toDataURL('image/png'));
        } else {
            var img = document.querySelector('#qrcode img');
            if (img) callback(img.src);
        }
    }, 300);
}

getDataUrl(function (url) {
    var btn = document.getElementById('qrDownloadBtn');
    btn.href = url;
    btn.style.opacity       = '1';
    btn.style.pointerEvents = 'auto';
});

function printQR() {
    getDataUrl(function (url) {
        var iframe = document.createElement('iframe');
        iframe.style.cssText = 'position:fixed;top:-9999px;left:-9999px;width:1px;height:1px;border:0';
        document.body.appendChild(iframe);
        var doc = iframe.contentDocument || iframe.contentWindow.document;
        doc.open();
        doc.write('<!DOCTYPE html><html><head><title>QR Label</title><style>');
        doc.write('body{margin:0;display:flex;align-items:center;justify-content:center;min-height:100vh;font-family:monospace;background:#fff}');
        doc.write('.box{border:2px solid #1a202c;border-radius:12px;padding:28px 36px;text-align:center}');
        doc.write('img{display:block;width:200px;height:200px;margin:0 auto 14px}');
        doc.write('.code{font-size:18px;font-weight:800;letter-spacing:2px;color:#1a202c;margin-bottom:5px}');
        doc.write('.name{font-size:13px;color:#4a5568}');
        doc.write('</style></head><body>');
        doc.write('<div class="box">');
        doc.write('<img src="' + url + '">');
        doc.write('<div class="code">' + qrCode + '</div>');
        doc.write('<div class="name">' + assetName + '</div>');
        doc.write('</div></body></html>');
        doc.close();
        iframe.contentWindow.focus();
        iframe.contentWindow.print();
        setTimeout(function () { document.body.removeChild(iframe); }, 3000);
    });
}
