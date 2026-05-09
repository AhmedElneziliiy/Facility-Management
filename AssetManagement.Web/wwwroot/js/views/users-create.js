function togglePw() {
    var inp = document.getElementById('pwInput');
    var ico = document.getElementById('pwIcon');
    if (inp.type === 'password') { inp.type = 'text';     ico.className = 'bi bi-eye-slash'; }
    else                         { inp.type = 'password'; ico.className = 'bi bi-eye'; }
}
