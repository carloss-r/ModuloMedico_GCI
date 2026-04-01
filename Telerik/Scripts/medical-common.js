// medical-common.js
// Utility functions and shared UI components

function showError(msg) {
    $('#msgIcon').html('<i class="fas fa-times-circle" style="color: #e74c3c;"></i>');
    $('#msgTitle').text('Error');
    $('#msgBody').text(msg);
    $('#btnMsgOk').css('background', '#e74c3c');
    $('#msgOverlay').css('display', 'flex');
}

function showInfo(msg) {
    $('#msgIcon').html('<i class="fas fa-info-circle" style="color: #3498db;"></i>');
    $('#msgTitle').text('Informaci\u00f3n');
    $('#msgBody').text(msg);
    $('#btnMsgOk').css('background', '#3498db');
    $('#msgOverlay').css('display', 'flex');
}

function showToast(msg, type) {
    var icon = 'fa-info-circle';
    var color = '#3498db';
    if(type === 'success') { icon = 'fa-check-circle'; color = '#27ae60'; }
    if(type === 'error') { icon = 'fa-times-circle'; color = '#e74c3c'; }
    
    var toastHtml = `
    <div id="medicalToast" style="position:fixed; top:20px; right:20px; z-index:9999; background:white; border-left:5px solid ${color}; padding:15px 20px; border-radius:4px; box-shadow:0 4px 12px rgba(0,0,0,0.15); display:none; align-items:center; min-width:300px; max-width:450px;">
        <i class="fas ${icon}" style="color:${color}; font-size:1.5rem; margin-right:15px;"></i>
        <div style="flex:1;">
            <div style="font-weight:700; color:#333; margin-bottom:2px; font-size:0.9rem;">${type === 'success' ? '\u00c9XITO' : (type === 'error' ? 'ERROR' : 'EXPEDIENTE CL\u00cdNICO')}</div>
            <div style="color:#666; font-size:0.85rem; line-height:1.3;">${msg}</div>
        </div>
        <button onclick="$(this).parent().fadeOut()" style="background:none; border:none; color:#ccc; cursor:pointer; font-size:1.2rem; margin-left:10px;">&times;</button>
    </div>`;
    
    $('#medicalToast').remove();
    $('body').append(toastHtml);
    $('#medicalToast').fadeIn(300);
    
    // Auto-hide after 8 seconds
    setTimeout(function() {
        $('#medicalToast').fadeOut(500);
    }, 8000);
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

function formatDateForInput(dateVal) {
    if (!dateVal) return "";
    
    try {
        // Handle /Date(1234567890)/
        if (typeof dateVal === 'string' && dateVal.indexOf('/Date(') !== -1) {
            var match = dateVal.match(/\d+/);
            if (match) {
                var timestamp = parseInt(match[0]);
                var date = new Date(timestamp);
                var year = date.getFullYear();
                var month = ('0' + (date.getMonth() + 1)).slice(-2);
                var day = ('0' + date.getDate()).slice(-2);
                return year + '-' + month + '-' + day;
            }
        }
        
        // Handle ISO or yyyy-mm-ddT...
        if (typeof dateVal === 'string' && dateVal.indexOf('T') !== -1) {
            return dateVal.split('T')[0];
        }
        
        // Handle dd/mm/yyyy or yyyy/mm/dd
        if (typeof dateVal === 'string' && dateVal.indexOf('/') !== -1) {
            var parts = dateVal.split('/');
            if (parts.length === 3) {
                if (parts[0].length === 4) return parts[0] + '-' + parts[1] + '-' + parts[2]; // yyyy/mm/dd
                if (parts[2].length === 4) return parts[2] + '-' + ('0' + parts[1]).slice(-2) + '-' + ('0' + parts[0]).slice(-2); // dd/mm/yyyy
            }
        }

        // If already yyyy-mm-dd
        if (typeof dateVal === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(dateVal)) {
            return dateVal;
        }

        return dateVal; // Return as is if format isn't recognized
    } catch (e) {
        console.error("Format date error:", e, dateVal);
        return "";
    }
}

function clearPanelErrors(panelId) {
    $('#panel' + panelId + ' .val-msg').remove();
    $('#panel' + panelId + ' .form-control').css({ 'border-color': '', 'background': '' });
}

// Input mask and formatting logic
function initInputFormatters() {
    // Numbers only
    $(document).on('input', '.val-num', function() { 
        this.value = this.value.replace(/[^0-9]/g, ''); 
    });
    
    // Decimals
    $(document).on('input', '.val-dec', function() { 
        var start = this.selectionStart;
        var oldVal = this.value;
        var newVal = oldVal.replace(/[^0-9.]/g, '').replace(/(\..*?)\..*/g, '$1'); 
        
        if (oldVal !== newVal) {
            this.value = newVal;
            if (this.setSelectionRange) {
                var diff = oldVal.length - newVal.length;
                this.setSelectionRange(start - diff, start - diff);
            }
        }
    });

    // Text only (with spaces and accents)
    $(document).on('input', '.val-text', function() { 
        this.value = this.value.replace(/[^a-zA-Z\u00C0-\u017F\s.]/g, ''); 
    });

    $(document).on('input', '.val-slash-num', function() {
        this.value = this.value.replace(/[^0-9/]/g, '');
    });
}
