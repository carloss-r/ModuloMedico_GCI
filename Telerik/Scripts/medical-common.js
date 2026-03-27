// medical-common.js
// Utility functions and shared UI components

function showError(msg) {
    $('#msgIcon').html('<i class="fas fa-times-circle" style="color: #e74c3c;"></i>');
    $('#msgTitle').text('Error');
    $('#msgBody').text(msg);
    $('#btnMsgOk').css('background', '#e74c3c');
    $('#msgOverlay').css('display', 'flex');
}

function showSuccess(msg, callback) {
    $('#msgIcon').html('<i class="fas fa-check-circle" style="color: #27ae60;"></i>');
    $('#msgTitle').text('\u00c9xito');
    $('#msgBody').text(msg);
    $('#btnMsgOk').css('background', '#27ae60').off('click').click(function() {
        $('#msgOverlay').hide();
        if(callback) callback();
    });
    $('#msgOverlay').css('display', 'flex');
}

var confirmCallback = null;
function showConfirm(msg, callback) {
    $('#confirmBody').text(msg);
    confirmCallback = callback;
    $('#confirmOverlay').css('display', 'flex');
}

function handleConfirm(result) {
    $('#confirmOverlay').hide();
    if(confirmCallback) confirmCallback(result);
}

function calcImc() {
    var w = parseFloat($('#txtPeso').val());
    var h = parseFloat($('#txtEstatura').val());
    if(w > 0 && h > 0) {
        var imc = w / (h * h);
        $('#txtImc').val(imc.toFixed(1));
        
        // Determinar Clasificación (Escala OMS)
        var desc = "";
        var color = "#2c3e50"; // Default
        
        if (imc < 18.5) {
            desc = "Bajo peso";
            color = "#3498db"; // Azul
        } else if (imc >= 18.5 && imc < 25) {
            desc = "Normal";
            color = "#27ae60"; // Verde
        } else if (imc >= 25 && imc < 30) {
            desc = "Sobrepeso";
            color = "#f39c12"; // Naranja
        } else if (imc >= 30 && imc < 35) {
            desc = "Obesidad Grado I";
            color = "#e67e22"; // Naranja Fuerte
        } else if (imc >= 35 && imc < 40) {
            desc = "Obesidad Grado II";
            color = "#d35400"; // Rojo Intenso
        } else if (imc >= 40) {
            desc = "Obesidad Grado III (M\u00f3rbida)";
            color = "#c0392b"; // Rojo Oscuro
        }
        
        $('#txtImcDescripcion').val(desc.toUpperCase()).css('color', color);
    } else {
        $('#txtImc, #txtImcDescripcion').val('');
    }
}

function calcularEdad() {
    var fNacStr = $('#txtFechaNacimiento').val();
    if(fNacStr) {
        var fNac = new Date(fNacStr);
        var hoy = new Date();
        var edad = hoy.getFullYear() - fNac.getFullYear();
        var m = hoy.getMonth() - fNac.getMonth();
        if (m < 0 || (m === 0 && hoy.getDate() < fNac.getDate())) {
            edad--;
        }
        $('#txtEdad').val(edad);
    } else {
        $('#txtEdad').val('');
    }
}

// Validation Helpers
function markError($el, msg) {
    $el.css({ 'border-color': '#e74c3c', 'background': '#fff8f8' });
    if (!$el.next('.val-msg').length) {
        $el.after('<span class="val-msg" style="color:#e74c3c; font-size:0.75rem; margin-top:3px; display:block;">' + msg + '</span>');
    }
}

function clearError($el) {
    $el.css({ 'border-color': '', 'background': '' });
    $el.next('.val-msg').remove();
}

function isOnlyLetters(val) { return /^[a-zA-ZÀ-ÿ\s]+$/.test(val); }
function isOnlyNumbers(val) { return /^\d+$/.test(val); }
function isDecimal(val)     { return /^\d+(\.\d+)?$/.test(val); }

function clearPanelErrors(panelId) {
    $('#panel' + panelId + ' .val-msg').remove();
    $('#panel' + panelId + ' .form-control').css({ 'border-color': '', 'background': '' });
}

// Input mask and formatting logic
function initInputFormatters() {
    // Numbers only
    $('.val-num').on('input', function() { 
        this.value = this.value.replace(/[^0-9]/g, ''); 
    });
    
    // Decimals (Cursor aware)
    $('.val-dec').on('input', function() { 
        var start = this.selectionStart;
        var oldVal = this.value;
        var newVal = oldVal.replace(/[^0-9.]/g, '').replace(/(\..*?)\..*/g, '$1'); 
        
        if (oldVal !== newVal) {
            this.value = newVal;
            if (this.type === 'text' || this.type === 'search') {
                var diff = oldVal.length - newVal.length;
                var newPos = start - diff;
                if (newPos < 0) newPos = 0;
                this.setSelectionRange(newPos, newPos);
            }
        }
    });

    $('.val-text').on('input', function() { 
        this.value = this.value.replace(/[^a-zA-Z\u00C0-\u017F\s]/g, ''); 
    });

    $('.val-slash-num').on('input', function() {
        this.value = this.value.replace(/[^0-9/]/g, '');
    });
}
