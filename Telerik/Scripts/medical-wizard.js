// medical-wizard.js
// Handles navigation and step validation

var currentStep = 1;
var totalSteps = 6;
var currentSexo = '';

function goToStep(step) {
    $('.step-panel').removeClass('active');
    $('#panel' + step).addClass('active');
    
    $('.wizard-step').removeClass('active');
    $('#step' + step).addClass('active');

    currentStep = step;
    updateButtons();
    window.scrollTo(0, 0);
}

function nextStep() {
    if (!validateStep(currentStep)) return; 
    if(currentStep < totalSteps) goToStep(currentStep + 1);
}

function prevStep() {
    if(currentStep > 1) goToStep(currentStep - 1);
}

function updateButtons() {
    $('#btnPrev').css('visibility', currentStep === 1 ? 'hidden' : 'visible');
    if(currentStep === totalSteps) {
        $('#btnNext').hide();
        $('#btnFinish').show();
    } else {
        $('#btnNext').show();
        $('#btnFinish').hide();
    }
}

function setSexoDisplay(sexo) {
    if (!sexo) {
        $('#secGineco, #formFem, #secGenito, #formMasc').hide();
        $('#msgSexoPendiente').show();
        currentSexo = '';
        return;
    }

    $('#msgSexoPendiente').hide();
    var s = sexo.toString().toUpperCase().trim();
    
    if (s === 'F' || s.indexOf('FEM') === 0 || s === 'MUJER' || s === '2') {
        currentSexo = 'F';
        $('#secGineco, #formFem').show();
        $('#secGenito, #formMasc').hide();
    } else if (s === 'M' || s.indexOf('MAS') === 0 || s === 'H' || s === 'HOMBRE' || s === '1') {
        currentSexo = 'M';
        $('#secGineco, #formFem').hide();
        $('#secGenito, #formMasc').show();
    } else {
        currentSexo = s;
        $('#secGineco, #formFem, #secGenito, #formMasc').hide();
        $('#msgSexoPendiente').show();
    }
}

function validateStep(step) {
    clearPanelErrors(step);
    var ok = true;

    if (step === 1) {
        var $nom = $('#txtNombre');
        if (!$nom.val().trim()) { markError($nom, 'El nombre es obligatorio.'); ok = false; }

        var $fec = $('#txtFechaNacimiento');
        if (!$fec.val()) { markError($fec, 'Ingrese la fecha de nacimiento.'); ok = false; }

        var $ec = $('#ddlEstadoCivil');
        if (!$ec.val()) { markError($ec, 'Seleccione el estado civil.'); ok = false; }

        var $sx = $('#ddlSexo');
        if (!$sx.val()) { markError($sx, 'Seleccione el sexo.'); ok = false; }

        var nss = $('#txtNss').val().trim();
        if (nss && !(/^\d{11}$/.test(nss))) {
            markError($('#txtNss'), 'El No. IMSS debe tener exactamente 11 dígitos numéricos.');
            ok = false;
        }

        var tel = $('#txtTelefono').val().trim();
        if (tel && !(/^\d{10,15}$/.test(tel))) {
            markError($('#txtTelefono'), 'El teléfono debe ser numérico (10-15 dígitos).');
            ok = false;
        }

        var $lug = $('#txtLugarEvaluacion');
        if (!$lug.val().trim()) { markError($lug, 'Ingrese el lugar de evaluación.'); ok = false; }
    }

    if (step === 3) {
        if ($('#chkFuma').is(':checked')) {
            var anos = $('#txtAnosFuma').val().trim();
            if (anos && !isOnlyNumbers(anos)) {
                markError($('#txtAnosFuma'), 'Solo se permiten números.');
                ok = false;
            }
            var cig = $('#txtCigarrillos').val().trim();
            if (cig && !isOnlyNumbers(cig)) {
                markError($('#txtCigarrillos'), 'Solo se permiten números.');
                ok = false;
            }
        }
    }

    if (step === 4) {
        var vitals = [
            { id: '#txtSistolica',       label: 'TA Sistólica',    type: 'num' },
            { id: '#txtDiastolica',      label: 'TA Diastólica',   type: 'num' },
            { id: '#txtFrecCardiaca',    label: 'FC',               type: 'num' },
            { id: '#txtFrecRespiratoria',label: 'FR',               type: 'num' },
            { id: '#txtPeso',            label: 'Peso',             type: 'dec' },
            { id: '#txtEstatura',        label: 'Estatura',         type: 'dec' }
        ];
        vitals.forEach(function(v) {
            var $el = $(v.id);
            var val = $el.val().trim();
            if (!val) {
                markError($el, v.label + ' es obligatorio.');
                ok = false;
            } else if (v.type === 'num' && !isOnlyNumbers(val)) {
                markError($el, 'Solo se permiten números enteros.');
                ok = false;
            } else if (v.type === 'dec' && !isDecimal(val)) {
                markError($el, 'Solo se permiten números (use punto decimal).');
                ok = false;
            }
        });

        var temp = $('#txtTemperatura').val().trim();
        if (temp && !isDecimal(temp)) {
            markError($('#txtTemperatura'), 'Solo se permiten números decimales (ej. 36.5).');
            ok = false;
        }
    }

    if (step === 5) {
        if (currentSexo === 'F') {
            var menarca = $('#txtMenarca').val().trim();
            if (menarca && !isOnlyNumbers(menarca)) {
                markError($('#txtMenarca'), 'Solo se permiten números.');
                ok = false;
            }
            var ivsaF = $('#txtIvsaFem').val().trim();
            if (ivsaF && !isOnlyNumbers(ivsaF)) {
                markError($('#txtIvsaFem'), 'Solo se permiten números.');
                ok = false;
            }
            ['#txtGestas','#txtPartos','#txtAbortos','#txtCesareas'].forEach(function(id) {
                var v = $(id).val().trim();
                if (v && !isOnlyNumbers(v)) {
                    markError($(id), 'Solo se permiten números.');
                    ok = false;
                }
            });
        }
        if (currentSexo === 'M') {
            var ivsaM = $('#txtIvsaMasc').val().trim();
            if (ivsaM && !isOnlyNumbers(ivsaM)) {
                markError($('#txtIvsaMasc'), 'Solo se permiten números.');
                ok = false;
            }
        }
    }

    if (step === 6) {
        var $apt = $('#ddlAptitud');
        if (!$apt.val()) { markError($apt, 'Debe seleccionar el RESULTADO.'); ok = false; }
    }

    if (!ok) {
        var $first = $('#panel' + step + ' .val-msg').first();
        if ($first.length) {
            $('html, body').animate({ scrollTop: $first.offset().top - 120 }, 300);
        }
    }
    return ok;
}
