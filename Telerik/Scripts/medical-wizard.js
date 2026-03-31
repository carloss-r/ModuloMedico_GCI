// medical-wizard.js
// Handles navigation and step validation

var currentStep = 1;
var totalSteps = 6;
var currentSexo = '';

function goToStep(step) {
    // If moving forward, must validate current step
    if (step > currentStep) {
        // Validate all steps between current and target
        for (var s = currentStep; s < step; s++) {
            if (!validateStep(s)) {
                // Focus on step indicator if navigation blocked
                showError("Debe completar correctamente todos los campos obligatorios del paso " + s + " antes de continuar.");
                return;
            }
        }
    }

    $('.step-panel').removeClass('active');
    $('#panel' + step).addClass('active');
    
    $('.wizard-step').removeClass('active');
    $('#step' + step).addClass('active');

    currentStep = step;
    updateButtons();
    window.scrollTo(0, 0);
}

function nextStep() {
    if (!validateStep(currentStep)) {
        showError("Por favor revise los campos marcados en rojo.");
        return;
    } 
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

    // Helper to check mandatory text
    function checkReq($el, msg) {
        if (!$el.val() || !$el.val().trim()) {
            markError($el, msg || 'Campo obligatorio');
            ok = false;
        }
    }

    if (step === 1) {
        checkReq($('#txtLugarEvaluacion'), 'Ingrese el lugar de la evaluación');
        checkReq($('#txtFechaExamen'), 'Ingrese la fecha');
        checkReq($('#txtNombre'), 'Nombre es obligatorio');
        checkReq($('#txtApellidoPaterno'), 'Apellido paterno obligatorio');
        checkReq($('#txtNss'), 'No. IMSS obligatorio');
        checkReq($('#ddlEstadoNacimiento'), 'Seleccione el estado de nacimiento');
        checkReq($('#txtFechaNacimiento'), 'Ingrese fecha de nacimiento');
        checkReq($('#ddlEstadoCivil'), 'Seleccione estado civil');
        checkReq($('#ddlManoDominante'), 'Seleccione mano dominante');
        checkReq($('#txtTelefono'), 'Teléfono obligatorio');
        checkReq($('#ddlPais'), 'Seleccione país');
        checkReq($('#ddlEstado'), 'Seleccione estado');
        checkReq($('#ddlMunicipio'), 'Seleccione municipio');
        checkReq($('#ddlColonia'), 'Seleccione colonia');
        checkReq($('#ddlEscolaridad'), 'Seleccione escolaridad');
        if (!$('#txtProfesion').is(':disabled')) {
            checkReq($('#txtProfesion'), 'Indique profesión u oficio');
        }
        checkReq($('#ddlSexo'), 'Sexo obligatorio');
        checkReq($('#ddlTipoSangre'), 'Seleccione tipo de sangre');

        var nss = $('#txtNss').val().trim();
        if (nss && nss.length !== 11) {
            markError($('#txtNss'), 'El No. IMSS debe tener 11 dígitos');
            ok = false;
        }
    }

    if (step === 2) {
        // En antecedentes, al menos las alergias/observaciones deberían tener algo o "Negado"
        checkReq($('#txtAlergias'), 'Indique alergias u "Observaciones negadas"');
    }

    if (step === 3) {
        if ($('#chkFuma').is(':checked')) {
            checkReq($('#txtAnosFuma'), 'Indique años');
            checkReq($('#txtCigarrillos'), 'Indique cigarros/día');
        }
        if ($('#chkDrogas').is(':checked')) {
            checkReq($('#txtTipoDrogas'), 'Especifique tipo de droga');
        }
        if ($('#chkAlcohol').is(':checked')) {
            checkReq($('#txtFrecAlcohol'), 'Seleccione frecuencia');
        }
    }

    if (step === 4) {
        var vitals = [
            { id: '#txtSistolica',       label: 'Sistólica' },
            { id: '#txtDiastolica',      label: 'Diastólica' },
            { id: '#txtFrecCardiaca',    label: 'FC' },
            { id: '#txtFrecRespiratoria',label: 'FR' },
            { id: '#txtPeso',            label: 'Peso' },
            { id: '#txtEstatura',        label: 'Estatura' },
            { id: '#txtTemperatura',     label: 'Temperatura' },
            { id: '#txtGlucosa',         label: 'Glucosa' },
            { id: '#txtOximetria',       label: 'Oximetría' }
        ];
        
        vitals.forEach(function(v) {
            checkReq($(v.id), v.label + ' obligatorio');
        });

        // Agudeza Visual
        checkReq($('#ddlOdSinLentes'), 'Obligatorio');
        checkReq($('#ddlOiSinLentes'), 'Obligatorio');
        checkReq($('#ddlAoSinLentes'), 'Obligatorio');
        checkReq($('#ddlUsaLentes'), 'Obligatorio');
    }

    if (step === 5) {
        if (currentSexo === 'F') {
            checkReq($('#txtMenarca'), 'Edad menarca obligatoria');
            checkReq($('#txtCiclos'), 'Seleccione ciclo');
            checkReq($('#txtFum'), 'Fecha FUM obligatoria');
            checkReq($('#txtIvsaFem'), 'IVSA obligatorio (0 si no aplica)');
        }
        if (currentSexo === 'M') {
            checkReq($('#txtIvsaMasc'), 'IVSA obligatorio (0 si no aplica)');
        }
    }

    if (step === 6) {
        checkReq($('#txtDiagnostico'), 'Diagnóstico obligatorio');
        checkReq($('#ddlAptitud'), 'Resultado obligatorio');
    }

    if (!ok) {
        var $first = $('#panel' + step + ' .val-msg').first();
        if ($first.length) {
            $('html, body').animate({ scrollTop: $first.offset().top - 120 }, 300);
        }
    }
    return ok;
}
