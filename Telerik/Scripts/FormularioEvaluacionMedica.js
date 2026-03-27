// FormularioEvaluacionMedica.js
// Entry point and orchestration for the Medical Evaluation Form

var cambiosSinGuardar = false;

$(document).ready(function() {
    // Detect changes to warn before leaving
    $('input, select, textarea').on('change input', function() {
        cambiosSinGuardar = true;
    });

    window.onbeforeunload = function (e) {
        if (cambiosSinGuardar) {
            var message = 'Tiene cambios sin guardar. Si sale de la página, los datos se perderán.';
            e.returnValue = message;
            return message;
        }
    };

    // Initialize formatting and input masks
    initInputFormatters();

    // Initialize dynamic tables
    initForms();

    // Load initial patient data (idOrden is injected in the view)
    loadPatientData(idOrden);

    // Contextual UI behavior
    $('#txtFechaNacimiento').on('change', function() {
        calcularEdad();
    });

    $('#ddlEscolaridad').on('change', function() {
        var val = $(this).val();
        var show = (val === 'Universidad' || val === 'Posgrado');
        if (!show) $('#txtProfesion').val('');
        $('#txtProfesion').prop('disabled', !show);
    });

    // Antecedentes Checkboxes toggle detalles
    $('#tbAntecedentesHF, #tbAntecedentesPP').on('change', '.chk-ant', function() {
        var $det = $(this).closest('tr').find('.ant-det');
        if(this.checked) {
            $det.prop('disabled', false).removeClass('blocked');
        } else {
            $det.val('').prop('disabled', true).addClass('blocked');
        }
    });

    $('#ddlSexo').change(function() {
        setSexoDisplay($(this).val());
    });
});